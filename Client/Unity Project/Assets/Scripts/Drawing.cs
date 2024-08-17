using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Drawing
{
    public enum Type { BRUSH, LINE, SHAPE, TEXT }

    /*** GLOBAL VALUES ***/

    public Type type;

    // Unique value used as the primary way to communicate with the server.
    // Can also use as a key to a Dictionary pair in order to find
    // -the in-scene GameObject to modify.
    public Guid id;

    // Can other users select this object?
    public bool locked;

    //TODO: add other metadata alongisde locked like usr, last modified, etc.
    //...

    // The color of the object.
    public Color color;

    /*** BRUSH/LINE VALUES **/

    public float size;

    public int sides;

    public List<Vector3> splinePoints;

    public int textureID;

    public bool metallic;

    public bool wave;

    public float wavePeak;

    public float waveTrough;

    public int waveSides;

    /** BRUSH VALUES ***/

    public bool loop;

    /*** SHAPE VALUES ***/
    public ShapeContainer.Type shapeType;

    /*** SHAPE/TEXT VALUES ***/
    // Should be all we need to determine size, position, collider, etc.
    Vector3 startPos, endPos;
}