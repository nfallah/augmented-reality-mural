using System;
using System.Collections.Generic;
using UnityEngine;
using static SerializeUtilities;

[Serializable]
public class BrushContainer
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
    public bool isLooped;
    
    public bool isMetallic;
    
    public int? textureID;

    public BrushContainer() { }

    public BrushContainer(float meshSize, int meshSides, Vector3 meshPosition, Color meshColor,
                          List<Vector3> meshPoints, bool isWave, float? waveBottomRatio,
                          float? waveTopRatio, bool isLooped, bool isMetallic, int? textureID)
    {
        this.meshSize = meshSize;
        this.meshSides = meshSides;
        this.meshPosition = new SVector3(meshPosition);
        this.meshColor = new SColor(meshColor);
        this.meshPoints = Vector3ToSVector3(meshPoints);
        this.isWave = isWave;
        this.waveBottomRatio = waveBottomRatio;
        this.waveTopRatio = waveTopRatio;
        this.isLooped = isLooped;
        this.isMetallic = isMetallic;
        this.textureID = textureID;
    }
}