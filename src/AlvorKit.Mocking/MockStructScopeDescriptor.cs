namespace AlvorKit.Mocking;

/// <summary>
/// Immutable struct selection metadata that retains no receiver value,
/// address, box, or managed reference.
/// </summary>
internal sealed class MockStructScopeDescriptor
{
    internal MockStructScopeDescriptor(
        Type structType,
        MockStructMode mode = MockStructMode.TypeWide,
        Delegate? predicate = null,
        MockCallSite? site = null)
    {
        ArgumentNullException.ThrowIfNull(structType);
        if (!structType.IsValueType || structType.IsByRefLike)
        {
            throw new MockException(
                $"Struct mocking requires a non-ref value type, not '{structType}'.");
        }

        bool valid = mode switch
        {
            MockStructMode.TypeWide =>
                predicate is null && site is null,
            MockStructMode.ValueMatched =>
                predicate is not null && site is null,
            MockStructMode.CallSite =>
                predicate is null && site is not null,
            _ => false
        };
        if (!valid)
        {
            throw new MockException(
                $"Struct mode '{mode}' has inconsistent predicate or site metadata.");
        }

        StructType = structType;
        Mode = mode;
        Predicate = predicate;
        Site = site;
    }

    internal Type StructType { get; }

    internal MockStructMode Mode { get; }

    internal Delegate? Predicate { get; }

    internal MockCallSite? Site { get; }

    internal MockStructScopeDescriptor Matching<T>(
        RefPredicate<T> predicate)
        where T : struct
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ValidateType<T>();
        RequireTypeWide(nameof(Matching));
        return new(
            StructType,
            MockStructMode.ValueMatched,
            predicate);
    }

    internal MockStructScopeDescriptor AtSite(MockCallSite site)
    {
        ArgumentNullException.ThrowIfNull(site);
        RequireTypeWide(nameof(AtSite));
        return new(
            StructType,
            MockStructMode.CallSite,
            site: site);
    }

    internal bool MatchesEntry<T>(scoped in T value)
        where T : struct
    {
        ValidateType<T>();
        return Mode switch
        {
            MockStructMode.TypeWide => true,
            MockStructMode.ValueMatched =>
                ((RefPredicate<T>)Predicate!)(in value),
            MockStructMode.CallSite => true,
            _ => throw new UnreachableException(
                $"Unknown struct mode '{Mode}'.")
        };
    }

    private void ValidateType<T>()
        where T : struct
    {
        if (StructType != typeof(T))
        {
            throw new MockException(
                $"Struct scope for '{StructType}' cannot inspect '{typeof(T)}'.");
        }
    }

    private void RequireTypeWide(string selection)
    {
        if (Mode != MockStructMode.TypeWide)
        {
            throw new MockException(
                $"Struct selection is already '{Mode}'. '{selection}' cannot " +
                "combine value and call-site identity.");
        }
    }
}
