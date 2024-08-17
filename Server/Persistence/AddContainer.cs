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
}