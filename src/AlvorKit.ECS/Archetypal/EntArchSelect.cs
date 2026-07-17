namespace AlvorKit.ECS;

/// <summary>Represents the first component in an archetypal query selection.</summary>
public readonly struct EntArchSelect<T, N, A> : IEntArchSelect<A>
{
    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Matches(int _, int archId) =>
        EntArchGraph<A>.FindFieldOrdinal(archId, EntArchColumn<T, N, A>.FieldId) !=
        EntArchGraph<A>.NoFieldOrdinal;
}

/// <summary>Adds one component to an existing archetypal query selection.</summary>
public readonly struct EntArchSelect<T, N, A, TPrev> : IEntArchSelect<A>
    where TPrev : struct, IEntArchSelect<A>
{
    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Matches(int allocId, int archId) =>
        TPrev.Matches(allocId, archId) &&
        EntArchGraph<A>.FindFieldOrdinal(archId, EntArchColumn<T, N, A>.FieldId) !=
        EntArchGraph<A>.NoFieldOrdinal;
}
