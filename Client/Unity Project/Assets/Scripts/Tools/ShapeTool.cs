using Microsoft.MixedReality.GraphicsTools;
using MixedReality.Toolkit;
using MixedReality.Toolkit.SpatialManipulation;
using System.Threading.Tasks;
using UnityEngine;

public class ShapeTool : MonoBehaviour
{
    public static ShapeTool Instance { get; private set; }

    [SerializeField]
    Material defaultMaterial;

    [SerializeField]
    private ShapeContainer.Type defaultShape;
    
    /* shapes[i] corresponds to the ith enum.
     * Each element should be have a size of (1, 1, 1) in order for the scaling operations to work
     * -as intended.
     */
    [SerializeField]
    private GameObject[] shapes;

    private bool activated, isLeftHand;

    private Transform shapeObj;

    private ShapeContainer.Type currentShape;

    private Vector3 currentStart, currentEnd;

    public Transform shapeTool;

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

        currentShape = defaultShape;

        if (enabled == true)
        {
            enabled = false;
        }

        shapeTool = new GameObject("Shape Tool").transform;
        shapeTool.transform.position = Vector3.zero;
    }

    private void Update()
    {
        if (shapeObj == null)
        {
            return;
        }    

        currentEnd = isLeftHand ? PinchManager.Instance.LeftPosition :
                                  PinchManager.Instance.RightPosition;

        if (MathUtils.Vector3Equals(currentStart, currentEnd) == true)
        {
            return;
        }

        Vector3 offset = currentEnd - currentStart;
        bool isInvalid = offset.x == 0 || offset.y == 0 || offset.z == 0;

        if (isInvalid == true && shapeObj.gameObject.activeSelf == true)
        {
            shapeObj.gameObject.SetActive(false);
        }
        else if (isInvalid == false)
        {
            shapeObj.position = (currentEnd + currentStart) / 2;
            shapeObj.localScale = offset;

            if (shapeObj.gameObject.activeSelf == false)
            {
                shapeObj.gameObject.SetActive(true);
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
        // While efficient, an out of bounds could occur below if we are not
        // -careful in the editor.
        shapeObj = Instantiate(shapes[(int)currentShape], shapeTool).transform;
        shapeObj.name = currentShape.ToString();
        shapeObj.transform.localPosition = Vector3.zero;
        currentStart = isLeftHand ? PinchManager.Instance.LeftPosition :
                                    PinchManager.Instance.RightPosition;
        shapeObj.GetComponent<Renderer>().material = defaultMaterial;
        shapeObj.GetComponent<Renderer>().material.color = BrushRenderer.Instance.defaultColor;
        shapeObj.gameObject.SetActive(false);
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

        // Do not place if inactive (i.e., >= 1 scale components are zero).
        if (shapeObj.gameObject.activeSelf == false)
        {
            Destroy(shapeObj.gameObject);
            enabled = false;
            return;
        }

        // If any of the scales are negative, we must add a mesh collider
        // -as a box collider is (apparently) not compatible with negative sizes.
        if (shapeObj.localScale.x < 0 || shapeObj.localScale.y < 0 || shapeObj.localScale.z < 0)
        {
            MeshCollider meshCollider = shapeObj.gameObject.AddComponent<MeshCollider>();
            meshCollider.convex = true;
        }
        // We add a collider not when an object is instantiated, but once we finalize its placement.
        else
        {
            switch (currentShape)
            {
                case (ShapeContainer.Type.CUBE):
                    shapeObj.gameObject.AddComponent<BoxCollider>();
                    break;
                case (ShapeContainer.Type.SPHERE):
                    shapeObj.gameObject.AddComponent<SphereCollider>();
                    break;
                default:
                    Debug.LogWarning("ShapeTool --> StopTool: no collider added.");
                    return;
            }
        }

        // Set layer to 'Shape'
        shapeObj.gameObject.layer = Settings.LAYER_SHAPE;

        // Add mesh outline (to be enabled when this object is selected)
        MeshOutline outline = shapeObj.gameObject.AddComponent<MeshOutline>();
        outline.enabled = false;
        outline.OutlineWidth = Settings.Instance.outlineThickness;
        outline.OutlineMaterial = Settings.Instance.outlineMaterial;

        StatefulInteractable i = shapeObj.gameObject.AddComponent<StatefulInteractable>();
        // Capturing 'shapeObj.gameObject' first ensures no null errors occur.
        GameObject capturedObject = shapeObj.gameObject;
        i.OnClicked.AddListener(() => StateManager.Instance.DrawClick(capturedObject));
        i.enabled = false;
        ObjectManipulator m = shapeObj.gameObject.AddComponent<ObjectManipulator>();
        m.enabled = false;
        i.enabled = true;

        // Register event
        ShapeContainer shapeContainer = new ShapeContainer
            (currentShape, shapeObj.localScale, shapeObj.position, shapeObj.gameObject.GetComponent<Renderer>().material.color);

        AddContainer addContainer = new AddContainer(shapeContainer, shapeObj.transform);
        Command command = new Command(addContainer, JustMonika.Instance.GetChunk(shapeObj.position));

        if (ClientManager.Instance.connected)
        {
            Destroy(shapeObj.gameObject);
            await ClientManager.Instance.SendCommand(command);
        }

        shapeObj = null;
        enabled = false;

        await Task.CompletedTask;
    }

    private void CancelTool()
    {
        if (enabled == false)
        {
            return;
        }

        // Also halt the drawing process if currently active
        enabled = false;
        Destroy(shapeObj.gameObject);
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

    //this changes the shape to rectangular prism, ellipsoid, etc
    public void changeShape(int type){
        ShapeContainer.Type newshape = (ShapeContainer.Type)type;
        CurrentShape= newshape;
    }

    public ShapeContainer.Type CurrentShape
    {
        get
        {
            return currentShape;
        }

        set
        {
            // Do not change the shape if the user is currently drawing
            if (enabled == false)
            {
                currentShape = value;
            }
            else
            {
                Debug.LogWarning("ShapeTool --> CurrentShape: attempting to change shape while drawing.");
            }
        }
    }

    public GameObject Regenerate(ShapeContainer c)
    {
        Transform shapeObj = Instantiate(shapes[(int)c.type], shapeTool).transform;
        shapeObj.name = c.type.ToString();
        shapeObj.transform.localPosition = Vector3.zero;
        shapeObj.GetComponent<Renderer>().material = defaultMaterial;
        shapeObj.GetComponent<Renderer>().material.color = c.meshColor.ToColor();
        shapeObj.position = c.meshPosition.ToVector3();
        shapeObj.localScale = c.meshSize.ToVector3();

        // If any of the scales are negative, we must add a mesh collider
        // -as a box collider is (apparently) not compatible with negative sizes.
        if (shapeObj.localScale.x < 0 || shapeObj.localScale.y < 0 || shapeObj.localScale.z < 0)
        {
            MeshCollider meshCollider = shapeObj.gameObject.AddComponent<MeshCollider>();
            meshCollider.convex = true;
        }
        // We add a collider not when an object is instantiated, but once we finalize its placement.
        else
        {
            switch (c.type)
            {
                case (ShapeContainer.Type.CUBE):
                    shapeObj.gameObject.AddComponent<BoxCollider>();
                    break;
                case (ShapeContainer.Type.SPHERE):
                    shapeObj.gameObject.AddComponent<SphereCollider>();
                    break;
                default:
                    Debug.LogWarning("ShapeTool --> StopTool: no collider added.");
                    return null;
            }
        }

        // Set layer to 'Shape'
        shapeObj.gameObject.layer = Settings.LAYER_SHAPE;

        // Add mesh outline (to be enabled when this object is selected)
        MeshOutline outline = shapeObj.gameObject.AddComponent<MeshOutline>();
        outline.enabled = false;
        outline.OutlineWidth = Settings.Instance.outlineThickness;
        outline.OutlineMaterial = Settings.Instance.outlineMaterial;

        StatefulInteractable i = shapeObj.gameObject.AddComponent<StatefulInteractable>();
        // Capturing 'shapeObj.gameObject' first ensures no null errors occur.
        GameObject capturedObject = shapeObj.gameObject;
        i.OnClicked.AddListener(() => StateManager.Instance.DrawClick(capturedObject));
        i.enabled = false;
        ObjectManipulator m = shapeObj.gameObject.AddComponent<ObjectManipulator>();
        m.enabled = false;
        i.enabled = true;

        return shapeObj.gameObject;
    }
}