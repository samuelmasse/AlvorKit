namespace AlvorKit.ECS;

/// <summary>Represents the first component in an archetypal query selection.</summary>
public readonly struct EntArchSelect<T, N, A> : IEntArchSelect<A>
{
    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Matches(int allocId, int archId) =>
        EntArchColumn<T, N, A>.ValuesAt(allocId, archId) != null;
}

/// <summary>Adds one component to an existing archetypal query selection.</summary>
public readonly struct EntArchSelect<T, N, A, TPrev> : IEntArchSelect<A>
    where TPrev : struct, IEntArchSelect<A>
{
    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Matches(int allocId, int archId) =>
        TPrev.Matches(allocId, archId) &&
        EntArchColumn<T, N, A>.ValuesAt(allocId, archId) != null;
}
