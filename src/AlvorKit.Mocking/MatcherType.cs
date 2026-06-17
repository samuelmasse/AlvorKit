namespace AlvorKit.Mocking;

/// <summary>Supported argument matcher strategies.</summary>
internal enum MatcherType
{
    /// <summary>Matches every actual argument value.</summary>
    Any,

    /// <summary>Matches actual argument values accepted by a predicate.</summary>
    Func
}
