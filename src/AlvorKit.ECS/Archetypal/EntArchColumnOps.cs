namespace AlvorKit.ECS;

internal abstract class EntArchColumnOps : EntComponentView
{
    internal abstract void EnsureCapacity(int allocId, int archId, int capacity, int count);

    internal abstract void ReduceCapacity(int allocId, int archId, int capacity, int count);

    internal abstract void Copy(int allocId, int srcArchId, int srcRow, int dstArchId, int dstRow);

    internal abstract void Clear(int allocId, int archId, int row);

    internal abstract void ClearAlloc(int allocId);

    internal abstract void AccumulateMetrics(ref EntArchMetrics metrics);
}
