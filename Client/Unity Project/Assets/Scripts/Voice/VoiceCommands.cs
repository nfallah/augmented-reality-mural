using MixedReality.Toolkit;
using UnityEngine;
using UnityEngine.Events;


public class VoiceCommands : MonoBehaviour {
    // start menu unity action variables
    private UnityAction activateStartMenu;
    private UnityAction deactivateStartMenu;
    // brush tool unity action variables
    private UnityAction brushActivateAction;
    private UnityAction brushDeactivateAction;
    // smoothness slider tool unity action variables
    private UnityAction smoothActivateAction;
    private UnityAction smoothDeactivateAction;
    // size slider tool unity action variables
    private UnityAction sizeActivateAction;
    private UnityAction sizeDeactivateAction;
    // color slider tool unity action variables
    private UnityAction colorActivateAction;
    private UnityAction colorDeactivateAction;
    // menu boomerang
    private UnityAction menuBoomerangAction;

    public static VoiceCommands Instance { get; private set; }

    private void Awake()
    {
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

    // Commands can be registered here.
    private void Start()
    {
        // start menu appearing voice commands - start menu and hide menu
        activateStartMenu = ButtonManager.Instance.enablestartMenu;
        VoiceManager.Instance.Register("start menu", activateStartMenu);
        deactivateStartMenu = ButtonManager.Instance.disablestartMenu;
        VoiceManager.Instance.Register("hide menu", deactivateStartMenu);

        // brush tool activated - draw and stop drawing
        brushActivateAction += BrushTool.Instance.Activate;
        VoiceManager.Instance.Register("draw", brushActivateAction);
        brushDeactivateAction += BrushTool.Instance.Deactivate;
        VoiceManager.Instance.Register("stop drawing", brushDeactivateAction);

        // color palette activated - color change and hide colors
        colorActivateAction = ButtonManager.Instance.enableColorPalette;
        VoiceManager.Instance.Register("color change", colorActivateAction);
        colorDeactivateAction = ButtonManager.Instance.disableColorPalette;
        VoiceManager.Instance.Register("hide colors", colorDeactivateAction);

        // size sliders activated - show size and hide size
        sizeActivateAction = ButtonManager.Instance.enableSizeSlider;
        VoiceManager.Instance.Register("show size", sizeActivateAction);
        sizeDeactivateAction = ButtonManager.Instance.disableSizeSlider;
        VoiceManager.Instance.Register("hide size", sizeDeactivateAction);

        // smoothness slider activated - show smoothness and hide smoothness
        smoothActivateAction = ButtonManager.Instance.enableSmoothnessSlider;
        smoothDeactivateAction = ButtonManager.Instance.disableSmoothnessSlider;
        VoiceManager.Instance.Register("show smoothness", smoothActivateAction);
        VoiceManager.Instance.Register("hide smoothness", smoothDeactivateAction);

        // menu boomerang
        menuBoomerangAction = ButtonManager.Instance.radialViewMenus;
        VoiceManager.Instance.Register("menu", menuBoomerangAction);
    }

    /* SAMPLE EVENT REGISTRATION (Unity also has documentation on how to use UnityActions)
     * // These should be global variables in the script!
     * private UnityAction brushActivateAction;
     * private UnityAction brushDeactivateAction;
     *    
     * brushActivateAction += BrushTool.Instance.Activate;
     * VoiceManager.Instance.Register("on", brushActivateAction);
     *
     * brushDeactivateAction += BrushTool.Instance.Deactivate;
     * VoiceManager.Instance.Register("off", brushDeactivateAction);
     */
}
