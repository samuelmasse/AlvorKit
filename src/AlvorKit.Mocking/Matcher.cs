namespace AlvorKit;

/// <summary>Argument matcher captured while setting up a mocked call.</summary>
internal readonly record struct Matcher(MatcherType Type, object? Object)
{
    /// <summary>Returns whether this matcher accepts an actual argument.</summary>
    internal bool Matches(object? actual) =>
        Type switch
        {
            MatcherType.Any => true,
            MatcherType.Func when actual is not null =>
                ((Func<object, bool>)Object!).Invoke(actual),
            MatcherType.Func => false,
            MatcherType.TypedPredicate => false,
            _ => throw new UnreachableException(
                $"Unknown matcher type '{Type}'.")
        };

    /// <summary>Returns whether a typed predicate accepts one live value.</summary>
    internal bool MatchesTyped<T>(scoped in T actual)
        where T : allows ref struct =>
        Type == MatcherType.TypedPredicate &&
        Object is MockTypedMatcher<T> matcher
            ? matcher.Matches(in actual)
            : throw new MockException(
                $"Typed matcher payload does not accept '{typeof(T)}'.");

    /// <summary>Gets whether live typed evaluation is required.</summary>
    internal bool RequiresTypedEvaluation =>
        Type == MatcherType.TypedPredicate;

    /// <summary>Matches a retained projector result without re-running live user code.</summary>
    internal bool MatchesProjected(object? actual) =>
        Type == MatcherType.TypedPredicate &&
        Object is MockTypedMatcher matcher &&
        matcher.MatchesProjected(actual);


    /// <summary>
    /// Describes the matcher without invoking its user-provided predicate.
    /// </summary>
    internal string Description =>
        Type switch
        {
            MatcherType.Any => "any value",
            MatcherType.Func => "predicate",
            MatcherType.TypedPredicate when Object is MockTypedMatcher matcher =>
                matcher.Description,
            _ => throw new UnreachableException(
                $"Unknown matcher type '{Type}'.")
        };
}
