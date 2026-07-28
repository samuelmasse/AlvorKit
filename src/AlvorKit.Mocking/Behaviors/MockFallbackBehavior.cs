namespace AlvorKit.Mocking;

/// <summary>Defines behavior when no configured setup matches a real call.</summary>
internal enum MockFallbackBehavior
{
    /// <summary>Rejects the unexpected invocation.</summary>
    Strict,

    /// <summary>Returns a stable default value.</summary>
    Loose,

    /// <summary>Continues into the original implementation.</summary>
    Partial
}
