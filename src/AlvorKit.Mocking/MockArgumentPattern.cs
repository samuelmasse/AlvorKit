namespace AlvorKit;

/// <summary>
/// Describes one heap-safe captured argument used by setup and verification
/// matching.
/// </summary>
internal readonly record struct MockArgumentPattern(object? Value)
{
    /// <summary>
    /// Returns whether an actual argument satisfies this captured pattern.
    /// </summary>
    internal bool Matches(object? actual) =>
        Value is Matcher matcher
            ? matcher.Matches(actual)
            : object.Equals(Value, actual);

    /// <summary>
    /// Evaluates only heap-safe matching and defers a live typed predicate.
    /// </summary>
    internal bool MatchesHeapSafe(object? actual) =>
        Value is Matcher { RequiresTypedEvaluation: true } || Matches(actual);

    /// <summary>Evaluates only a deferred live matcher for an accepted candidate.</summary>
    internal bool MatchesDeferred<T>(scoped in T actual)
        where T : allows ref struct =>
        !(Value is Matcher { RequiresTypedEvaluation: true } matcher) || matcher.MatchesTyped(in actual);


    /// <summary>Gets whether live typed evaluation is required.</summary>
    internal bool RequiresTypedEvaluation =>
        Value is Matcher { RequiresTypedEvaluation: true };

    /// <summary>
    /// Describes the pattern category without evaluating a predicate or
    /// formatting a captured user value.
    /// </summary>
    internal string Description =>
        Value switch
        {
            Matcher matcher => matcher.Description,
            null => "exact null",
            _ => "exact value"
        };
}
