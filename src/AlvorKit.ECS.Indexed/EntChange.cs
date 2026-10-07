namespace AlvorKit;

/// <summary>A synchronous change view. Copy values explicitly to retain them beyond callback delivery.</summary>
public readonly ref struct EntChange<T>
{
    /// <summary>Borrows the pre-commit value for synchronous delivery only.</summary>
    private readonly ref readonly T? before;
    /// <summary>Borrows the committed slot for synchronous delivery only.</summary>
    private readonly ref readonly T? after;
    /// <summary>Presence before the operation, independent of the default value.</summary>
    private readonly bool wasPresent;
    /// <summary>Presence after the operation, independent of the default value.</summary>
    private readonly bool isPresent;

    /// <summary>Whether storage existed before this change.</summary>
    public bool WasPresent => wasPresent;
    /// <summary>Whether storage exists after this change.</summary>
    public bool IsPresent => isPresent;
    /// <summary>Prior value, or default when previously absent; copy explicitly to retain it.</summary>
    public ref readonly T? Before => ref before;
    /// <summary>Committed value, or default after removal; copy explicitly to retain it.</summary>
    public ref readonly T? After => ref after;

    /// <summary>Borrows both values without copying wide components into each callback argument.</summary>
    internal EntChange(in T? before, in T? after, bool wasPresent, bool isPresent)
    {
        this.before = ref before;
        this.after = ref after;
        this.wasPresent = wasPresent;
        this.isPresent = isPresent;
    }
}
