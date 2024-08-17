using static SerializeUtilities;

[Serializable]
public class ShapeContainer
{
    public enum Type { CUBE, SPHERE }

    public Type type;

    public SVector3 meshSize;

    public SVector3 meshPosition;

    public SColor meshColor;
}