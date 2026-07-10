namespace AlvorKit.ECS;

internal abstract class EntArchFieldHandler
{
    internal abstract void Resize(int allocatorId, int archId, int capacity);

    internal abstract void Move(int allocatorId, int srcArchId, int srcRow, int dstArchId, int dstRow);

    internal abstract void Clear(int allocatorId, int archId, int row);
}
