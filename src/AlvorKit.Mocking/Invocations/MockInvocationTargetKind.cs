namespace AlvorKit.Mocking;

/// <summary>Identifies how an invocation target is owned.</summary>
internal enum MockInvocationTargetKind
{
    /// <summary>The target is one runtime-owned mock instance.</summary>
    Mock,

    /// <summary>The target is a receiver-free interception call site in a session.</summary>
    CallSite
}
