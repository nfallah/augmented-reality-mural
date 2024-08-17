using UnityEngine;

public class CubicBezier
{
    private CubicBezier() { }

    public static float Compute(float t, float p0, float p1, float p2, float p3)
    {
        t = Mathf.Clamp01(t);
        return Mathf.Pow(1 - t, 3) * p0 +                       // First term
               3 * Mathf.Pow(1 - t, 2) * t * p1 +               // Second term
               3 * Mathf.Pow(1 - t, 2) * Mathf.Pow(t, 2) * p2 + // Third term
               Mathf.Pow(t, 3) * p3;                            // Fourth term
    }

    public static Vector3 Compute(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return new Vector3(
            Compute(t, p0.x, p1.x, p2.x, p3.x),
            Compute(t, p0.y, p1.y, p2.y, p3.y),
            Compute(t, p0.z, p1.z, p2.z, p3.z)
            );
    }

    public static Vector3[] Compute(int totalPoints, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        /* We must have at least 2 points for the start and end positions,
         * This is our base case -- a linear line.
         */
        if (totalPoints < 2)
        {
            return null;
        }

        Vector3[] points = new Vector3[totalPoints];
        float stepSize = 1f / (totalPoints - 1);

        for (int i = 0; i < totalPoints; i++)
        {
            points[i] = Compute(i * stepSize, p0, p1, p2, p3);
        }

        return points;
    }
}