namespace AlvorKit;

/// <summary>Reports the exact metadata token produced for one relocation.</summary>
public readonly record struct InterceptionGenerationRelocationResult(
    ulong RequestId,
    ulong GenerationId,
    uint RelocationIndex,
    InterceptionGenerationRelocationKind Kind,
    int MetadataToken,
    int HResult);
