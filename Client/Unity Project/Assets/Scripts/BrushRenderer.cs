using MixedReality.Toolkit;
using MixedReality.Toolkit.SpatialManipulation;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.ProBuilder;

public class BrushRenderer : MonoBehaviour
{
    public static BrushRenderer Instance { get; private set; }

    [Header("Mesh Values")]

    [SerializeField]
    private int defaultSides;

    [SerializeField]
    public float defaultSize;

    // *Removes* points!
    [SerializeField]
    private float defaultOptimizationThreshold;

    /* Adds points!
     * Ideally, the order should be to apply optimizations and then smoothness, but test and see
     * Which works better.
     */
    public float defaultSmoothnessThreshold;

    [SerializeField]
    private int defaultSmoothnessSegments;

    [SerializeField]
    public Color defaultColor;

    [SerializeField]
    private Material defaultMaterial;

    [Range(0, 180)]
    [SerializeField]
    private float angleOffset;

    //[SerializeField]
    public bool isWave;

    //[SerializeField]
    public int waveSides;

    // Treated as a ratio with respect to waveSize
    [Range(0, 1f)]
    //[SerializeField]
    public float waveTrough;

    // Treated as a ratio with respect to waveSize
    [Range(0, 1f)]
    //[SerializeField]
    public float wavePeak;

    //[SerializeField]
    public bool isMetallic;

    public bool loop;

    [Header("Debug Values")]

    /* Should we debug?
     * Inherently this is not used to not waste an if statement on release builds, but when
     * -debugging code, this can be used alongside the debug functions in this script.
     */
    [SerializeField]
    private bool debug;

    [SerializeField]
    private int debugSubdivisions;

    [SerializeField]
    private float debugSize;

    private void Awake()
    {
        /* Enforce a singleton state pattern */

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

    public GameObject GenerateMesh(bool isBrush, List<Vector3> splinePoints, int? sides = null, float? size = null,
                             Color? color = null, bool? isWave = null, bool? loop = null,
                             float? wavePeak = null, float? waveTrough = null,
                             int? waveSides = null, float? smoothnessThreshold = null,
                             int textureID = -1)
    {
        // Exit if splinePoints is null or invalid
        if (splinePoints == null || splinePoints.Count < 2)
        {
            return null;
        }

        // 2 nondistinct vertices when line is created.
        if (!isBrush && splinePoints[0] == splinePoints[1])
        {
            Debug.Log("NOOOOOO!!");
            return null;
        }

        /*** Assign default parameters if they were not provided in the function call ***/
        isWave ??= this.isWave;
        sides ??= defaultSides;
        sides = isWave.Value ? waveSides.Value : sides;

        // Exit if the number of sides is invalid
        if (sides < 3)
        {
            return null;
        }

        wavePeak ??= this.wavePeak;
        waveTrough ??= this.waveTrough;
        waveSides ??= this.waveSides;
        loop ??= this.loop;
        size ??= defaultSize;
        color ??= defaultColor;
        smoothnessThreshold ??= defaultSmoothnessThreshold;

        // Optimize the points first
        splinePoints = OptimizePoints(splinePoints);

        // Then smooth the spline points
        if (smoothnessThreshold.Value >= 0)
        {
            splinePoints = SmoothPoints(splinePoints, smoothnessThreshold.Value);
        }

        float perimeter = 0;

        // 'textureID' > -1 implies we want to apply a valid texture
        if (textureID > -1)
        {
            if (!isWave.Value)
            {
                float largeAngle = 360f / sides.Value;
                float smallAngle = (180f - largeAngle) / 2;
                float singleSide = size.Value * Mathf.Sin(largeAngle * Mathf.Deg2Rad) / Mathf.Sin(smallAngle * Mathf.Deg2Rad);
                perimeter = sides.Value * singleSide;
            }
            else
            {
                int innerSides = sides.Value / 2;
                float innerAngle = 360f / (innerSides * 2);
                float starHeight = (wavePeak.Value - waveTrough.Value) * size.Value;
                float innerHeight = size.Value - starHeight;
                float baseLength = innerHeight * Mathf.Tan(innerAngle * Mathf.Deg2Rad);
                float singleSide = Mathf.Sqrt(Mathf.Pow(starHeight, 2) + Mathf.Pow(baseLength, 2));
                perimeter = sides.Value * singleSide;
            }
            splinePoints = MeshUtils.Verticize(splinePoints, perimeter);
        }

        /*** Generate and populate a line renderer to calculate our initial rotation points ***/

        HashSet<Vector3> distinctVertices = new HashSet<Vector3>(splinePoints);

        // Brush must have at least 3 distinct vertices?
        if (isBrush && distinctVertices.Count < 3)
        {
            Debug.Log("NOOOO 2!");
            return null;
        }

        Mesh lr_mesh = new Mesh();
        LineRenderer lr = new GameObject().AddComponent<LineRenderer>();
        lr.positionCount = splinePoints.Count;
        lr.SetPositions(splinePoints.ToArray());
        lr.startWidth = lr.endWidth = size.Value;
        lr.BakeMesh(lr_mesh, true);
        Destroy(lr.gameObject);
        Vector3[] firstPoints = new Vector3[splinePoints.Count];

        for (int i = 0; i < splinePoints.Count; i++)
        {
            firstPoints[i] = lr_mesh.vertices[i * 2];
        }

        /*** Generate mesh vertices ***/

        Vector3[] vertices = new Vector3[splinePoints.Count * sides.Value];
        Vector3[] normals = new Vector3[splinePoints.Count * sides.Value];
        int listIndex;
        Vector3 direction;

        // Generate vertices in the range [1, n)
        for (int i = 0; i < splinePoints.Count - 1; i++)
        {
            listIndex = i * sides.Value;
            direction = splinePoints[i + 1] - splinePoints[i];
            GenerateVertices(splinePoints[i], direction, firstPoints[i], vertices, normals, listIndex,
                             sides.Value, size.Value, isWave.Value, wavePeak.Value,
                             waveTrough.Value);
        }

        /* Generate vertex n; this occurs outside of the for loop as a backward vector must now be
         * -calculated, which strays from the default behavior of computing a forward vector.
         */
        listIndex = (splinePoints.Count - 1) * sides.Value;
        direction = splinePoints[^1] - splinePoints[^2];
        GenerateVertices(splinePoints[^1], direction, firstPoints[^1], vertices, normals, listIndex,
                         sides.Value, size.Value, isWave.Value, wavePeak.Value,
                         waveTrough.Value);

        /*** Generate mesh triangles ***/

        int numTubeTriangles = (splinePoints.Count - 1) * sides.Value * 6;
        int numSideTriangles = !loop.Value ? (sides.Value - 2) * 6 : 0;
        int numLoopTriangles = loop.Value ? sides.Value * 6 : 0;
        int[] triangles = new int[numTubeTriangles + numSideTriangles + numLoopTriangles];

        for (int i = 0; i < splinePoints.Count - 1; i++)
        {
            int vertexStart = i * sides.Value;
            int vertexEnd = (i + 1) * sides.Value;
            int triangleStart = i * sides.Value * 6;
            GenerateTriangles(vertexStart, vertexEnd, triangleStart, triangles, sides.Value);
        }

        // Looping occurs *after* smoothing/optimization
        // TODO: ensure this works with the UV system because right now it does *NOT*!
        if (loop.Value)
        {
            int vertexStart = (splinePoints.Count - 1) * sides.Value;
            int vertexEnd = 0;
            int triangleStart = (splinePoints.Count - 1) * sides.Value * 6;
            GenerateTriangles(vertexStart, vertexEnd, triangleStart, triangles, sides.Value);
        }

        // Add side caps
        if (!loop.Value)
        {
            int[] sideTriangles = isWave.Value ? MeshUtils.TriangulateWave(waveSides.Value, 1) :
                                                 MeshUtils.Triangulate(sides.Value);
            int start = numTubeTriangles;

            for (int i = 0; i < numSideTriangles / 2; i++)
            {
                triangles[start++] = sideTriangles[i];
            }

            for (int i = (numSideTriangles / 2) - 1; i >= 0; i--)
            {
                triangles[start++] = sideTriangles[i] + sides.Value * (splinePoints.Count - 1);
            }
        }

        Mesh m = new Mesh();
        m.vertices = vertices;
        m.triangles = triangles;

        if (textureID > -1)
        {
            m.uv = MeshUtils.Texturize(splinePoints.Count, sides.Value, perimeter);
        }

        Vector3 center = m.bounds.center;

        for (int i = 0; i < m.vertices.Length; i++)
        {
            vertices[i] -= center;
        }

        m.vertices = vertices;
        m.normals = normals;
        m.RecalculateBounds();
        GameObject obj = new GameObject();
        MeshFilter mf = obj.AddComponent<MeshFilter>();
        MeshRenderer mr = obj.AddComponent<MeshRenderer>();
        mf.mesh = m;
        mr.material = defaultMaterial;

        if (isMetallic)
        {
            mr.material.SetFloat("_Metallic", 1);
            mr.material.SetFloat("_Glossiness", 1);
        }

        mr.material.color = color.Value;
        obj.transform.position = center;
        obj.AddComponent<MeshCollider>();
        StatefulInteractable interactable = obj.AddComponent<StatefulInteractable>();
        interactable.OnClicked.AddListener(() => StateManager.Instance.DrawClick(obj));
        // Temporarily disable the interactable here to avoid the stinky warnings. . . (bro called the warnings "stinky")
        interactable.enabled = false;
        ObjectManipulator manipulator = obj.AddComponent<ObjectManipulator>();
        manipulator.enabled = false;
        interactable.enabled = true;
        return obj;
    }

    // Generates the initial set of vertices based on point placements and other parameters.
    private void GenerateVertices(Vector3 position, Vector3 direction, Vector3 firstPoint,
                                  Vector3[] vertices, Vector3[] normals, int listIndex, int sides,
                                  float size, bool isWave, float wavePeak, float waveTrough)
    {
        float angleStep = 360f / sides;
        Vector3 firstDirection = (firstPoint - position).normalized;
        Vector3 perpendicularDirection = Vector3.Cross(direction, firstDirection).normalized;

        for (int i = 0; i < sides; i++)
        {
            float extra = 1f;

            if (isWave)
            {
                int waveSegments = 1;
                int ind = i % (waveSegments * 2);
                bool toPeak = ind >= waveSegments;
                float t = (float)(i % waveSegments) / waveSegments;

                // Going from trough to peak
                if (toPeak)
                {
                    extra = Mathf.Lerp(waveTrough, wavePeak, t);
                }
                // Going from peak to trough
                else
                {
                    extra = Mathf.Lerp(waveTrough, wavePeak, 1 - t);
                }
            }

            float angle = angleStep * i + angleOffset;
            Quaternion rotation = Quaternion.AngleAxis(angle, direction);
            Vector3 offset = rotation * perpendicularDirection * size * extra;
            vertices[listIndex + i] = position + offset;
            normals[listIndex + i] = rotation * perpendicularDirection;
        }
    }

    // Generates a series of trinagles for two given spline points
    private void GenerateTriangles(int startIndex, int endIndex, int triangleStart, int[] triangles,
                                   int sides)
    {
        for (int i = 0; i < sides; i++)
        {
            int startLast = i == sides - 1 ? startIndex : startIndex + i + 1;
            int endLast = i == sides - 1 ? endIndex : endIndex + i + 1;

            // First triangle
            triangles[triangleStart++] = endLast;
            triangles[triangleStart++] = endIndex + i;
            triangles[triangleStart++] = startIndex + i;

            // Second triangle
            triangles[triangleStart++] = startIndex + i;
            triangles[triangleStart++] = startLast;
            triangles[triangleStart++] = endLast;
        }
    }

    /* Eliminates extraneous points that are essentially too smooth.
     * Can achieve lossless compression if threshold == 0, and anything more is technically
     * -lossy. However, with low enough values, it saves a LOT of data points without affecting the
     * -finalized drawing.
     */
    private List<Vector3> OptimizePoints(List<Vector3> splinePoints, float? threshold = null,
                                         bool verbose = false)
    {
        threshold ??= defaultOptimizationThreshold;
        List<Vector3> newPoints = new List<Vector3>() { splinePoints[0] };
        int lastIndex = 0;

        for (int i = 1; i < splinePoints.Count - 1; i++)
        {
            Vector3 currentDirection = (splinePoints[i] - splinePoints[lastIndex]).normalized;
            Vector3 nextDirection = (splinePoints[i + 1] - splinePoints[i]).normalized;

            /* If the distance between the two directions is above our smoothness threshold,
             * We add the point since it is signifcant. Otherwise, the point is discarded.
             * Another way of going about this check is obtaining the angle between the two
             * -directions and seeing if it is below the threshold instead.
             */
            if (Vector3.Distance(currentDirection, nextDirection) > threshold.Value)
            {
                newPoints.Add(splinePoints[i]);
                lastIndex = i;
            }
        }

        newPoints.Add(splinePoints[^1]);

        if (verbose == true)
        {
            Debug.Log("Optimized " + (splinePoints.Count - newPoints.Count) + " data points!");
        }

        return newPoints;
    }

    // Adds additional points to the drawing to offset rough features like sharp turns, etc.
    private List<Vector3> SmoothPoints(List<Vector3> splinePoints, float threshold,
                                       int? segments = null)
    {
        segments ??= defaultSmoothnessSegments;
        List<Vector3> newPoints = new List<Vector3>() { splinePoints[0] };
        Vector3 prevPoint = splinePoints[0];

        for (int i = 1; i < splinePoints.Count - 1; i++)
        {
            // Direction of i - 1 to i
            Vector3 fromDir = (splinePoints[i] - prevPoint).normalized;
            // Direction of i to i + 1
            Vector3 toDir = (splinePoints[i + 1] - splinePoints[i]).normalized;
            // Angle between these two directions, [0, 180)
            float dirAngle = Vector3.Angle(fromDir, toDir);

            /* If the angle between directions is sharp, exclude the current point and add
             *-multiple quadric bezier interpolations from i - 1 to i + 1
             */
            if (dirAngle >= threshold)
            {
                Vector3[] interpolatedPoints = QuadraticBezier.Compute(segments.Value + 2, prevPoint,
                                               splinePoints[i], splinePoints[i + 1]);

                for (int j = 1; j < interpolatedPoints.Length - 1; j++)
                {
                    newPoints.Add(interpolatedPoints[j]);
                }

                prevPoint = newPoints[^1]; // The last interpolated point is our new last point.
            }
            // Otherwise, just keep the point since the angle is below the sharpness threshold.
            else
            {
                newPoints.Add(splinePoints[i]);
                prevPoint = splinePoints[i];
            }

        }
        
        // Add final point
        newPoints.Add(splinePoints[^1]);
        return newPoints;
    }

    public void DebugPoints(Vector3[] points, int? subdivisions = null, float? size = null)
    {
        subdivisions ??= debugSubdivisions;
        size ??= debugSize;
        Color color = new Color(UnityEngine.Random.Range(0f, 1), UnityEngine.Random.Range(0f, 1), UnityEngine.Random.Range(0f, 1));

        foreach (Vector3 point in points)
        {
            ProBuilderMesh debugMesh = ShapeGenerator.GenerateIcosahedron(PivotLocation.Center,
                                                                          size.Value,
                                                                          subdivisions.Value);
            debugMesh.gameObject.name = "Debug Point";
            debugMesh.transform.position = point;
            debugMesh.GetComponent<Renderer>().material.color = color;
        }
    }

    public void DebugVertices(Vector3[] vertices, int sides, int? subdivisions = null,
                              float? size = null)
    {
        subdivisions ??= debugSubdivisions;
        size ??= debugSize;
        Color[] colors = new Color[sides];

        for (int i = 0; i < sides; i++)
        {
            colors[i] = new Color(UnityEngine.Random.Range(0f, 1), UnityEngine.Random.Range(0f, 1), UnityEngine.Random.Range(0f, 1));
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            ProBuilderMesh debugMesh = ShapeGenerator.GenerateIcosahedron(PivotLocation.Center,
                                       size.Value, subdivisions.Value);
            debugMesh.gameObject.name = "Debug Vertex";
            debugMesh.transform.position = vertices[i];
            debugMesh.GetComponent<Renderer>().material.color = colors[i % sides];
        }
    }
}