namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>The allocator operation represented by one visualizer script step.</summary>
internal enum AllocatorCommandKind
{
    None,
    Alloc,
    Realloc,
    Free,
    Pack,
}

/// <summary>One deterministic allocator operation in a scenario script.</summary>
internal readonly record struct AllocatorCommand(
    AllocatorCommandKind Kind,
    int Slot,
    int Alignment,
    long Size,
    string Label)
{
    /// <summary>Creates the initial no-operation command used before the first scripted step.</summary>
    internal static AllocatorCommand Start() =>
        new(AllocatorCommandKind.None, 0, 0, 0, "start");

    /// <summary>Creates an allocation command for a handle slot.</summary>
    internal static AllocatorCommand Alloc(int slot, int alignment, long size, string label) =>
        new(AllocatorCommandKind.Alloc, slot, alignment, size, label);

    /// <summary>Creates a resize command for an existing handle slot.</summary>
    internal static AllocatorCommand Realloc(int slot, int alignment, long size, string label) =>
        new(AllocatorCommandKind.Realloc, slot, alignment, size, label);

    /// <summary>Creates a free command for a handle slot.</summary>
    internal static AllocatorCommand Free(int slot, string label) =>
        new(AllocatorCommandKind.Free, slot, 0, 0, label);

    /// <summary>Creates a pack command.</summary>
    internal static AllocatorCommand Pack(string label) =>
        new(AllocatorCommandKind.Pack, 0, 0, 0, label);
}
