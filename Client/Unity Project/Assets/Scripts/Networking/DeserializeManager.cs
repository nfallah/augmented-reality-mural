using UnityEngine;
using static AddContainer;

public class DeserializeUtilities : MonoBehaviour
{
    private DeserializeUtilities() { }

    public static GameObject Regenerate(AddContainer addContainer)
    {
        switch (addContainer.type)
        {
            case Type.BRUSH:
                return BrushTool.Instance.Regenerate(addContainer.brushContainer);
            case Type.LINE:
                return LineTool.Instance.Regenerate(addContainer.lineContainer);
            case Type.SHAPE:
                return ShapeTool.Instance.Regenerate(addContainer.shapeContainer);
            case Type.TEXT:
                return TextTool.Instance.Regenerate(addContainer.textContainer);
            default:
                Debug.LogWarning("Regenerate: enum not implemented.");
                return null;
        }
    }
}