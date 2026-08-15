namespace AlvorKit;

/// <summary>Defines fallback behavior for a newly-created full mock.</summary>
public enum MockBehavior
{
    /// <summary>Rejects an intercepted invocation without a matching setup.</summary>
    Strict,

    /// <summary>Returns stable default values for unmatched invocations.</summary>
    Loose
}
