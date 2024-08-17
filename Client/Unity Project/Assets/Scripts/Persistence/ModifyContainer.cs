using System;
using UnityEngine;
using static SerializeUtilities;

[Serializable]
public class ModifyContainer
{
    public uint id;

    public SVector3 newPosition;

    public SVector3 newSize;

    public SVector3 newRotation;

    public SVector2 lastChunk;

    public ModifyContainer() { }

    public ModifyContainer(uint id, Vector3 newPosition, Vector3 newSize, Vector3 newRotation,
                           Vector2 lastChunk)
    {
        this.id = id;
        this.newPosition = new SVector3(newPosition);
        this.newSize = new SVector3(newSize);
        this.newRotation = new SVector3(newRotation);
        this.lastChunk = new SVector2(lastChunk);
    }
}