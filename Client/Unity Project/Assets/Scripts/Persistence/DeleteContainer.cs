using System;

[Serializable]
public class DeleteContainer
{
    public uint id;

    public DeleteContainer() { }

    public DeleteContainer(uint id)
    {
        this.id = id;
    }
}