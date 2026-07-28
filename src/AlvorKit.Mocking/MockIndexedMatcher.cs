namespace AlvorKit.Mocking;

/// <summary>Identifies how an indexed matcher placeholder reaches its parameter.</summary>
internal enum MockIndexedMatcherPassingKind
{
    /// <summary>The matcher placeholder is passed by value or as an input reference.</summary>
    Value,

    /// <summary>The matcher placeholder is passed by mutable managed reference.</summary>
    Reference
}

/// <summary>Binds one matcher directly to a declared parameter index.</summary>
internal readonly record struct MockIndexedMatcher(
    int DeclaredIndex,
    Type ValueType,
    MockIndexedMatcherPassingKind PassingKind,
    Matcher Matcher);
