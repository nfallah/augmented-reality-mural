using System;
using UnityEngine;
using static SerializeUtilities;

[Serializable]
public class ShapeContainer
{
    public enum Type { CUBE, SPHERE }

    public Type type;

    public SVector3 meshSize;

    public SVector3 meshPosition;

    public SColor meshColor;

    public ShapeContainer() { }

    public ShapeContainer(Type type, Vector3 meshSize, Vector3 meshPosition, Color meshColor)
    {
        this.type = type;
        this.meshSize = new SVector3(meshSize);
        this.meshPosition = new SVector3(meshPosition);
        this.meshColor = new SColor(meshColor);
    }
}