using System;
using UnityEngine;
using static SerializeUtilities;

[Serializable]
public class AddContainer
{
    public enum Type { BRUSH, LINE, SHAPE, TEXT }

    public Type type;

    public uint id;

    public SVector3 pos, rot, sca;

    public BrushContainer brushContainer;

    public LineContainer lineContainer;

    public ShapeContainer shapeContainer;

    public TextContainer textContainer;

    public AddContainer() { }

    public AddContainer(BrushContainer brushContainer, Transform t)
    {
        type = Type.BRUSH;
        this.brushContainer = brushContainer;
        pos = new SVector3(t.position);
        rot = new SVector3(t.eulerAngles);
        sca = new SVector3(t.localScale);
    }

    public AddContainer(LineContainer lineContainer, Transform t)
    {
        type = Type.LINE;
        this.lineContainer = lineContainer;
        pos = new SVector3(t.position);
        rot = new SVector3(t.eulerAngles);
        sca = new SVector3(t.localScale);
    }

    public AddContainer(ShapeContainer shapeContainer, Transform t)
    {
        type = Type.SHAPE;
        this.shapeContainer = shapeContainer;
        pos = new SVector3(t.position);
        rot = new SVector3(t.eulerAngles);
        sca = new SVector3(t.localScale);
    }

    public AddContainer(TextContainer textContainer, Transform t)
    {
        type = Type.TEXT;
        this.textContainer = textContainer;
        pos = new SVector3(t.position);
        rot = new SVector3(t.eulerAngles);
        sca = new SVector3(t.localScale);
    }
}