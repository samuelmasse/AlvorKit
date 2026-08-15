namespace AlvorKit;

/// <summary>Supported argument matcher strategies.</summary>
internal enum MatcherType
{
    /// <summary>Matches every actual argument value.</summary>
    Any,

    /// <summary>
    /// Matches non-null actual argument values accepted by a predicate.
    /// </summary>
    Func,

    /// <summary>
    /// Matches a live value through an exact typed predicate.
    /// </summary>
    TypedPredicate
}
