public class SerializeUtilities
{
    private SerializeUtilities() { }

    [Serializable]
    public struct SVector3
    {
        public float x, y, z;
    }

    [Serializable]
    public struct SVector2
    {
        public float x, y;

        public readonly override string ToString() => $"{x},{y}";

        public readonly bool Equals(SVector2 v)
        {
            return x == v.x && y == v.y; 
        }
    }

    [Serializable]
    public struct SColor
    {
        public float r, g, b, a;
    }

    [Serializable]
    public struct SingleQueryObj
    {
        public SVector2 targetChunk;

        public uint targetID;
    }
}