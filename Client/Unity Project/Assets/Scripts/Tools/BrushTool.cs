using Microsoft.MixedReality.GraphicsTools;
using System.Collections.Generic;
using UnityEngine;

public class BrushTool : MonoBehaviour
{
    public static BrushTool Instance { get; private set; }

    /* 'granularity' refers to how many units the user must travel before placing a point.
     * A lower value will place more points and make the mesh smoother, but this comes at the cost-
     * of worse performance and more server storage.
     */
    [SerializeField]
    private float granularity;

    private bool activated;

    private bool isLeftHand;

    private float distanceLeft;

    private Vector3? oldPos;

    private List<Vector3> meshPoints;

    public Transform brushParent;

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

        brushParent = new GameObject("Brush Tool").transform;
        brushParent.position = Vector3.zero;
    }

    private void Update()
    {
        Vector3 newPos = isLeftHand ? PinchManager.Instance.LeftPosition :
                                      PinchManager.Instance.RightPosition;

        // Only update the line if the position has changed since the last frame.
        if (MathUtils.Vector3Equals(newPos, oldPos.Value) == false)
        {
            PreviewManager.Instance.UpdatePreview(newPos);
        }

        float distanceTraveled = (newPos - oldPos.Value).magnitude;

        if (distanceTraveled >= distanceLeft)
        {
            // Calculate the total distance to be tracked
            float totalDistanceTracked = distanceLeft;
            distanceTraveled -= distanceLeft;
            distanceLeft = granularity;

            // Calculate granularity repetitions
            int granularityRepetitions = Mathf.FloorToInt(distanceTraveled / granularity);
            totalDistanceTracked += granularityRepetitions * granularity;
            distanceTraveled -= granularityRepetitions * granularity;
            distanceLeft -= distanceTraveled;

            // Place a new point based on the total distance tracked
            Vector3 meshPoint = MathUtils.TravelAcross(oldPos.Value, newPos, totalDistanceTracked);
            meshPoints.Add(meshPoint);
            PreviewManager.Instance.AddPreview(meshPoint);
        }
        else
        {
            distanceLeft -= distanceTraveled;
        }

        oldPos = newPos;
    }

    public void StartTool(bool isLeftHand)
    {
        if (enabled == true)
        {
            return;
        }

        enabled = true;
        this.isLeftHand = isLeftHand; 
        distanceLeft = granularity;
        Vector3 startPos = isLeftHand ? PinchManager.Instance.LeftPosition :
                                        PinchManager.Instance.RightPosition;
        oldPos = startPos;
        meshPoints = new List<Vector3> { startPos };
        PreviewManager.Instance.StartPreview(startPos);
    }

    public async void StopTool(bool isLeftHand)
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

        /*** Add final position and generate spline mesh ***/
        Vector3 endPos = oldPos.Value;
        meshPoints.Add(endPos);
        GameObject obj = BrushRenderer.Instance.GenerateMesh(true, meshPoints);
        Command c = null;

        if (obj != null)
        {
            obj.name = "Brush";
            obj.transform.SetParent(brushParent);

            // Set layer to 'Brush'
            obj.layer = 7;

            // Add mesh outline (to be enabled when this object is selected)
            MeshOutline outline = obj.AddComponent<MeshOutline>();
            outline.enabled = false;
            outline.OutlineWidth = Settings.Instance.outlineThickness;
            outline.OutlineMaterial = Settings.Instance.outlineMaterial;

            // Finally serialize:
            BrushContainer lineContainer = new BrushContainer(
                BrushRenderer.Instance.defaultSize,
                BrushRenderer.Instance.waveSides,
                obj.transform.position,
                BrushRenderer.Instance.defaultColor,
                meshPoints,
                BrushRenderer.Instance.isWave,
                BrushRenderer.Instance.waveTrough,
                BrushRenderer.Instance.wavePeak,
                BrushRenderer.Instance.loop,
                BrushRenderer.Instance.isMetallic,
                null
                );
            AddContainer addContainer = new AddContainer(lineContainer, obj.transform);
            c = new Command(addContainer, JustMonika.Instance.GetChunk(obj.transform.position));
        }

        /*** Reset values ***/
        enabled = false;
        oldPos = null;
        meshPoints = null;
        PreviewManager.Instance.StopPreview();

        if (c != null && ClientManager.Instance.connected)
        {
            await ClientManager.Instance.SendCommand(c);
        }
        
        if (ClientManager.Instance.connected)
        {
            Destroy(obj);
        }
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

        // Also halt the drawing process if currently active
        if (enabled == true)
        {
            enabled = false;
            oldPos = null;
            meshPoints = null;
            PreviewManager.Instance.StopPreview();
        }
    }

    public GameObject Regenerate(BrushContainer c)
    {
        GameObject newObj = BrushRenderer.Instance.GenerateMesh
        (
            true,
            SerializeUtilities.SVector3ToVector3(c.meshPoints),
            c.meshSides,
            c.meshSize,
            c.meshColor.ToColor(),
            c.isWave,
            c.isLooped,
            c.waveTopRatio,
            c.waveBottomRatio,
            null,
            null,
            c.textureID ?? -1
        );

        if (newObj != null)
        {
            // Add mesh outline (to be enabled when this object is selected)
            MeshOutline outline = newObj.AddComponent<MeshOutline>();
            outline.enabled = false;
            outline.OutlineWidth = Settings.Instance.outlineThickness;
            outline.OutlineMaterial = Settings.Instance.outlineMaterial;
            newObj.transform.position = c.meshPosition.ToVector3(); // redundant but once we make brushrenderer regen this will work.
            newObj.name = "Brush";
            newObj.layer = Settings.LAYER_BRUSH;
            newObj.transform.SetParent(brushParent);
        }

        return newObj;
    }
}