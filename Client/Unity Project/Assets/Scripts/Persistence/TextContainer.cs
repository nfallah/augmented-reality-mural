using System;
using UnityEngine;
using static SerializeUtilities;

[Serializable]
public class TextContainer
{
    public string text;

    public SVector3 meshSize;

    public SVector3 meshPosition;

    public SVector3 meshRotation;

    public SColor meshColor;

    public TextContainer() { }

    public TextContainer(string text, Vector3 meshSize, Vector3 meshPosition, Vector3 meshRotation,
                         Color meshColor)
    {
        this.text = text;
        this.meshSize = new SVector3(meshSize);
        this.meshPosition = new SVector3(meshPosition);
        this.meshRotation = new SVector3(meshRotation);
        this.meshColor = new SColor(meshColor);
    }
}