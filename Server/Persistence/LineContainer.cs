using static SerializeUtilities;

[Serializable]
public class LineContainer
{
    /* General Properties */
    public float meshSize;

    public int meshSides;

    public SVector3 meshPosition;

    public SColor meshColor;

    public List<SVector3> meshPoints;

    /* Wave Properties */
    public bool isWave;

    public float? waveBottomRatio;

    public float? waveTopRatio;

    /* Miscellaneous Properties */
    public bool isMetallic;

    public int? textureID;
}