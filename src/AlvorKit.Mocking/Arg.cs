namespace AlvorKit;

/// <summary>Provides argument matchers for mocked value, reference, and by-ref parameters.</summary>
public static class Arg
{
    /// <summary>Matches any argument value of the requested type while configuring a mocked call.</summary>
    public static T Any<T>()
    {
        if (Capture.Context.IsActive)
            Capture.WriteMatcher(new(MatcherType.Any, null));

        return Value<T>();
    }

    /// <summary>Matches arguments accepted by a predicate while configuring a mocked call.</summary>
    public static T Match<T>(Func<T, bool> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        if (Capture.Context.IsActive)
        {
            Func<object, bool> f = o => func.Invoke((T)o);
            Capture.WriteMatcher(new(MatcherType.Func, f));
        }

        return Value<T>();
    }

    /// <summary>Returns the valid alternate used for ordinary matcher placement or the default outside capture.</summary>
    internal static T Value<T>()
    {
        if (Capture.Context.IsDisambiguating)
            return MockOrdinaryMatcherAlternate<T>.Value;
        else return default!;
    }

    /// <summary>Matches any value at one declared parameter index during active capture.</summary>
    public static T Any<T>(int parameterIndex)
        where T : allows ref struct
    {
        ArgumentOutOfRangeException.ThrowIfNegative(parameterIndex);
        Capture.WriteIndexedMatcher<T>(
            parameterIndex,
            MockIndexedMatcherPassingKind.Value,
            new(MatcherType.Any, null));
        return default!;
    }

    /// <summary>Matches a live value at one declared parameter index during active capture.</summary>
    public static T Match<T>(
        int parameterIndex,
        Func<T, bool> predicate)
        where T : allows ref struct
    {
        ArgumentOutOfRangeException.ThrowIfNegative(parameterIndex);
        ArgumentNullException.ThrowIfNull(predicate);
        bool typed(scoped in T value) => predicate(value);
        Capture.WriteIndexedMatcher<T>(
            parameterIndex,
            MockIndexedMatcherPassingKind.Value,
            new(
                MatcherType.TypedPredicate,
                new MockTypedMatcher<T>(typed, "predicate")));
        return default!;
    }

    /// <summary>Matches any mutable reference at one declared parameter index during active capture.</summary>
    public static ref T AnyRef<T>(int parameterIndex)
        where T : allows ref struct
    {
        ArgumentOutOfRangeException.ThrowIfNegative(parameterIndex);
        Capture.WriteIndexedMatcher<T>(
            parameterIndex,
            MockIndexedMatcherPassingKind.Reference,
            new(MatcherType.Any, null));
        return ref Unsafe.NullRef<T>();
    }

    /// <summary>Matches a live mutable reference at one declared parameter index during active capture.</summary>
    public static ref T Match<T>(
        int parameterIndex,
        RefPredicate<T> predicate)
        where T : allows ref struct
    {
        ArgumentOutOfRangeException.ThrowIfNegative(parameterIndex);
        ArgumentNullException.ThrowIfNull(predicate);
        Capture.WriteIndexedMatcher<T>(
            parameterIndex,
            MockIndexedMatcherPassingKind.Reference,
            new(
                MatcherType.TypedPredicate,
                new MockTypedMatcher<T>(predicate, "predicate")));
        return ref Unsafe.NullRef<T>();
    }

    /// <summary>Matches a read-only span against one setup-time content copy.</summary>
    public static ReadOnlySpan<T> ReadOnlySpanEqual<T>(
        int parameterIndex,
        ReadOnlySpan<T> expected)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(parameterIndex);
        var copy = expected.ToArray();
        bool predicate(scoped in ReadOnlySpan<T> actual) =>
                actual.SequenceEqual(copy);
        Capture.WriteIndexedMatcher<ReadOnlySpan<T>>(
            parameterIndex,
            MockIndexedMatcherPassingKind.Value,
            new(
                MatcherType.TypedPredicate,
                new MockTypedMatcher<ReadOnlySpan<T>>(
predicate,
                    "exact read-only span",
                    projected =>
                        projected is T[] values &&
                        values.AsSpan().SequenceEqual(copy))));
        return default;
    }

    /// <summary>Matches a mutable span against one setup-time content copy.</summary>
    public static Span<T> SpanEqual<T>(
        int parameterIndex,
        ReadOnlySpan<T> expected)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(parameterIndex);
        var copy = expected.ToArray();
        bool predicate(scoped in Span<T> actual) =>
                actual.SequenceEqual(copy);
        Capture.WriteIndexedMatcher<Span<T>>(
            parameterIndex,
            MockIndexedMatcherPassingKind.Value,
            new(
                MatcherType.TypedPredicate,
                new MockTypedMatcher<Span<T>>(
predicate,
                    "exact span",
                    projected =>
                        projected is T[] values &&
                        values.AsSpan().SequenceEqual(copy))));
        return default;
    }
}
