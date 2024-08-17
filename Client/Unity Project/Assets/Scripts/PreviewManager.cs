using UnityEngine;

public class PreviewManager : MonoBehaviour
{
    public static PreviewManager Instance { get; private set; }

    /*** Adjustable values ***/

    public float defaultSize;

    public Color defaultColor;

    [SerializeField]
    private int defaultSides;

    [SerializeField]
    private Material defaultMaterial;

    /*** Private values ***/

    private bool activated;

    private GameObject lineRendererObj;

    private LineRenderer lineRenderer;

    /*** Temporary value(s) ***/

    private int currentCount;

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

        /*** Initialize the line renderer and assign inspector values ***/
        lineRendererObj = new GameObject("Line Renderer (Preview Manager)");
        lineRenderer = lineRendererObj.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        lineRenderer.material = defaultMaterial;
        lineRendererObj.SetActive(false);
    }

    public void StartPreview(Vector3 start, int? sides = null, float? size = null, Color? color = null)
    {
        if (activated == true)
        {
            return;
        }

        activated = true;

        sides ??= defaultSides;
        size ??= defaultSize;
        color ??= defaultColor;

        lineRendererObj.SetActive(true);
        lineRenderer.startWidth = lineRenderer.endWidth = size.Value;
        lineRenderer.material.color = color.Value;
        lineRenderer.numCapVertices = sides.Value;

        lineRenderer.positionCount = currentCount = 2;
        lineRenderer.SetPosition(0, start);
        UpdatePreview(start);
    }

    public void UpdatePreview(Vector3 end)
    {
        if (activated == false)
        {
            return;
        }

        lineRenderer.SetPosition(currentCount - 1, end);
    }

    public void AddPreview(Vector3 end)
    {
        if (activated == false)
        {
            return;
        }

        lineRenderer.positionCount = ++currentCount;
        UpdatePreview(end);
    }
    
    public void StopPreview()
    {
        if (activated == false)
        {
            return;
        }

        activated = false;
        lineRenderer.positionCount = 0;
        lineRendererObj.SetActive(false);
    }
}