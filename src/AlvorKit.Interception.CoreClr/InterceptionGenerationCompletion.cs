namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Structured ABI v3 completion for one immutable method generation.</summary>
public readonly record struct InterceptionGenerationCompletion(
    ulong RequestId,
    ulong PatchId,
    ulong GenerationId,
    ulong PriorGenerationId,
    InterceptionState State,
    int HResult,
    InterceptionGenerationFailureStage FailureStage,
    uint? FailureRelocationIndex,
    uint RequestedRelocations,
    uint AppliedRelocations,
    uint RequestedIlMapEntries,
    uint AppliedIlMapEntries,
    ulong TargetRejitId);
