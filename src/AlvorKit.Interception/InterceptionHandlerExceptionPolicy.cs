namespace AlvorKit;

/// <summary>Controls the exception boundary of an exact managed handler trampoline.</summary>
public enum InterceptionHandlerExceptionPolicy
{
    /// <summary>Release the in-flight lease and propagate the original handler exception.</summary>
    Propagate,

    /// <summary>
    /// Record the exception, deactivate the handler, and return the exact default
    /// value; managed-reference returns are rejected because they have no safe default.
    /// </summary>
    ContainAndDeactivate
}
