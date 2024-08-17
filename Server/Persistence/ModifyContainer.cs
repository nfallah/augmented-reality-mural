using static SerializeUtilities;

[Serializable]
public class ModifyContainer
{
    public uint id;

    public SVector3 newPosition;

    public SVector3 newSize;

    public SVector3 newRotation;

    public SVector2 lastChunk;
}