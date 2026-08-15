namespace AlvorKit;

/// <summary>Maps one original IL offset to its generated-body IL offset.</summary>
public readonly record struct InterceptionGenerationIlMapEntry(
    uint OldOffset,
    uint NewOffset,
    bool Accurate = true);
