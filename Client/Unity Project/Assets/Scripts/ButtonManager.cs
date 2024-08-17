using UnityEngine;
using UnityEngine.UI;
using MixedReality.Toolkit;
using MixedReality.Toolkit.SpatialManipulation;
using MixedReality.Toolkit.UX;
using Microsoft.MixedReality.GraphicsTools;
using TMPro;
using System.Collections.Generic;
using System;
using UnityEngine.Events; 

//this script is for managing the UI of the buttons. i.e., determining which button menus to show up depending on which buttons
//are pressed

public class ButtonManager : MonoBehaviour
{
    public static ButtonManager Instance;
    
    // added to the popup!
    [SerializeField]
    GameObject startMenu;
    bool isStartMenuEnabled = false;
    
    // added to the popup!
    [SerializeField]
    GameObject submenu;
    bool isSubMenuEnabled = false;
    
    // added to the popup!
    [SerializeField]
    GameObject colorPalette;
    bool isColorPaletteEnabled = false;
    
    // added to the popup!
    [SerializeField]
    GameObject smoothnessSlider;
    bool isSmoothnessSliderEnabled = false;

    // ***** ADD *****
    [SerializeField]
    GameObject loopButton;
    bool isLoopButtonEnabled = false;
    
    // added to the popup!
    [SerializeField]
    GameObject sizeSlider;
    bool isSizeSliderEnabled = false;

    // ***** ADD *****
    [SerializeField]
    GameObject shapeToolMenu;
    bool isShapeToolMenuEnabled = false;

    [SerializeField]
    GameObject shapeToolMenuButtons;
    bool isShapeToolButtonsEnabled = false;

    // ***** ADD *****
    [SerializeField]
    GameObject shapeToolMenuPin;
    bool isShapeToolPinEnabled = false;

    //[SerializeField]
    //GameObject shapeToolButtonsPin;

    [SerializeField]
    public GameObject submenuButtons;
    bool isSubMenuButtonsEnabled = false;

    // ***** ADD *****
    [SerializeField]
    GameObject submenuPin;
    bool isSubMenuPinEnabled = false;

    [SerializeField]
    GameObject startmenuButtons;

    // ***** ADD *****
    [SerializeField]
    GameObject startmenuPin;
    bool isStartmenuPinEnabled = false;

    [SerializeField]
    GameObject colorPaletteButtons;

    // ***** ADD *****
    [SerializeField]
    GameObject colorPalettePin;
    bool isColorPalettePinEnabled = false;

    [SerializeField]
    public GameObject lineSelectButtons;
    bool islineSelectButtonsEnabled = false;

    // what does this do???
    [SerializeField]
    public GameObject lineSelectButtonsRealButtons;

    // ***** ADD *****
    [SerializeField]
    public GameObject deleteButton;
    bool isDeleteButtonEnabled = false;

    // ***** ADD *****
    [SerializeField]
    private PressableButton selectMoveButton;
    private bool isSelectMoveButtonEnabled = false;

    public Transform cameraPos;

    public MRTKUGUIInputField textToolField;

    // ***** ADD *****
    [SerializeField]
    GameObject inputField;

    [SerializeField]
    public GameObject multipleLineSelectButtons; 

    [SerializeField]
    public GameObject multipleLineSelectButtonsRealButtons;

    [SerializeField]
    public PressableButton multiSelectMoveButton;

    [SerializeField]
    GameObject importedMenu;

    [SerializeField]
    public PressableButton colorMixerButton;

    public bool isMultiple;

    //this allows for instances to be made from this file
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

    // when the "game" is started, automatically have only the startmenu open
    void Start()
    {
        enablestartMenu();
        disableSubmenu();
        disableColorPalette();
        disableSmoothnessSlider();
        disableSizeSlider();
        disableLineSelectButtons();
        disableShapeToolMenu();
        disableInputField();
        disableMultipleSelect(false);
        disableImportedMenu();
        SelectTool.Instance.isMix=false;
        //default color is red for the menu and brush
        PreviewManager.Instance.defaultColor=Color.red;
        BrushRenderer.Instance.defaultColor=Color.red;
        changecolor("#FF0000");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // resets boolean values so next click will show the text popup again +
    // go back to normal functionality similiar to the way it does it at startup
    public void help()
    {
        isColorPaletteEnabled = false;
        isColorPalettePinEnabled = false;
        isDeleteButtonEnabled=false;
        isSelectMoveButtonEnabled=false;
        isDeleteButtonEnabled=false;
        islineSelectButtonsEnabled=false;
        isLoopButtonEnabled=false;
        isShapeToolButtonsEnabled = false;
        isShapeToolMenuEnabled=false;
        isShapeToolPinEnabled=false;
        isSizeSliderEnabled=false;
        isSmoothnessSliderEnabled=false;
        isStartMenuEnabled=false;
        isStartmenuPinEnabled=false;
        isSubMenuButtonsEnabled=false;
        isSubMenuEnabled=false;
        isSubMenuPinEnabled=false;
    }


    //uses a hex to color converter
    //makes the brush color the color picked by the user on the color palette
    //makes the backplate of every button menu the color selected but to opacity 0.3f
    public void changecolor (string hexvalue){
        //defaultColor=Color.red;
        ColorUtility.TryParseHtmlString(hexvalue, out BrushRenderer.Instance.defaultColor);
        PreviewManager.Instance.defaultColor=BrushRenderer.Instance.defaultColor;
        Color backplateColor=new Color (BrushRenderer.Instance.defaultColor.r,BrushRenderer.Instance.defaultColor.g,BrushRenderer.Instance.defaultColor.b,0.3f);
        submenuButtons.GetComponent<Microsoft.MixedReality.GraphicsTools.CanvasElementRoundedRect>().color=backplateColor;
        submenuPin.GetComponent<Microsoft.MixedReality.GraphicsTools.CanvasElementRoundedRect>().color=backplateColor;
        startmenuButtons.GetComponent<Microsoft.MixedReality.GraphicsTools.CanvasElementRoundedRect>().color=backplateColor;
        startmenuPin.GetComponent<Microsoft.MixedReality.GraphicsTools.CanvasElementRoundedRect>().color=backplateColor;
        colorPaletteButtons.GetComponent<Microsoft.MixedReality.GraphicsTools.CanvasElementRoundedRect>().color=backplateColor;
        colorPalettePin.GetComponent<Microsoft.MixedReality.GraphicsTools.CanvasElementRoundedRect>().color=backplateColor;
        shapeToolMenuButtons.GetComponent<Microsoft.MixedReality.GraphicsTools.CanvasElementRoundedRect>().color=backplateColor;
        shapeToolMenuPin.GetComponent<Microsoft.MixedReality.GraphicsTools.CanvasElementRoundedRect>().color=backplateColor;
    }
    
    public void toggleMixOn(){
        SelectTool.Instance.isMix=true;
    }

    public void toggleMixOff(){
        SelectTool.Instance.isMix=false;
    }

    //makes lineSelectButtons the color of the object it is selected by with 0.3f transparency
    public void changeColorOfLineSelectButtons(){
        if (StateManager.Instance.selectedObj==null){
            Debug.Log("selectedObj is null :(");
            return;
        }
        //if the selectedObj is text, it doesn't have a mesh renderer component, so need to get its TextMeshPro component
        //10 is the enum for the text layer
        if (StateManager.Instance.selectedObj.layer==10){
            Color lineSelectButtonsColorFromText=new Color (StateManager.Instance.selectedObj.GetComponent<TextMeshPro>().color.r,
            StateManager.Instance.selectedObj.GetComponent<TextMeshPro>().color.g,
            StateManager.Instance.selectedObj.GetComponent<TextMeshPro>().color.b,
            0.3f);
            lineSelectButtonsRealButtons.GetComponent<Microsoft.MixedReality.GraphicsTools.CanvasElementRoundedRect>().color=lineSelectButtonsColorFromText;
        } else{//if it's not a text (shape, line, draw)
            Color lineSelectButtonsColorFromDraw=new Color (StateManager.Instance.selectedObj.GetComponent<MeshRenderer>().material.color.r,
            StateManager.Instance.selectedObj.GetComponent<MeshRenderer>().material.color.g, 
            StateManager.Instance.selectedObj.GetComponent<MeshRenderer>().material.color.b,
            0.3f);
            lineSelectButtonsRealButtons.GetComponent<Microsoft.MixedReality.GraphicsTools.CanvasElementRoundedRect>().color
            =lineSelectButtonsColorFromDraw;
        }
       // Debug.Log(lineSelectButtonsRealButtons.GetComponent<Microsoft.MixedReality.GraphicsTools.CanvasElementRoundedRect>().color + "lineselectbuttons color");
        //Debug.Log(StateManager.Instance.selectedObj.GetComponent<MeshRenderer>().material.color + "selectedobj color");
    }

    public void disablestartMenu (){
        startMenu.SetActive(false);
    }

    public void enablestartMenu (){
        startMenu.SetActive(true);
        if (!isStartMenuEnabled)
        {
            isStartMenuEnabled = true;
        }
    }

    public void disableSubmenu(){
        submenu.SetActive(false);
    }

    public void enableSubmenu(){
        submenu.SetActive(true);
        if (!isSubMenuEnabled)
        {
            isSubMenuEnabled = true;
        }
    }

    public void disableColorPalette(){
        colorPalette.SetActive(false);
    }

    public void enableColorPalette(){
        colorPalette.SetActive(true);
        if (!isColorPaletteEnabled)
        {
            isColorPaletteEnabled = true;
        }
        }

    public void disableSmoothnessSlider(){
        smoothnessSlider.SetActive(false);
    }

    public void enableSmoothnessSlider(){
        smoothnessSlider.SetActive(true);
        if (!isSmoothnessSliderEnabled)
        {
            isSmoothnessSliderEnabled = true;
        }
    }

    public void updateSmoothnessThreshold(){
        BrushRenderer.Instance.defaultSmoothnessThreshold=smoothnessSlider.GetComponent<MixedReality.Toolkit.UX.Slider>().Value;
    }

    // public void updateLoop(){
    //     BrushRenderer.Instance.loop=loopButton.GetComponent<MixedReality.Toolkit.UX.Loop>().Value;
    // }

    public void disableLoop(){
        BrushRenderer.Instance.loop=false;
    }

    public void enableLoop(){
        BrushRenderer.Instance.loop=true;
    }

    public void disableSizeSlider(){
        sizeSlider.SetActive(false);
    }

    public void enableSizeSlider(){
        sizeSlider.SetActive(true);
        if (!isSizeSliderEnabled)
        {
            isSizeSliderEnabled = true;
        }
    }

    public void enableImportedMenu(){
        importedMenu.SetActive(true);
    }

    public void disableImportedMenu(){
        importedMenu.SetActive(false);
    }

    public void updateDefaultSize(){
        BrushRenderer.Instance.defaultSize=sizeSlider.GetComponent<MixedReality.Toolkit.UX.Slider>().Value;
        PreviewManager.Instance.defaultSize=sizeSlider.GetComponent<MixedReality.Toolkit.UX.Slider>().Value;
    }

    public void enableLineSelectButtons(){
        lineSelectButtons.SetActive(true);
    }

    // Here we also do the same logic check as we do in statemanager.
    // Possible optimization: move the logic to a function that is called so both here
    // and statemanager.drawclick can call it instead.
    public void disableLineSelectButtons(){
        //lineSelectButtons.SetActive(false);
        //if selectedobj != null then ensure its stateful is enabled and the object manipulator is disabled
        if(StateManager.Instance.selectedObj != null && StateManager.Instance.selectedObj.GetComponent<StatefulInteractable>().enabled == false){
            // Once again, order of disabling first before enabling greatly matters!
            StateManager.Instance.selectedObj.GetComponent<ObjectManipulator>().enabled = false;
            StateManager.Instance.selectedObj.GetComponent<StatefulInteractable>().enabled = true;
        }

        if (StateManager.Instance.selectedObj != null && StateManager.Instance.selectedObj.layer == 9 && StateManager.Instance.selectedObj.transform.childCount > 0)
        {
            Transform pivot = StateManager.Instance.selectedObj.transform.GetChild(0);
            // Unpivot first or else the menu will be deleted with the pivot.
            lineSelectButtons.transform.SetParent(StateManager.Instance.selectedObj.transform);
            Destroy(pivot.gameObject);
            Debug.Log("Hello?");
        }
        if (StateManager.Instance.selectedObj != null)
        {
            if (StateManager.Instance.selectedObj.layer == 10)
            {
                StateManager.Instance.selectedObj.transform.GetChild(0).gameObject.SetActive(false);
            }
            else
            {
                StateManager.Instance.selectedObj.GetComponent<MeshOutline>().enabled = false;
            }
        }
        //if(StateManager.Instance.selectedObj != null){
        //    StateManager.Instance.selectedObj.transform.SetParent(null);
        //}
        lineSelectButtons.transform.SetParent(null);
        // No selected object because we technically just closed the menu.
        StateManager.Instance.selectedObj = null;
        //lineSelectButtons.transform.localScale = Vector3.one;
        lineSelectButtons.SetActive(false);
        //lineSelectButtons.set
    }

    public void enableShapeToolMenu(){
        shapeToolMenu.SetActive(true);
    }

    public void disableShapeToolMenu(){
        shapeToolMenu.SetActive(false);
    }

    public void destroySelectedObj(){
        if(StateManager.Instance.selectedObj==null){
            Debug.LogWarning("selectedObj is null :(");
            return;
        }

        lineSelectButtons.transform.SetParent(null);

        // If connected don't destroy and let server confirm we could.
        if (ClientManager.Instance.connected)
        {
            //Debug.Log("SENT DELETE COMMAND!: " + StateManager.Instance.selectedObj.GetComponent<IDContainer>().id);
            DeleteContainer deleteCommand = new DeleteContainer(StateManager.Instance.selectedObj.GetComponent<IDContainer>().id);
            ClientManager.Instance.SendCommand(new Command(deleteCommand, JustMonika.Instance.GetChunk(StateManager.Instance.selectedObj.transform.position)));
        }
        else
        {
            Destroy(StateManager.Instance.selectedObj);
        }
        lineSelectButtons.SetActive(false);
        //Debug.Log("selectedObj before deleting is..."+StateManager.Instance.selectedObj.name);
        StateManager.Instance.selectedObj=null;
    }

    public void destroySelectedObjects(){
         foreach (GameObject g in SelectTool.Instance.selectedObjects){
            if (ClientManager.Instance.connected)
            {
                DeleteContainer deleteCommand = new DeleteContainer(g.GetComponent<IDContainer>().id);
                ClientManager.Instance.SendCommand(new Command(deleteCommand, JustMonika.Instance.GetChunk(g.transform.position)));
            }
            else
            {
                Destroy(g);
            }
            // g=null;
        } 
        multipleLineSelectButtons.SetActive(false);
    }

    public void radialViewMenus() { 
        startMenu.GetComponent<RadialView>().enabled = true;
        startMenu.SetActive(true);
        submenu.GetComponent<RadialView>().enabled =true;
        colorPalette.GetComponent<RadialView>().enabled = true;
        shapeToolMenu.GetComponent<RadialView>().enabled = true;
        
    }
    public void resetSelectMenu(){
        // Note: setting toggle to a value also triggers the events that would happen
        // -had we manually pressed the button in the scene.
        // this probably means we have some redundant logic wrt (with respect to) this function, but it does not
        // -break anything as of now.
        selectMoveButton.ForceSetToggled(false);
    }

    public void resetmultiSelectMenu(){
        multiSelectMoveButton.ForceSetToggled(false);
    }

    // Order of disabling --> enabling matters. With the opposite orders, both scripts are enabled
    // -at the same time, which seems to mess things up. Therefore disable the active component
    // -first  before enabling the inactive one.
    
    public void enableObjectTool() {
        ObjectTool.Instance.scrollDownMenu.SetActive(true);
    }

    public void disableObjectTool()
    {
        ObjectTool.Instance.scrollDownMenu.SetActive(false);
    }
    public void enableObjectManipulator(){
        //ORDER MATTERS
        StateManager.Instance.selectedObj.GetComponent<StatefulInteractable>().enabled=false;
        StateManager.Instance.selectedObj.GetComponent<ObjectManipulator>().enabled=true;
        //ObjectManipulator om = StateManager.Instance.selectedObj.AddComponent<ObjectManipulator>();
        //XRInteractionManager.interactable=false;
        //Debug.Log("object manipulator enabled");
    }

    public void disableObjectManipulator(){
        //ORDER MATTERS
        StateManager.Instance.selectedObj.GetComponent<ObjectManipulator>().enabled=false;
        StateManager.Instance.selectedObj.GetComponent<StatefulInteractable>().enabled=true;
        //Debug.Log("object manipulator disabled");
    }

    public void enableObjectManipulators(){
        //ORDER MATTERS
         foreach (GameObject g in SelectTool.Instance.selectedObjects){
            g.GetComponent<StatefulInteractable>().enabled=false;
            g.GetComponent<ObjectManipulator>().enabled=true;
        } 
    }

    public void disableObjectManipulators(){
        SelectTool.Instance.selectionBox.Activate();
        //ORDER MATTERS
        foreach (GameObject g in SelectTool.Instance.selectedObjects){
            g.GetComponent<ObjectManipulator>().enabled=false;
            g.GetComponent<StatefulInteractable>().enabled=true;
        } 
    }

    public void enableInputField(){
        inputField.SetActive(true);
    }

    public void disableInputField(){
        inputField.SetActive(false);
    }

    public void enableMultipleSelect(){
        multipleLineSelectButtons.SetActive(true);
    }

    public void disableMultipleSelect(bool isCalledFromMultiDuplicate){
        multipleLineSelectButtons.SetActive(false);
        //if this is called from the multi duplicate button, then keep isMultiple true in order to avoid
        //multiple menus popping up (bad errors)
        if (isCalledFromMultiDuplicate){
            isMultiple=true;
            //Debug.Log("ISMULTIPLE IS TRUE (FROM MULTIDUPLICATE CALL)");
        } else{
            isMultiple=false;
            //Debug.Log("ISMULTIPLE IS FALSE (FROM DISABLING MULTI SELECT)");
        }
        //make sure to disable object manipulator and enable stateful interactable by resetting the menu
        resetmultiSelectMenu();
        //don't just disable the menu, but also for each object that was selected, unhighlight it
         foreach (GameObject g in SelectTool.Instance.selectedObjects){
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

    public GameObject duplicateObj(GameObject obj)
    {
        //GameObject obj = StateManager.Instance.selectedObj;
        // isMultiple=false;
        // Uh oh!
        if (obj == null)
        {
            return null;
        }

        // TODO: fix shape pivot issue & the select menu issue as well
        if (isMultiple==true){
            disableMultipleSelect(true);
        } else{
            disableLineSelectButtons();
        }
        GameObject newObj = Instantiate(obj);
        //make the duplicate object a child of the empty parent that holds all of that type of drawing
        switch (obj.layer)
        {
            case 7:
                newObj.transform.SetParent(BrushTool.Instance.brushParent);
                break;
            case 8:
                newObj.transform.SetParent(LineTool.Instance.lineParent);
                break;
            case 9:
                newObj.transform.SetParent(ShapeTool.Instance.shapeTool);
                break;
            case 10:
                newObj.transform.SetParent(TextTool.Instance.textTool);
                break;
        }

        //after here is where shit goes wrong
        if (isMultiple==false){
            // find a way to see if prev parent event is still in onclicked? or maybe clear as a just in case sort of thing.
            newObj.GetComponent<StatefulInteractable>().OnClicked.AddListener(() => StateManager.Instance.DrawClick(newObj));
            Vector3 offsetDir = (Camera.main.transform.position - newObj.transform.position).normalized;
            newObj.transform.position += offsetDir * 0.08f;
            StateManager.Instance.DrawClick(newObj);

            (_, Vector3? _offsetDir) = MathUtils.GetMenuSelectPos(ButtonManager.Instance.cameraPos.position, newObj);
            // If no collision happened (laserOffset && offsetDir == null), just default to the center of the gameobject.
            if (_offsetDir != null)
            {
                    ButtonManager.Instance.lineSelectButtons.transform.position += 0.08f * _offsetDir.Value;
                    Debug.Log(ButtonManager.Instance.lineSelectButtons);
            }
        }
        else
        {
            newObj.GetComponent<StatefulInteractable>().OnClicked.AddListener(() => StateManager.Instance.DrawClick(newObj));
            Vector3 offsetDir = (Camera.main.transform.position - newObj.transform.position).normalized;
            newObj.transform.position += offsetDir * 0.08f;
            StateManager.Instance.DrawClick(newObj);
        }
    //code for SERVE-r (nails emoji)
    // if (ClientManager.Instance.connected)
    //     {
        switch (newObj.layer)
        {
            case Settings.LAYER_BRUSH://brushtool
            // BrushContainer duplicateLineContainer=new BrushContainer(BrushRenderer.Instance.defaultSize,
            //     BrushRenderer.Instance.waveSides,
            //     obj.transform.position,
            //     BrushRenderer.Instance.defaultColor,
            //     meshPoints,
            //     BrushRenderer.Instance.isWave,
            //     BrushRenderer.Instance.waveTrough,
            //     BrushRenderer.Instance.wavePeak,
            //     BrushRenderer.Instance.loop,
            //     BrushRenderer.Instance.isMetallic,
            //     null );
           // AddContainer addContainer = new AddContainer(duplicateLineContainer, obj.transform);
           // Command c = new Command(addContainer, JustMonika.Instance.GetChunk(obj.transform.position));
            
                break;
            case Settings.LAYER_LINE://linetool
                break;
            case Settings.LAYER_SHAPE://shapetool
                
                break;
            case Settings.LAYER_TEXT://texttool
                if (ClientManager.Instance.connected){
                    //make the container and command, and send the command
                    Vector3 v = new Vector3(newObj.GetComponent<TextMeshPro>().rectTransform.sizeDelta.x, newObj.GetComponent<TextMeshPro>().rectTransform.sizeDelta.y, 1);
                    TextContainer textContainer=new TextContainer(
                        newObj.GetComponent<TextMeshPro>().text, //text
                        v,//size 
                        newObj.GetComponent<TextMeshPro>().rectTransform.position, //position
                        newObj.transform.eulerAngles,//rotation
                        newObj.GetComponent<TextMeshPro>().color//color
                    );
                    AddContainer addContainer = new AddContainer(textContainer, newObj.transform);
                    Command command = new Command(addContainer, JustMonika.Instance.GetChunk(newObj.transform.position));
                    Destroy(newObj);
                    enabled = false;
                    obj = null;
                    ClientManager.Instance.SendCommand(command);
                }
                else{
                    //actually do the action 
                    enabled = false;
                    newObj = null;
                 }
                break;
        }
        // } else{
        //     enabled = false;
        //     obj = null;
        // }
        return newObj;
    }

    public void duplicateObj(){
        duplicateObj(StateManager.Instance.selectedObj);
    }

//this is called when the duplicate button is pressed on the multiplelineselectbuttons button
    public void duplicateObjects(){
        //this list has the currently selected gameobjects
        List<GameObject> objs=SelectTool.Instance.selectedObjects;

        //this list will have the newly copied gameobjects
        List<GameObject> newObjs=new List<GameObject>();

        foreach (GameObject g in objs)
    	{
        	newObjs.Add(duplicateObj(g));
            //Debug.Log(g.name + " copied into " + duplicateObj(g).name);
    	}
        SelectTool.Instance.setUpMultiSelect(newObjs);
        isMultiple=true;
        Debug.Log("ISMULTIPLE IS TRUE (FROM DUPLICATEOBJECTS)");
        //
    }
}