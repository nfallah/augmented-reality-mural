using MixedReality.Toolkit.SpatialManipulation;
using MixedReality.Toolkit;
using TMPro;
using UnityEngine;
using Microsoft.MixedReality.GraphicsTools;

public class TextTool : MonoBehaviour
{
    public static TextTool Instance { get; private set; }

    [SerializeField]
    private float colliderPadding, textThickness;

    // Used for the outline
    [SerializeField]
    private GameObject childPrefab;

    [SerializeField]
    private string defaultText;

    [SerializeField]
    private Material outlineMaterial;

    private bool activated, isLeftHand;

    private TextMeshPro currentText;

    // Parent that stores all placed texts during runtime
    public Transform textTool;

    private Vector3 currentStart, currentEnd;

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
    
        textTool = new GameObject("Text Tool").transform;
        textTool.transform.position = Vector3.zero;
    }

    private void Start()
    {
        ButtonManager.Instance.textToolField.text = "";

        if (enabled == true)
        {
            enabled = false;
        }
    }

    private void Update()
    {
        if (currentText == null)
        {
            return;
        }

        currentEnd = isLeftHand ? PinchManager.Instance.LeftPosition :
                                  PinchManager.Instance.RightPosition;

        if (MathUtils.Vector3Equals(currentStart, currentEnd) == true)
        {
            return;
        }

        currentText.transform.position = (currentStart + currentEnd) / 2f;
        Vector3 size = new Vector3(Mathf.Abs(currentEnd.x - currentStart.x),
                                   Mathf.Abs(currentEnd.y - currentStart.y),
                                   1);
        bool isInvalid = size.x == 0 || size.y == 0;

        if (isInvalid == true && currentText.gameObject.activeSelf == true)
        {
            currentText.gameObject.SetActive(false);
        }
        else if (isInvalid == false)
        {
            currentText.rectTransform.sizeDelta = size;
            Vector3 dir = currentText.transform.position - Camera.main.transform.position;
            Quaternion lookAtUser = Quaternion.LookRotation(dir);
            currentText.transform.rotation = lookAtUser;

            if (currentText.gameObject.activeSelf == false)
            {
                currentText.gameObject.SetActive(true);
            }
        }
    }

    private void StartTool(bool isLeftHand)
    {
        if (enabled == true)
        {
            return;
        }

        enabled = true;
        this.isLeftHand = isLeftHand;
        currentStart = isLeftHand ? PinchManager.Instance.LeftPosition :
                                    PinchManager.Instance.RightPosition;

        /*** Instantiate text object with all relevant values ***/
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(textTool);
        textObj.transform.localPosition = Vector3.zero;

        // Add and set text component values
        currentText = textObj.AddComponent<TextMeshPro>();
        currentText.enableAutoSizing = true;
        currentText.fontSizeMin = 0;
        currentText.fontSizeMax = 100;
        currentText.color = BrushRenderer.Instance.defaultColor;
        currentText.horizontalAlignment = HorizontalAlignmentOptions.Center;
        currentText.verticalAlignment = VerticalAlignmentOptions.Middle;
        currentText.enableWordWrapping = false;
        currentText.overflowMode = TextOverflowModes.Overflow;
        currentText.text = ButtonManager.Instance.textToolField.text.Trim();

        // If the user does not have custom text, utilize the default string
        if (currentText.text.Length <= 0)
        {
            currentText.text = defaultText;
        }

        textObj.SetActive(false);
    }

    private async void StopTool(bool isLeftHand)
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

        // Do not place text if inactive (size.x || size.y == 0) or font size is 0.
        if (currentText.fontSize <= 0 || currentText.gameObject.activeSelf == false)
        {
            Destroy(currentText.gameObject);
            enabled = false;
            currentText = null;
            return;
        }

        // Clamp size of text to exactly what is needed
        currentText.rectTransform.sizeDelta = currentText.textBounds.size;

        // Otherwise, add a box collider and register clicked event.
        BoxCollider collider = currentText.gameObject.AddComponent<BoxCollider>();
        collider.center = Vector3.zero;
        Vector2 size2D = currentText.rectTransform.sizeDelta;
        Vector3 size3D = new Vector3(size2D.x + colliderPadding, size2D.y + colliderPadding, textThickness);
        // Set collider side
        collider.size = size3D;
        StatefulInteractable i = currentText.gameObject.AddComponent<StatefulInteractable>();
        // Capturing 'currentText.gameObject' first ensures no null errors occur.
        GameObject capturedObject = currentText.gameObject;
        i.OnClicked.AddListener(() => StateManager.Instance.DrawClick(capturedObject));
        i.enabled = false;
        ObjectManipulator m = currentText.gameObject.AddComponent<ObjectManipulator>();
        m.enabled = false;
        i.enabled = true;

        // Set layer to 'Text'
        currentText.gameObject.layer = Settings.LAYER_TEXT;

        // Add mesh outline child
        GameObject outline = OutlineGenerator.GenerateOutline(size3D, null, textThickness / 2f);
        outline.transform.SetParent(currentText.transform);
        outline.transform.localPosition = Vector3.zero;
        outline.transform.localEulerAngles = Vector3.zero;
        outline.GetComponent<Renderer>().material = outlineMaterial;
        outline.SetActive(false);

        if (!ClientManager.Instance.connected)
        {
            enabled = false;
            currentText = null;
            return;
        }

        // Send command
        TextContainer textContainer = new TextContainer
            (
                currentText.text,
                new Vector3(currentText.rectTransform.sizeDelta.x, currentText.rectTransform.sizeDelta.y, 1),
                currentText.transform.position,
                currentText.transform.eulerAngles,
                currentText.color

            );
        AddContainer addContainer = new AddContainer(textContainer, currentText.transform);
        Command command = new Command(addContainer, JustMonika.Instance.GetChunk(currentText.transform.position));
        Destroy(currentText.gameObject);
        enabled = false;
        currentText = null;

        await ClientManager.Instance.SendCommand(command);
    }

    private void CancelTool()
    {
        if (enabled == false)
        {
            return;
        }

        // Also halt the drawing process if currently active
        enabled = false;
        Destroy(currentText.gameObject);
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

    public GameObject Regenerate(TextContainer c)
    {
        Vector3 pos = c.meshPosition.ToVector3();
        /*** Instantiate text object with all relevant values ***/
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(textTool);
        textObj.transform.position = pos;

        // Add and set text component values
        TextMeshPro currentText = textObj.AddComponent<TextMeshPro>();
        currentText.enableAutoSizing = true;
        currentText.fontSizeMin = 0;
        currentText.fontSizeMax = 100;
        currentText.color = c.meshColor.ToColor();
        currentText.horizontalAlignment = HorizontalAlignmentOptions.Center;
        currentText.verticalAlignment = VerticalAlignmentOptions.Middle;
        currentText.enableWordWrapping = false;
        currentText.overflowMode = TextOverflowModes.Overflow;
        currentText.text = c.text;

        Vector3 size = c.meshSize.ToVector3();

        currentText.rectTransform.sizeDelta = size;
        currentText.transform.eulerAngles = c.meshRotation.ToVector3();

        // Otherwise, add a box collider and register clicked event.
        BoxCollider collider = currentText.gameObject.AddComponent<BoxCollider>();
        collider.center = Vector3.zero;
        Vector2 size2D = currentText.rectTransform.sizeDelta;
        Vector3 size3D = new Vector3(size2D.x + colliderPadding, size2D.y + colliderPadding, textThickness);
        // Set collider side
        collider.size = size3D;
        StatefulInteractable i = currentText.gameObject.AddComponent<StatefulInteractable>();
        // Capturing 'currentText.gameObject' first ensures no null errors occur.
        GameObject capturedObject = currentText.gameObject;
        i.OnClicked.AddListener(() => StateManager.Instance.DrawClick(capturedObject));
        i.enabled = false;
        ObjectManipulator m = currentText.gameObject.AddComponent<ObjectManipulator>();
        m.enabled = false;
        i.enabled = true;

        // Set layer to 'Text'
        currentText.gameObject.layer = Settings.LAYER_TEXT;

        // Add mesh outline child
        GameObject outline = OutlineGenerator.GenerateOutline(size3D, null, textThickness / 2f);
        outline.transform.SetParent(currentText.transform);
        outline.transform.localPosition = Vector3.zero;
        outline.transform.localEulerAngles = Vector3.zero;
        outline.GetComponent<Renderer>().material = outlineMaterial;
        outline.SetActive(false);
        return textObj;
    }
}