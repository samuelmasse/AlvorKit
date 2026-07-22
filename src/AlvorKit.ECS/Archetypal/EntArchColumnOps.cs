namespace AlvorKit.ECS;

internal abstract class EntArchColumnOps : EntComponentView
{
    internal abstract void EnsureCapacity(int rowSetId, int capacity, int count);

    internal abstract void ReduceCapacity(int rowSetId, int capacity, int count);

    internal abstract void Copy(int srcRowSetId, int srcRow, int dstRowSetId, int dstRow);

    internal abstract void Clear(int rowSetId, int row);

    internal abstract void ClearRowSet(int rowSetId);

    internal abstract void AccumulateMetrics(ref EntArchMetrics metrics);
}
