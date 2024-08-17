using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathUtils
{
    // Private constructor (we do not need to instantiate this class).
    private MathUtils() { }

    public static bool Vector3Equals(Vector3 v1, Vector3 v2)
    {
        return Vector3.Distance(v1, v2) < 2*Mathf.Epsilon;
    }

    public static Vector3 Vector3Truncate(Vector3 v, float gridMultiplier = 1)
    {
        return new Vector3(
            (int)(v.x / gridMultiplier) * gridMultiplier,
            (int)(v.y / gridMultiplier) * gridMultiplier,
            (int)(v.z / gridMultiplier) * gridMultiplier);
    }

    public static Vector3 Vector3Abs(Vector3 v)
    {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }

    public static Vector3 Vector3Sign(Vector3 v)
    {
        return new Vector3(Mathf.Sign(v.x), Mathf.Sign(v.y), Mathf.Sign(v.z));
    }

    // Returns a point that is 'distance' units alongside the ray created from 'start' to 'end'
    public static Vector3 TravelAcross(Vector3 start, Vector3 end, float distance)
    {
        return new Ray(start, end - start).GetPoint(distance);
    }

    public static (Vector3?, Vector3?) GetMenuSelectPos(Vector3 start, GameObject endObj)
    {
        Vector3 end = endObj.transform.position;
        Ray r = new Ray(start, (end - start).normalized);
        int oldLayer = endObj.layer;
        // set temp layer for raycasting
        // 6 is a placeholder layer we use (called "Raycast" layer in menu!)
        int tempLayer = 1 << 6;
        Vector3? pos = null;
        Vector3? direction = null;
        endObj.layer = 6;
        if (Physics.Raycast(r, out RaycastHit hit, (end - start).magnitude + 5f, tempLayer))
        {
               //Debug.Log(hit.transform.gameObject.name);
//            Debug.Log(hit.point);
            pos = hit.point;
            direction = (start - end).normalized;
        }
        endObj.layer = oldLayer;
        return (pos, direction);
    }
}