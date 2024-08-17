using System.Collections.Generic;
using UnityEngine;

public class MeshUtils : MonoBehaviour
{
    // Private constructor (we do not need to instantiate this class).
    private MeshUtils() { }

    // Returns the triangle array for a generic n sided shape.
    public static int[] Triangulate(int sides)
    {
        int[] triangles = new int[(sides - 2) * 3];
        int triangleIndex = 0;

        for (int i = 0; i < sides - 2; i++)
        {
            triangles[triangleIndex++] = i + 2;
            triangles[triangleIndex++] = i + 1;
            triangles[triangleIndex++] = 0;
        }

        return triangles;
    }


    /* Returns the triangle array for a wavy n sided shape.
     * The function assumes that sides[0] is a peak vertex, not a trough.
     * Then you alternate based on this behavior to create the list.
     */
    public static int[] TriangulateWave(int sides, int segments)
    {
        int[] triangles = new int[(sides - 2) * 3];
        int triangleIndex = 0;
        List<int> peaksAndTroughs = new List<int>();

        for (int i = 0; i < sides; i += segments)
        {
            peaksAndTroughs.Add(i);
        }

        for (int i = 0; i < sides; i += 2)
        {
            int prevTrough = (i - 1 + sides) % sides;
            int nextTrough = (i + 1) % sides;
            triangles[triangleIndex++] = nextTrough;
            triangles[triangleIndex++] = i;
            triangles[triangleIndex++] = prevTrough;
        }

        int[] innerTriangles = Triangulate(peaksAndTroughs.Count / 2);

        for (int i = 0; i < innerTriangles.Length; i++)
        {
            triangles[triangleIndex++] = (innerTriangles[i] * 2) + 1;
        }

        return triangles;
    }

    /* Given the set of spline points and a perimeter, adds additional points to equally space
     * -each vertex by perimeter units. This is necessary for adding textures to the brush, which
     * -will repeat in multiples of perimeter.
     */
    public static List<Vector3> Verticize(List<Vector3> splinePoints, in float perimeter)
    {
        float currentPerimeter = perimeter;
        List<Vector3> newPoints = new List<Vector3>() { splinePoints[0] };

        for (int i = 1; i < splinePoints.Count; i++)
        {
            Vector3 curr = splinePoints[i];
            Vector3 prev = splinePoints[i - 1];
            float distance = Vector3.Distance(prev, curr);

            // Unlikely but would be very awesome if it happens
            if (currentPerimeter - distance == 0)
            {
                newPoints.Add(curr);
                continue;
            }

            // This is good, we can simply skip over and do no work
            if (currentPerimeter - distance > 0)
            {
                currentPerimeter -= distance;
            }
            else
            {
                float sum = 0;
                while (currentPerimeter - distance < 0)
                {
                    Vector3 newPoint = MathUtils.TravelAcross(prev, curr, currentPerimeter + sum);
                    sum += currentPerimeter;
                    distance -= currentPerimeter;
                    currentPerimeter = perimeter;
                    newPoints.Add(newPoint);
                }

                // Consider the equals edge case
                // Then we don't need to add 'curr'
                if (distance == 0)
                {
                    continue;
                }
            }

            newPoints.Add(curr);
        }

        return newPoints;
    }

    // TODO: implement for texture brush
    public static Vector2[] Texturize(int numPoints, int numSides, in float perimeter)
    {
        return null;
    }
}
