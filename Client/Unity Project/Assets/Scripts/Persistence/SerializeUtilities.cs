using System;
using System.Collections.Generic;
using UnityEngine;

public class SerializeUtilities
{
    private SerializeUtilities() { }

    [Serializable]
    public struct SVector3
    {
        public float x, y, z;

        public SVector3(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public readonly Vector3 ToVector3() => new(x, y, z);
    }

    [Serializable]
    public struct SVector2
    {
        public float x, y;

        public SVector2(Vector2 v)
        {
            x = v.x;
            y = v.y;
        }

        public readonly bool Equals(SVector2 v)
        {
            return x == v.x && y == v.y;
        }

        public readonly Vector2 ToVector2() => new(x, y);
    }

    [Serializable]
    public struct SColor
    {
        public float r, g, b, a;

        public SColor(Color c)
        {
            r = c.r;
            g = c.g;
            b = c.b;
            a = c.a;
        }

        public readonly Color ToColor() => new(r, g, b, a);
    }

    [Serializable]
    public struct SingleQueryObj
    {
        public SVector2 targetChunk;

        public uint targetID;
    }

    public static List<SVector3> Vector3ToSVector3(List<Vector3> vList)
    {
        List<SVector3> sList = new List<SVector3>();

        foreach (Vector3 v in vList)
        {
            sList.Add(new SVector3(v));
        }
        return sList;
    }

    public static List<Vector3> SVector3ToVector3(List<SVector3> sList)
    {
        List<Vector3> vList = new List<Vector3>();

        foreach (SVector3 s in sList)
        {
            vList.Add(s.ToVector3());
        }
        return vList;
    }
}