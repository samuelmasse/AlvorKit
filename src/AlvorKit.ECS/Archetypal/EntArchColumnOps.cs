namespace AlvorKit;

internal abstract class EntArchColumnOps : EntComponentView
{
    internal abstract void EnsureCapacity(int rowSetId, int capacity, int count);

    internal abstract void ReduceCapacity(int rowSetId, int capacity, int count);

    /// <summary>Moves the retained value and removes its source row in one column visit.</summary>
    internal abstract void Move(int srcRowSetId, int srcRow, int dstRowSetId, int dstRow, int lastRow);

    /// <summary>Swap-fills one removed row and clears reference-containing tail storage.</summary>
    internal abstract void Remove(int rowSetId, int row, int lastRow);

    internal abstract void ClearRowSet(int rowSetId);

    internal abstract void AccumulateMetrics(ref EntArchMetrics metrics);
}
