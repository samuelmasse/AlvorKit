namespace AlvorKit.ECS;

/// <summary>Describes the fields and values used to construct one final archetypal shape.</summary>
public interface IEntArchInit<A>
{
    /// <summary>Gets the number of fields in the final shape.</summary>
    static abstract int FieldCount { get; }

    /// <summary>Writes every registered field ID into the supplied span.</summary>
    static abstract void WriteFieldIds(Span<int> fieldIds);

    /// <summary>Writes every component value into its aligned final-arch column.</summary>
    void WriteValues(int allocId, int archId, int row);
}
