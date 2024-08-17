using Microsoft.MixedReality.GraphicsTools;
using System.Collections.Generic;
using UnityEngine;

public class LineTool : MonoBehaviour
{
    public static LineTool Instance { get; private set; }

    private bool activated;
    
    private bool isLeftHand;

    private Vector3? oldPos;

    private List<Vector3> meshPoints;

    public Transform lineParent;

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

        lineParent = new GameObject("Line Tool").transform;
        lineParent.position = Vector3.zero;
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

        oldPos = newPos;
    }

    private void StartTool(bool isLeftHand)
    {
        if (enabled == true)
        {
            return;
        }

        enabled = true;
        this.isLeftHand = isLeftHand;
        Vector3 startPos = isLeftHand ? PinchManager.Instance.LeftPosition :
                                        PinchManager.Instance.RightPosition;
        oldPos = startPos;
        meshPoints = new List<Vector3> { startPos };
        PreviewManager.Instance.StartPreview(startPos);
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

        /*** Add final position and generate spline mesh ***/
        Vector3 endPos = oldPos.Value;
        meshPoints.Add(endPos);
        // Hard code with no loop and smoothness as it does not make sense for a line tool.
        GameObject obj = BrushRenderer.Instance.GenerateMesh(false, meshPoints, loop: false, smoothnessThreshold: -1);
        Command c = null;
        if (obj != null)
        {
            obj.name = "Line";
            obj.transform.SetParent(lineParent);

            // Set layer to 'Line'
            obj.layer = Settings.LAYER_LINE;

            // Add mesh outline (to be enabled when this object is selected)
            MeshOutline outline = obj.AddComponent<MeshOutline>();
            outline.enabled = false;
            outline.OutlineWidth = Settings.Instance.outlineThickness;
            outline.OutlineMaterial = Settings.Instance.outlineMaterial;

            LineContainer lineContainer = new LineContainer(
                BrushRenderer.Instance.defaultSize,
                BrushRenderer.Instance.waveSides,
                obj.transform.position,
                BrushRenderer.Instance.defaultColor,
                meshPoints,
                BrushRenderer.Instance.isWave,
                BrushRenderer.Instance.waveTrough,
                BrushRenderer.Instance.wavePeak,
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

    public GameObject Regenerate(LineContainer c)
    {
        GameObject newObj = BrushRenderer.Instance.GenerateMesh
        (
            false,
            SerializeUtilities.SVector3ToVector3(c.meshPoints),
            c.meshSides,
            c.meshSize,
            c.meshColor.ToColor(),
            c.isWave,
            false,
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
            newObj.layer = Settings.LAYER_LINE;
            newObj.name = "Line";
            newObj.transform.SetParent(lineParent);
        }

        return newObj;
    }
}