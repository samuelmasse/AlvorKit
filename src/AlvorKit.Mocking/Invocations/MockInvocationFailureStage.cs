namespace AlvorKit;

/// <summary>Identifies which dispatch stage threw an invocation exception.</summary>
internal enum MockInvocationFailureStage
{
    /// <summary>An argument matcher threw.</summary>
    Matcher,

    /// <summary>An entry snapshot projector threw.</summary>
    EntryProjector,

    /// <summary>A live receiver mutation before behavior threw.</summary>
    EntryMutation,

    /// <summary>A configured behavior or strict fallback threw.</summary>
    Behavior,

    /// <summary>A typed return factory threw.</summary>
    ReturnFactory,

    /// <summary>The original implementation threw.</summary>
    OriginalImplementation,

    /// <summary>An exit snapshot projector threw.</summary>
    ExitProjector,

    /// <summary>A live receiver mutation after behavior threw.</summary>
    ExitMutation,

    /// <summary>An instrumentation continuation failed.</summary>
    BackendContinuation
}
