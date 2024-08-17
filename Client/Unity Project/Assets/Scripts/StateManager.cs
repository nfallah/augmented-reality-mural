using System;
using UnityEngine;
using UnityEngine.Events; 
using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using MixedReality.Toolkit.Subsystems;
using MixedReality.Toolkit.SpatialManipulation;
using Microsoft.MixedReality.GraphicsTools;

public class StateManager : MonoBehaviour
{
    public static StateManager Instance;

    public GameObject selectedObj;

    public Vector3 constantscale = new Vector3(1, 1, 1); // Set the desired scale in the Inspector

    /* When utilizing this enum for inspector events, use the integer equivalent as shown below. */
    public enum Mode
    {
        BRUSH,  // 0
        SHAPE,  // 1
        TEXT,   // 2
        SELECT, // 3
        ERASE,  // 4
        LINE    // 5
    }

    [SerializeField]
    private Mode defaultMode;

    public Mode prevMode, currMode;

    private void Awake()
    {
        // Enforce a singleton state pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }
    }

    private void Start()
    {
        currMode = defaultMode;
        EnableMode(defaultMode);
        selectedObj=null;
        //BrushRenderer.DrawClick(BrushRenderer.Instance.obj);
        //clickHandler = FindObjectOfType<ClickHandler>();
        //erasing();
    }

    public void SwitchMode(int _newMode)
    {
        // Ensure the integer is not out of bounds for our enum values.
        if (_newMode < 0 || _newMode >= Enum.GetValues(typeof(Mode)).Length)
        {
            throw new Exception("StateManager --> SwitchMode: out of bounds");
        }

        Mode newMode = (Mode)_newMode;

        // Ensure the new mode is not simply the current one
        if (newMode == currMode)
        {
            return;
        }

        prevMode = currMode;
        currMode = newMode;
        
        // The order of disabling the previous mode before enabling the current mode matters!
        DisableMode(prevMode);
        EnableMode(currMode);
    }

    private void DisableMode(Mode oldMode)
    {
        switch (oldMode)
        {
            case Mode.BRUSH:
                BrushTool.Instance.Deactivate();
                break;
            case Mode.SHAPE:
                ShapeTool.Instance.Deactivate();
                break;
            case Mode.TEXT:
                TextTool.Instance.Deactivate();
                break;
            case Mode.SELECT:
                ButtonManager.Instance.disableLineSelectButtons();
                SelectTool.Instance.Deactivate();
                break;
            case Mode.ERASE:
                EraseTool.Instance.Deactivate();
                break;
            case Mode.LINE:
                LineTool.Instance.Deactivate();
                break;
            default:
                throw new Exception("StateManager --> DisableMode: enum not implemented");
        }
    }

    public void EnableMode(Mode newMode)
    {
        switch (newMode)
        {
            case Mode.BRUSH:
                BrushTool.Instance.Activate();
                ButtonManager.Instance.disableInputField();
                break;
            case Mode.SHAPE:
                ShapeTool.Instance.Activate();
                ButtonManager.Instance.disableInputField();
                break;
            case Mode.TEXT:
                TextTool.Instance.Activate();
                break;
            case Mode.SELECT:
                SelectTool.Instance.Activate();
                ButtonManager.Instance.disableInputField();
                break;
            case Mode.ERASE:
                EraseTool.Instance.Activate();
                ButtonManager.Instance.disableInputField();
                break;
            case Mode.LINE:
                LineTool.Instance.Activate();
                ButtonManager.Instance.disableInputField();
                break;
            default:
                throw new Exception("StateManager --> EnableMode: enum not implemented");
        }
    }

    //depending on the mode, have different things happen to the selected drawing 
    public void DrawClick(GameObject obj)
	{
        //obj is the current object
//        Debug.Log("obj clicked: " + obj);
    	switch (currMode)
    	{
        	case Mode.SELECT:
                if (SelectTool.Instance.enabled == true || ButtonManager.Instance.multipleLineSelectButtons.activeInHierarchy || ButtonManager.Instance.isMultiple==true)
                {
                    return;
                }
                ButtonManager.Instance.isMultiple=false;
                //Debug.Log("ISMULTIPLE IS FALSE (FROM DRAWCLICK)");
                //Debug.Log(obj.transform.position);
                // Show UI buttons for that specific drawing
                  // EX: locked status, backplate color
                //create an offset to make the select button be slightly above the target line
                //Vector3 laserOffset = new Vector3(0, .08f, 0);
                //TODO: use offsetDir to move a bit further from the hit position (though it can be null if no collision was detected!)
                (Vector3? laserOffset, Vector3? offsetDir) = MathUtils.GetMenuSelectPos(ButtonManager.Instance.cameraPos.position, obj);
                // If no collision happened (laserOffset && offsetDir == null), just default to the center of the gameobject.
                laserOffset ??= obj.transform.position;
                if (offsetDir != null)
                {
                    //Debug.Log("why?");
                    laserOffset += offsetDir * 0.08f;   
                }
                ButtonManager.Instance.lineSelectButtons.SetActive(true);
                //ButtonManager.Instance.multipleLineSelectButtons.SetActive(true);
               // Debug.Log("multiple select buttons active");
                //ButtonManager.Instance.lineSelectButtons.transform.position=obj.transform.position+laserOffset;
                ButtonManager.Instance.lineSelectButtons.transform.position = laserOffset.Value;
                //also have the menu face the user
                Vector3 dir = (ButtonManager.Instance.lineSelectButtons.transform.position - Camera.main.transform.position).normalized;
        	    Quaternion lookAtUser = Quaternion.LookRotation(dir);
                // Global rotation over localRotation!
        	    ButtonManager.Instance.lineSelectButtons.transform.rotation = lookAtUser;
                // At this point, exit prematurely if we simply re-selected the same thing.
                if (obj == selectedObj)
                {
                    return;
                }

                // Debug.Log("selected at location " + obj.transform.position);
                //if selectedobj != null then ensure its stateful is enabled and the object manipulator is disabled
                if (selectedObj!=null && selectedObj.GetComponent<StatefulInteractable>().enabled == false){
                    // Once again, order of disabling first before enabling greatly matters!
                    selectedObj.GetComponent<ObjectManipulator>().enabled=false;
                    selectedObj.GetComponent<StatefulInteractable>().enabled=true;
                }

                // If shape, unparent from pivot and delete pivot.
                if (selectedObj != null && selectedObj.layer == 9 && selectedObj.transform.childCount > 0)
                {
                    Transform pivot = selectedObj.transform.GetChild(0);
                    // Unpivot first or else the menu will be deleted with the pivot.
                    ButtonManager.Instance.lineSelectButtons.transform.SetParent(selectedObj.transform);
                    Destroy(pivot.gameObject);
                }

                //disable the yellow outline of the object
                if (selectedObj != null)
                {
                    //if the gameobject is text, then disable the border around it to be highlighted
                    if (selectedObj.layer == 10)
                    {
                        selectedObj.transform.GetChild(0).gameObject.SetActive(false);
                    }
                    //if it's not text, then just disable the gameobject's mesh outline
                    else
                    {
                        selectedObj.GetComponent<MeshOutline>().enabled = false;
                    }
                }

                // Reset the button to its default state (e.g. move toggle state, etc.)
                selectedObj = obj;
                ButtonManager.Instance.resetSelectMenu();

                //change the color of the lineselectbuttons to be the color of the object it's talking about
                ButtonManager.Instance.changeColorOfLineSelectButtons();
                
                //store the object selected by the user into the global variable selectedObj in order to be able to delete that object using the "delete" button
                //ButtonManager.Instance.lineSelectButtons.transform.SetParent(selectedObj.transform);

                // If we chose a shape, scale menu so that it doesn't shrink/stretch.
                Vector3 scale = obj.transform.localScale;
                // No divsion by zero!
                if (obj.layer == 9 && scale.x != 0 && scale.y != 0 && scale.z != 0)
                {
                    Vector3 newScale = new Vector3(
                        1 / scale.x,
                        1 / scale.y,
                        1 / scale.z);

                    GameObject pivot = new GameObject("Pivot");
                    pivot.transform.SetParent(obj.transform);
                    pivot.transform.localPosition = Vector3.zero;
                    pivot.transform.rotation=Quaternion.Euler(0, 0, 0);//this attempts to prevent the line select buttons distortion but it does not. rotation stays (0,0,0) for the pivot
                    pivot.transform.localScale = newScale;
                    ButtonManager.Instance.lineSelectButtons.transform.SetParent(pivot.transform);
                }
                else
                {
                    ButtonManager.Instance.lineSelectButtons.transform.SetParent(selectedObj.transform);
                }
                if (selectedObj.layer == 10)
                {
                    Instance.selectedObj.transform.GetChild(0).gameObject.SetActive(true);
                }
                else
                {
                    selectedObj.GetComponent<MeshOutline>().enabled = true;
                }
                break;
        	case Mode.ERASE:
            //when you click on a gameobject, it deletes it
                ButtonManager.Instance.isMultiple=false;
                Debug.Log("ISMULTIPLE IS FALSE (FROM ERASE MODE)");
                ButtonManager.Instance.lineSelectButtons.SetActive(false);
               // Destroy(obj);
               // If connected don't destroy and let server confirm we could.
                if (ClientManager.Instance.connected)
                {
                    DeleteContainer deleteCommand = new DeleteContainer(obj.GetComponent<IDContainer>().id);
                    ClientManager.Instance.SendCommand(new Command(deleteCommand, JustMonika.Instance.GetChunk(obj.transform.position)));
                }
                else
                {
                    Destroy(obj);
                }
               // Debug.Log(obj.name + " erased");
            	break;
        	default:
                ButtonManager.Instance.isMultiple=false;
                //Debug.Log("ISMULTIPLE IS FALSE (FROM NOT ERASE OR SELECT MODE)");
            	return;
    	}
	} 
 }