namespace AlvorKit.ECS;

/// <summary>Stores the first field value in a final archetypal shape.</summary>
public readonly struct EntArchInit<T, N, A>(T value) : IEntArchInit<A>
{
    /// <inheritdoc />
    public static int FieldCount => 1;

    /// <inheritdoc />
    public static void WriteFieldIds(Span<int> fieldIds) =>
        fieldIds[0] = EntArchColumn<T, N, A>.FieldId;

    /// <inheritdoc />
    public void WriteValues(int allocId, int archId, int row) =>
        EntArchColumn<T, N, A>.Values[EntArchRows<A>.RowSetIdAt(allocId, archId)][row] = value;
}

/// <summary>Adds one field value to an existing final archetypal shape.</summary>
public readonly struct EntArchInit<T, N, A, TPrev>(TPrev prev, T value) : IEntArchInit<A>
    where TPrev : struct, IEntArchInit<A>
{
    /// <inheritdoc />
    public static int FieldCount => TPrev.FieldCount + 1;

    /// <inheritdoc />
    public static void WriteFieldIds(Span<int> fieldIds)
    {
        TPrev.WriteFieldIds(fieldIds[..^1]);
        fieldIds[^1] = EntArchColumn<T, N, A>.FieldId;
    }

    /// <inheritdoc />
    public void WriteValues(int allocId, int archId, int row)
    {
        prev.WriteValues(allocId, archId, row);
        EntArchColumn<T, N, A>.Values[EntArchRows<A>.RowSetIdAt(allocId, archId)][row] = value;
    }
}
