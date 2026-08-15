namespace AlvorKit;

/// <summary>Explains why invocation history has no retained argument value.</summary>
internal enum MockUnavailableReason
{
    /// <summary>A byref-like value had no configured heap-safe projector.</summary>
    ByRefLikeProjectionNotConfigured,

    /// <summary>An output parameter has no input value to retain.</summary>
    OutHasNoEntryValue,

    /// <summary>No exit projection was configured for the parameter.</summary>
    ExitProjectionNotConfigured,

    /// <summary>The invocation threw before a normal exit value was observed.</summary>
    NoNormalCompletion,

    /// <summary>A borrowed return was intentionally not retained.</summary>
    BorrowedReturnNotRetained,

    /// <summary>The active instrumentation backend cannot observe the value.</summary>
    BackendCannotObserve
}
