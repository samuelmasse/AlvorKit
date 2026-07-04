namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>A deterministic allocator script shown by the visualizer.</summary>
public class AllocatorScenario(string name, string description, long initialSize, AllocatorCommand[] commands)
{
    /// <summary>Gets the scenario display name.</summary>
    public string Name { get; } = name;

    /// <summary>Gets a short scenario description.</summary>
    public string Description { get; } = description;

    /// <summary>Gets the allocator initial backing-store size.</summary>
    public long InitialSize { get; } = initialSize;

    /// <summary>Gets the deterministic command script.</summary>
    public AllocatorCommand[] Commands { get; } = commands;

    /// <summary>Gets the number of demo-owned handle slots needed by the script.</summary>
    public int HandleSlotCount { get; } = CountHandleSlots(commands);

    /// <summary>Creates every built-in visualizer scenario.</summary>
    public static AllocatorScenario[] CreateAll() =>
    [
        LinearAllocNoResize(),
        SameHandleReuseHit(),
        SameHandleShrinkPack(),
        SameHandleGrowReplace(),
        SteadyWindowPrefilled(),
        FragmentedSameSizeHoles(),
        FragmentedDistinctSizeHoles(),
        PackOnlyFragmented(),
        ResizeThreshold(),
    ];

    /// <summary>Counts the required handle slots from the highest slot touched by the script.</summary>
    private static int CountHandleSlots(ReadOnlySpan<AllocatorCommand> commands)
    {
        var max = 0;
        for (var i = 0; i < commands.Length; i++)
        {
            if (commands[i].Kind != AllocatorCommandKind.Pack)
                max = Math.Max(max, commands[i].Slot + 1);
        }

        return Math.Max(1, max);
    }

    /// <summary>Builds the simple tail-allocation scenario.</summary>
    private static AllocatorScenario LinearAllocNoResize()
    {
        List<AllocatorCommand> commands = [];
        for (var i = 0; i < 28; i++)
            commands.Add(AllocatorCommand.Alloc(i, 16, 72 + (i & 3) * 16, $"alloc slot {i}"));

        return new("linear-alloc-no-resize", "Tail block consumption without resize or pack.", 8192, [.. commands]);
    }

    /// <summary>Builds the same-handle reuse-hit scenario.</summary>
    private static AllocatorScenario SameHandleReuseHit()
    {
        List<AllocatorCommand> commands =
        [
            AllocatorCommand.Alloc(0, 16, 320, "initial slot 0"),
        ];

        for (var i = 0; i < 18; i++)
            commands.Add(AllocatorCommand.Realloc(0, 16, 96 + (i & 7) * 12, "reuse slot 0"));

        return new("same-handle-reuse-hit", "Repeated smaller requests keep the same backing range.", 2048, [.. commands]);
    }

    /// <summary>Builds a scenario where pack reclaims retained same-handle shrink capacity.</summary>
    private static AllocatorScenario SameHandleShrinkPack()
    {
        AllocatorCommand[] commands =
        [
            AllocatorCommand.Alloc(0, 1, 32, "prefix range"),
            AllocatorCommand.Alloc(1, 16, 320, "large slot 1"),
            AllocatorCommand.Realloc(1, 16, 96, "shrink slot 1"),
            AllocatorCommand.Free(0, "open front gap"),
            AllocatorCommand.Pack("pack shrunk slot"),
        ];

        return new("same-handle-shrink-pack", "A shrink keeps capacity until pack compacts to the logical size.", 2048, commands);
    }

    /// <summary>Builds the same-handle grow-replace scenario.</summary>
    private static AllocatorScenario SameHandleGrowReplace()
    {
        List<AllocatorCommand> commands =
        [
            AllocatorCommand.Alloc(0, 16, 64, "initial slot 0"),
        ];

        for (var i = 0; i < 18; i++)
            commands.Add(AllocatorCommand.Realloc(0, 16, 96 + i * 24, "grow slot 0"));

        return new("same-handle-grow-replace", "Growing the same handle frees the old range and takes a new one.", 8192, [.. commands]);
    }

    /// <summary>Builds a prefilled sliding-window churn scenario.</summary>
    private static AllocatorScenario SteadyWindowPrefilled()
    {
        const int window = 18;
        List<AllocatorCommand> commands = [];
        for (var i = 0; i < window; i++)
            commands.Add(AllocatorCommand.Alloc(i, 16, 96 + (i & 5) * 16, $"prefill {i}"));

        for (var i = 0; i < 30; i++)
        {
            var slot = i % window;
            commands.Add(AllocatorCommand.Free(slot, $"retire {slot}"));
            commands.Add(AllocatorCommand.Alloc(slot, 16, 80 + (i & 7) * 20, $"refill {slot}"));
        }

        return new("steady-window-prefilled", "A live window churns while the allocator reuses holes.", 8192, [.. commands]);
    }

    /// <summary>Builds a fragmented scenario with equal-size holes.</summary>
    private static AllocatorScenario FragmentedSameSizeHoles()
    {
        const int holes = 18;
        const int separators = 64;
        List<AllocatorCommand> commands = [];
        for (var i = 0; i < holes; i++)
        {
            commands.Add(AllocatorCommand.Alloc(i, 1, 64, $"hole seed {i}"));
            commands.Add(AllocatorCommand.Alloc(separators + i, 1, 1, $"separator {i}"));
        }

        for (var i = 0; i < holes; i++)
            commands.Add(AllocatorCommand.Free(i, $"open hole {i}"));

        for (var i = 0; i < holes; i++)
        {
            commands.Add(AllocatorCommand.Alloc(i, 1, 64, $"reuse hole {i}"));
            commands.Add(AllocatorCommand.Free(i, $"release hole {i}"));
        }

        return new("fragmented-same-size-holes", "Same-size holes demonstrate equal-size best-fit reuse.", 4096, [.. commands]);
    }

    /// <summary>Builds a fragmented scenario with distinct-size holes.</summary>
    private static AllocatorScenario FragmentedDistinctSizeHoles()
    {
        const int holes = 18;
        const int separators = 64;
        List<AllocatorCommand> commands = [];
        for (var i = 0; i < holes; i++)
        {
            commands.Add(AllocatorCommand.Alloc(i, 1, 32 + i * 8, $"seed {32 + i * 8}b"));
            commands.Add(AllocatorCommand.Alloc(separators + i, 1, 1, $"separator {i}"));
        }

        for (var i = 0; i < holes; i++)
            commands.Add(AllocatorCommand.Free(i, $"open {32 + i * 8}b"));

        for (var i = 0; i < holes; i++)
        {
            var size = 36 + (i % holes) * 8;
            commands.Add(AllocatorCommand.Alloc(i, 1, size, $"best fit {size}b"));
            commands.Add(AllocatorCommand.Free(i, $"release {size}b"));
        }

        return new("fragmented-distinct-size-holes", "Distinct holes show the size index finding the smallest fitting block.", 8192, [.. commands]);
    }

    /// <summary>Builds a pack-focused scenario with alternating live ranges.</summary>
    private static AllocatorScenario PackOnlyFragmented()
    {
        const int ranges = 36;
        List<AllocatorCommand> commands = [];
        for (var i = 0; i < ranges; i++)
            commands.Add(AllocatorCommand.Alloc(i, 16, 72 + (i & 7) * 12, $"alloc {i}"));

        for (var i = 0; i < ranges; i += 2)
            commands.Add(AllocatorCommand.Free(i, $"free gap {i}"));

        commands.Add(AllocatorCommand.Pack("pack live ranges"));
        return new("pack-only-fragmented", "Alternate ranges are freed, then live ranges compact toward the front.", 8192, [.. commands]);
    }

    /// <summary>Builds a scenario that crosses the allocator resize threshold.</summary>
    private static AllocatorScenario ResizeThreshold()
    {
        List<AllocatorCommand> commands = [];
        for (var i = 0; i < 22; i++)
            commands.Add(AllocatorCommand.Alloc(i, 16, 88 + (i & 3) * 24, $"alloc {i}"));

        return new("resize-threshold", "Capacity grows when the allocator cannot satisfy a near-full request.", 1024, [.. commands]);
    }
}
