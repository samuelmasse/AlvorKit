namespace AlvorKit;

/// <summary>Configures heap-safe value behaviors on ref-safe setup clauses.</summary>
public static class MockSetupClauseExtensions
{
    /// <summary>Configures the captured call to return one ordinary heap-safe value.</summary>
    public static void Return<T>(
        this MockSetupClause<T> clause,
        T value)
    {
        ArgumentNullException.ThrowIfNull(clause);
        clause.AddOrdinaryReturn(value);
    }

    /// <summary>Configures successive ordinary returns, repeating the final value.</summary>
    public static void ReturnSequence<T>(
        this MockSetupClause<T> clause,
        params T[] values)
    {
        ArgumentNullException.ThrowIfNull(clause);
        ArgumentNullException.ThrowIfNull(values);
        clause.AddOrdinaryReturnSequence(values);
    }

    /// <summary>Calculates an ordinary heap-safe result from one invocation-local call context.</summary>
    public static void Answer<T>(
        this MockSetupClause<T> clause,
        Func<MockCall, T> answer)
    {
        ArgumentNullException.ThrowIfNull(clause);
        ArgumentNullException.ThrowIfNull(answer);
        clause.AddOrdinaryAnswer(answer);
    }

    /// <summary>Calculates a return from one live ref-safe argument.</summary>
    public static void Answer<T, TArgument>(
        this MockSetupClause<T> clause,
        Func<TArgument, T> callback)
        where T : allows ref struct
        where TArgument : allows ref struct
    {
        ArgumentNullException.ThrowIfNull(clause);
        ArgumentNullException.ThrowIfNull(callback);
        clause.AddTypedCallback(callback);
    }

    /// <summary>Calculates a return from three live ref-safe arguments.</summary>
    public static void Answer<T, T1, T2, T3>(
        this MockSetupClause<T> clause,
        Func<T1, T2, T3, T> callback)
        where T : allows ref struct
        where T1 : allows ref struct
        where T2 : allows ref struct
        where T3 : allows ref struct
    {
        ArgumentNullException.ThrowIfNull(clause);
        ArgumentNullException.ThrowIfNull(callback);
        clause.AddTypedCallback(callback);
    }

    /// <summary>Calculates a return through a delegate normalized to the captured exact signature.</summary>
    public static void Answer<T>(
        this MockSetupClause<T> clause,
        Delegate callback)
        where T : allows ref struct
    {
        ArgumentNullException.ThrowIfNull(clause);
        ArgumentNullException.ThrowIfNull(callback);
        clause.AddTypedCallback(callback);
    }

    /// <summary>
    /// Copies setup input once and returns mutable views over the same mock-owned
    /// storage on every matching call.
    /// </summary>
    public static void ReturnOwned<T>(
        this MockSetupClause<Span<T>> clause,
        scoped ReadOnlySpan<T> source)
    {
        ArgumentNullException.ThrowIfNull(clause);
        var owner = new MockOwnedSpanReturn<T>(source);
        clause.ReturnFactory(owner.Mutable);
    }

    /// <summary>
    /// Copies setup input once and returns read-only views over the same
    /// mock-owned storage on every matching call.
    /// </summary>
    public static void ReturnOwned<T>(
        this MockSetupClause<ReadOnlySpan<T>> clause,
        scoped ReadOnlySpan<T> source)
    {
        ArgumentNullException.ThrowIfNull(clause);
        var owner = new MockOwnedSpanReturn<T>(source);
        clause.ReturnFactory(owner.ReadOnly);
    }
}
