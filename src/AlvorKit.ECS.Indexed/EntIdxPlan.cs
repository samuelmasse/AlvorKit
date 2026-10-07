namespace AlvorKit;

/// <summary>Intrusive ownership keeps plan release allocation-free without a second registration collection.</summary>
internal abstract class EntIdxPlan(EntIdxPlan? next)
{
    /// <summary>Next plan owned by the same context.</summary>
    internal EntIdxPlan? Next => next;

    /// <summary>Clears the typed registry entry before the context slot can be recycled.</summary>
    internal abstract void Release(int contextIndex);
}
