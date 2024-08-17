using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;
using UnityEngine.Events; 
using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using MixedReality.Toolkit.Subsystems;
using MixedReality.Toolkit.SpatialManipulation;
using Microsoft.MixedReality.GraphicsTools;

public class SelectTool : MonoBehaviour
{
    public static SelectTool Instance { get; private set; }

    // Visually renders the current volume of the user's selection.
    [SerializeField]
    public SelectionBox selectionBox;

    public bool activated, isLeftHand;

    Vector3 currPos, lastPos;

    public List<GameObject> selectedObjects = new List<GameObject>();

    public bool isMix;

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

        if (enabled == true)
        {
            enabled = false;
        }
    }

    private void Start(){
        //nothing is multiselected yet
        selectedObjects=null;
        //by default, dont have the brush color be the average color generated by multiselect 
    }

    private void Update()
    {
        currPos = isLeftHand ? PinchManager.Instance.LeftPosition :
                               PinchManager.Instance.RightPosition;
        selectionBox.UpdatePos(currPos, lastPos);
    }

    private void StartTool(bool isLeftHand)
    {
        //if some stuff is multiselected, then don't allow the selectionbox
        if (StateManager.Instance.selectedObj != null || ButtonManager.Instance.multipleLineSelectButtons.activeInHierarchy)
        {
            return;
        }

        if (enabled == true)
        {
            return;
        }
        enabled = true;
        this.isLeftHand = isLeftHand;
        lastPos = isLeftHand ? PinchManager.Instance.LeftPosition :
                               PinchManager.Instance.RightPosition;
        selectionBox.Activate();
    }

    public void setUpMultiSelect(List<GameObject> targetObjects){
        //if there were things selected before, un-outline them
        if (selectedObjects!=null){
        foreach (GameObject g in selectedObjects){
             //disable the yellow outline of the object
                if (g != null)
                {
                    //if the gameobject is text, then disable the border around it to be highlighted
                    if (g.layer == 10)
                    {
                        g.transform.GetChild(0).gameObject.SetActive(false);
                    }
                    //if it's not text, then just disable the gameobject's mesh outline
                    else
                    {
                        g.GetComponent<MeshOutline>().enabled = false;
                    }
                }
        } 
    }
    //the global variable selectedobjects now points to targetobjects
    selectedObjects=targetObjects;
    ButtonManager.Instance.resetmultiSelectMenu();
    Color c = AverageColor(selectedObjects);
    //if the button colormix is toggled, then make the brush color the average color
    if(isMix){
        //change the color of the brush and menus to be the average color of the selected objects
        ButtonManager.Instance.changecolor("#" + ColorUtility.ToHtmlStringRGB(c));
        //turn off the colormixer button
        ButtonManager.Instance.colorMixerButton.ForceSetToggled(false);
    }
    foreach (GameObject g in selectedObjects){
             //enable the yellow outline of the object
                if (g != null)
                {
                    //set parent of each object in selectedObjects to the multilineselectbuttons
                    //g.transform.transform.SetParent(ButtonManager.Instance.multipleLineSelectButtons.transform);
                    //if the gameobject is text, then enable the border around it to be highlighted
                    if (g.layer == 10)
                    {
                        g.transform.GetChild(0).gameObject.SetActive(true);
                    }
                    //if it's not text, then just enable the gameobject's mesh outline
                    else
                    {
                        g.GetComponent<MeshOutline>().enabled = true;
                    }
                }
        } 
        // Reset the button to its default state (e.g. move toggle state, etc.)
        // StateManager.Instance.selectedObj = StateManager.Instance.obj;
        //find nearest object to user
        float mindistance=Vector3.Distance(targetObjects[0].transform.position,Camera.main.transform.position);
        GameObject nearestobject=targetObjects[0];

        for (int i=1 ; i<targetObjects.Count ; i++){
            if(Vector3.Distance(targetObjects[i].transform.position,Camera.main.transform.position) < mindistance){
                mindistance=Vector3.Distance(targetObjects[i].transform.position,Camera.main.transform.position);
                nearestobject=targetObjects[i];
            }
        }
        (Vector3? laserOffset, Vector3? offsetDir) = MathUtils.GetMenuSelectPos(ButtonManager.Instance.cameraPos.position, nearestobject.transform.gameObject);
                // If no collision happened (laserOffset && offsetDir == null), just default to the center of the nearestobject.
                laserOffset ??= nearestobject.transform.position;
                if (offsetDir != null)
                {
                    laserOffset += offsetDir * 0.08f;   
                }
        ButtonManager.Instance.multipleLineSelectButtons.SetActive(true);
        ButtonManager.Instance.multipleLineSelectButtons.transform.position = laserOffset.Value;
        //also have the menu face the user
        Vector3 dir = (ButtonManager.Instance.multipleLineSelectButtons.transform.position - Camera.main.transform.position).normalized;
        Quaternion lookAtUser = Quaternion.LookRotation(dir);
        // Global rotation over localRotation!
        ButtonManager.Instance.multipleLineSelectButtons.transform.rotation = lookAtUser;
    }
//when u unpinch
    public void StopTool(bool isLeftHand)
    {
        if (enabled == false)
        {
            return;
        }

        // Must unpinch from the same hand; exit if this is not the case.
        if (isLeftHand != this.isLeftHand)
        {
            return;
        }
        enabled = false;
        selectionBox.Deactivate();

        /*** Given 'currPos' and 'lastPos', find the set of colliders to enable ***/

        // First, ensure the start and end positions are not identical.
        if (MathUtils.Vector3Equals(currPos, lastPos) == true)
        {
            return;
        }

        // Otherwise, obtain the set of colliders and store their game objects
        Vector3 center = (currPos + lastPos) / 2f;
        Vector3 halfExtents = MathUtils.Vector3Abs(currPos - lastPos) / 2f;
        Collider[] targetColliders = Physics.OverlapBox(center, halfExtents);
        //Debug.Log("Found (" + targetColliders.Length + ")" + " collider" + (targetColliders.Length == 1 ? "" : "s"));

        // We found within our rectangular prism, so exit we prematurely exit.
        if (targetColliders.Length <= 0)
        {
            return;
        }

        List<GameObject> targetObjects = new List<GameObject>();

        for (int i = 0; i < targetColliders.Length; i++)
        {
            GameObject candidateObj = targetColliders[i].gameObject;

            /* The candidate must have one of four valid layers:
             * 7:  brush
             * 8:  line
             * 9:  shape
             * 10: text
             */
            if (candidateObj.layer >= 7 && candidateObj.layer <= 10)
            {
                targetObjects.Add(candidateObj);
            }
        }

        //Debug.Log("Kept (" + targetObjects.Count + ")" + " collider" + (targetObjects.Count == 1 ? "" : "s"));

        // With the obtained game objects, call the relevant function(s) to proceed.
        if (targetObjects.Count == 0)
        {
            return;
        }

        if (targetObjects.Count == 1)
        {
            // "Pretend" that we clicked on a singular object.
            ButtonManager.Instance.isMultiple=false;
            Debug.Log("ISMULTIPLE IS FALSE");
            StateManager.Instance.DrawClick(targetObjects[0]);
            return;
        } else if (targetObjects.Count>1){
            // If at least 2 gameobjects are selected, then offset the multiplelineselectbuttons to be near the nearest object to the user
            //also, calculate the average color of the gameobjects and assign that color to multiplelineselectbuttons
            //if something is selected already, get rid of its outline
            ButtonManager.Instance.isMultiple=true;
            Debug.Log("ISMULTIPLE IS TRUE");
            setUpMultiSelect(targetObjects);  
        }
    }
//switch modes abruptly while drawing
    private void CancelTool()
    {
        if (enabled == false)
        {
            return;
        }

        enabled = false;
        selectionBox.Deactivate();
    }

    public void Activate()
    {
        if (activated == true)
        {
            return;
        }

        activated = true;
        PinchManager.Instance.OnHandPinched += StartTool;
        PinchManager.Instance.OnHandReleased += StopTool;
    }

    public void Deactivate()
    {
        if (activated == false)
        {
            return;
        }

        activated = false;
        PinchManager.Instance.OnHandPinched -= StartTool;
        PinchManager.Instance.OnHandReleased -= StopTool;
        CancelTool();
    }

    public static Color AverageColor(List<GameObject> targetObjects)
    {
        Color color = new Color();

        foreach (GameObject obj in targetObjects)
        {
            if (obj.layer == 10)
            {
                color += obj.GetComponent<TextMeshPro>().color;
            }
            else
            {
                color += obj.GetComponent<Renderer>().material.color;
            }
        }

//        Debug.Log(ColorUtility.ToHtmlStringRGB(color / targetObjects.Count));
        //make the multiplelineselectbuttons the average color
        Color buttonsColor=color/targetObjects.Count;
        buttonsColor.a=1.0f;
        ButtonManager.Instance.multipleLineSelectButtonsRealButtons.GetComponent<Microsoft.MixedReality.GraphicsTools.CanvasElementRoundedRect>().color=
        buttonsColor;
        return buttonsColor;
    }
}