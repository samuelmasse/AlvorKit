namespace AlvorKit.ECS;

internal abstract class EntArchColumnOps
{
    internal abstract void Resize(int allocId, int archId, int capacity);

    internal abstract void Copy(int allocId, int srcArchId, int srcRow, int dstArchId, int dstRow);

    internal abstract void Clear(int allocId, int archId, int row);

    internal abstract void AccumulateMetrics(ref EntArchMetrics metrics);
}
