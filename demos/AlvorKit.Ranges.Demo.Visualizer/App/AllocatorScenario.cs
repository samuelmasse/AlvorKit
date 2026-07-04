namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>A deterministic allocator script shown by the visualizer.</summary>
public class AllocatorScenario(string name, string description, long initialSize, AllocatorCommand[] commands)
{
    private const long FirstUsableIndex = 1;
    private const int LinearAlignment = 16;
    private const int LinearMinSize = 32;
    private const int LinearSizeMask = 127;
    private const int HoleSize = 64;
    private const int SeparatorSize = 1;
    private const int ShrinkPackCapacity = 320;
    private const int ShrinkPackMinSize = 64;
    private const int ShrinkPackSizeMask = 15;
    private const int ShrinkPackSizeStep = 8;
    private const int LargeOperations = 20_000;
    private const int LargeWindow = 256;
    private const int LargeAlignmentMosaicRanges = 4_096;
    private const int LargeVariableReallocRanges = 40_000;
    private const int LargeVariableReallocPasses = 3;
    private const int LargeVariableReallocOutlierCount = 512;
    private const int LargeVariableReallocOutlierChangesPerPass = 8;
    private const int LargeVariableReallocCommandCount =
        LargeVariableReallocRanges * (LargeVariableReallocPasses + 1) +
        LargeVariableReallocOutlierCount * LargeVariableReallocPasses * LargeVariableReallocOutlierChangesPerPass;

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
        LargeLinearAllocNoResize(),
        LargeLinearAllocWithResize(),
        LargeSteadyWindowPrefilled(),
        LargeFragmentedSameSizeHoles(),
        LargeFragmentedDistinctSizeHoles(),
        LargeSameHandleShrinkPack(),
        LargeFragmentedPackScenario(),
        LargePackOnlyFragmented(),
        LargeAlignmentMosaic(),
        LargePackVsResizeThreshold(),
        LargeVariableReallocOutliers(),
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

    /// <summary>Builds a benchmark-scale linear allocation scenario that does not resize.</summary>
    private static AllocatorScenario LargeLinearAllocNoResize()
    {
        List<AllocatorCommand> commands = new(LargeOperations);
        AddLinearAllocCommands(commands, LargeOperations, "large linear alloc");

        return new(
            "large-linear-alloc-no-resize-20k",
            "20,000 tail allocations with a pre-sized backing store.",
            LinearInitialSize(LargeOperations),
            [.. commands]);
    }

    /// <summary>Builds a benchmark-scale linear allocation scenario from the default backing store.</summary>
    private static AllocatorScenario LargeLinearAllocWithResize()
    {
        List<AllocatorCommand> commands = new(LargeOperations);
        AddLinearAllocCommands(commands, LargeOperations, "large linear resize alloc");

        return new(
            "large-linear-alloc-with-resize-20k",
            "20,000 tail allocations starting from the default backing store.",
            RangeAllocator.DefaultInitialSize,
            [.. commands]);
    }

    /// <summary>Builds a benchmark-scale prefilled sliding-window churn scenario.</summary>
    private static AllocatorScenario LargeSteadyWindowPrefilled()
    {
        List<AllocatorCommand> commands = new(LargeWindow + LargeOperations * 2);
        for (var i = 0; i < LargeWindow; i++)
        {
            commands.Add(AllocatorCommand.Alloc(
                i,
                8 << (i & 3),
                24 + ((i * 13) & 255),
                "prefill large live window"));
        }

        for (var i = 0; i < LargeOperations; i++)
        {
            var slot = i % LargeWindow;
            commands.Add(AllocatorCommand.Free(slot, "retire large window slot"));
            commands.Add(AllocatorCommand.Alloc(
                slot,
                8 << (i & 3),
                24 + ((i * 13) & 255),
                "refill large window slot"));
        }

        return new(
            "large-steady-window-prefilled-256x20k",
            "256 live handles churn through 20,000 refill cycles.",
            RangeAllocator.DefaultInitialSize,
            [.. commands]);
    }

    /// <summary>Builds a benchmark-scale same-size fragmented-hole scenario.</summary>
    private static AllocatorScenario LargeFragmentedSameSizeHoles()
    {
        List<AllocatorCommand> commands = new(LargeWindow * 3 + LargeOperations * 2);
        AddSameSizeHoleSetup(commands, LargeWindow, LargeWindow);
        AddHoleChurn(commands, LargeWindow, LargeOperations, static i => HoleSize, "large same-size hole hit");

        return new(
            "large-fragmented-same-size-holes-256x20k",
            "256 equal holes are reused by 20,000 alloc/free cycles.",
            SameSizeHoleInitialSize(LargeWindow),
            [.. commands]);
    }

    /// <summary>Builds a benchmark-scale distinct-size fragmented-hole scenario.</summary>
    private static AllocatorScenario LargeFragmentedDistinctSizeHoles()
    {
        List<AllocatorCommand> commands = new(LargeWindow * 3 + LargeOperations * 2);
        AddDistinctSizeHoleSetup(commands, LargeWindow, LargeWindow);
        AddHoleChurn(commands, LargeWindow, LargeOperations, static i => 32 + (i % LargeWindow), "large distinct hole hit");

        return new(
            "large-fragmented-distinct-size-holes-256x20k",
            "256 distinct holes are searched by 20,000 alloc/free cycles.",
            DistinctSizeHoleInitialSize(LargeWindow),
            [.. commands]);
    }

    /// <summary>Builds a benchmark-scale same-handle shrink-and-pack scenario.</summary>
    private static AllocatorScenario LargeSameHandleShrinkPack()
    {
        List<AllocatorCommand> commands = new(LargeWindow * 2 + 1);
        for (var i = 0; i < LargeWindow; i++)
        {
            commands.Add(AllocatorCommand.Alloc(
                i,
                LinearAlignment,
                ShrinkPackCapacity + (i & ShrinkPackSizeMask) * ShrinkPackSizeStep,
                "large retained capacity"));
            commands.Add(AllocatorCommand.Realloc(
                i,
                LinearAlignment,
                ShrinkPackMinSize + (i & ShrinkPackSizeMask),
                "large same-handle shrink"));
        }

        commands.Add(AllocatorCommand.Pack("pack 256 shrunk live ranges"));

        return new(
            "large-same-handle-shrink-pack-256",
            "256 handles retain shrink slack until one pack reclaims it.",
            ShrinkPackInitialSize(LargeWindow),
            [.. commands]);
    }

    /// <summary>Builds a benchmark-scale fragmented pack scenario from the default backing store.</summary>
    private static AllocatorScenario LargeFragmentedPackScenario()
    {
        List<AllocatorCommand> commands = new(LargeOperations + LargeOperations / 2 + 1);
        AddFragmentedPackCommands(commands, LargeOperations);

        return new(
            "large-fragmented-pack-scenario-20k",
            "20,000 ranges allocate from default size, alternate gaps open, then pack.",
            RangeAllocator.DefaultInitialSize,
            [.. commands]);
    }

    /// <summary>Builds a benchmark-scale pack-only fragmented scenario with no setup resize.</summary>
    private static AllocatorScenario LargePackOnlyFragmented()
    {
        List<AllocatorCommand> commands = new(LargeOperations + LargeOperations / 2 + 1);
        AddFragmentedPackCommands(commands, LargeOperations);

        return new(
            "large-pack-only-fragmented-20k",
            "20,000 pre-sized ranges open alternating gaps, then pack 10,000 live ranges.",
            FragmentedPackInitialSize(LargeOperations),
            [.. commands]);
    }

    /// <summary>Builds a large alignment-heavy scenario that exposes padding pressure.</summary>
    private static AllocatorScenario LargeAlignmentMosaic()
    {
        List<AllocatorCommand> commands = new(LargeAlignmentMosaicRanges);
        for (var i = 0; i < LargeAlignmentMosaicRanges; i++)
        {
            commands.Add(AllocatorCommand.Alloc(
                i,
                8 << (i & 3),
                24 + ((i * 13) & 255),
                "large alignment mosaic"));
        }

        return new(
            "large-alignment-mosaic-4096",
            "4,096 allocations cycle alignments 8, 16, 32, and 64 to show padding.",
            AlignmentMosaicInitialSize(LargeAlignmentMosaicRanges),
            [.. commands]);
    }

    /// <summary>Builds a larger scenario that crosses the allocator's internal pack-vs-resize boundary.</summary>
    private static AllocatorScenario LargePackVsResizeThreshold()
    {
        const int ranges = 512;
        const int separatorBase = ranges;
        const int packProbeSlot = ranges * 2;
        const int resizeProbeSlot = packProbeSlot + 1;

        List<AllocatorCommand> commands = new(ranges * 2 + ranges / 2 + 2);
        for (var i = 0; i < ranges; i++)
        {
            commands.Add(AllocatorCommand.Alloc(i, LinearAlignment, 48 + (i & ShrinkPackSizeMask), "threshold payload"));
            commands.Add(AllocatorCommand.Alloc(separatorBase + i, LinearAlignment, 8, "threshold separator"));
        }

        for (var i = 0; i < ranges; i += 2)
            commands.Add(AllocatorCommand.Free(i, "open threshold gap"));

        commands.Add(AllocatorCommand.Alloc(packProbeSlot, LinearAlignment, 4096, "force internal pack"));
        commands.Add(AllocatorCommand.Alloc(resizeProbeSlot, LinearAlignment, 20_000, "force internal resize"));

        return new(
            "large-pack-vs-resize-threshold",
            "512 fragmented pairs force one internal pack, then one internal resize.",
            ThresholdInitialSize(ranges),
            [.. commands]);
    }

    /// <summary>Builds a large variable-size realloc scenario with repeated outlier spikes.</summary>
    private static AllocatorScenario LargeVariableReallocOutliers()
    {
        List<AllocatorCommand> commands = new(LargeVariableReallocCommandCount);
        for (var i = 0; i < LargeVariableReallocRanges; i++)
        {
            commands.Add(AllocatorCommand.Alloc(
                i,
                VariableReallocAlignment(i),
                VariableReallocInitialByteSize(i),
                "variable initial alloc"));
        }

        for (var pass = 0; pass < LargeVariableReallocPasses; pass++)
        {
            for (var i = 0; i < LargeVariableReallocRanges; i++)
            {
                commands.Add(AllocatorCommand.Realloc(
                    i,
                    VariableReallocAlignment(i),
                    VariableReallocSweepByteSize(i, pass),
                    "variable realloc sweep"));
            }

            for (var change = 0; change < LargeVariableReallocOutlierChangesPerPass; change++)
            {
                var outlierChange = pass * LargeVariableReallocOutlierChangesPerPass + change;
                for (var outlier = 0; outlier < LargeVariableReallocOutlierCount; outlier++)
                {
                    var slot = VariableReallocOutlierSlot(outlier);
                    commands.Add(AllocatorCommand.Realloc(
                        slot,
                        VariableReallocAlignment(slot),
                        VariableReallocOutlierByteSize(outlier, outlierChange),
                        "variable outlier realloc"));
                }
            }
        }

        return new(
            "large-variable-realloc-outliers-40k",
            "40,000 variable allocations resize through full sweeps while 512 outlier handles spike and shrink repeatedly.",
            VariableReallocInitialSize(LargeVariableReallocRanges),
            [.. commands]);
    }

    /// <summary>Adds linear allocation commands with the benchmark size pattern.</summary>
    private static void AddLinearAllocCommands(List<AllocatorCommand> commands, int count, string label)
    {
        for (var i = 0; i < count; i++)
            commands.Add(AllocatorCommand.Alloc(i, LinearAlignment, LinearMinSize + (i & LinearSizeMask), label));
    }

    /// <summary>Adds the setup for same-size holes separated by live one-byte ranges.</summary>
    private static void AddSameSizeHoleSetup(List<AllocatorCommand> commands, int holes, int separatorBase)
    {
        for (var i = 0; i < holes; i++)
        {
            commands.Add(AllocatorCommand.Alloc(i, 0, HoleSize, "large same-size hole seed"));
            commands.Add(AllocatorCommand.Alloc(separatorBase + i, 0, SeparatorSize, "large same-size separator"));
        }

        for (var i = 0; i < holes; i++)
            commands.Add(AllocatorCommand.Free(i, "open large same-size hole"));
    }

    /// <summary>Adds the setup for distinct-size holes separated by live one-byte ranges.</summary>
    private static void AddDistinctSizeHoleSetup(List<AllocatorCommand> commands, int holes, int separatorBase)
    {
        for (var i = 0; i < holes; i++)
        {
            commands.Add(AllocatorCommand.Alloc(i, 0, 32 + i, "large distinct hole seed"));
            commands.Add(AllocatorCommand.Alloc(separatorBase + i, 0, SeparatorSize, "large distinct separator"));
        }

        for (var i = 0; i < holes; i++)
            commands.Add(AllocatorCommand.Free(i, "open large distinct hole"));
    }

    /// <summary>Adds alloc/free churn against fragmented holes.</summary>
    private static void AddHoleChurn(List<AllocatorCommand> commands, int slots, int operations, Func<int, long> size, string label)
    {
        for (var i = 0; i < operations; i++)
        {
            var slot = i % slots;
            commands.Add(AllocatorCommand.Alloc(slot, 0, size(i), label));
            commands.Add(AllocatorCommand.Free(slot, "release large hole hit"));
        }
    }

    /// <summary>Adds the fragmented pack setup shared by the benchmark-scale pack scenarios.</summary>
    private static void AddFragmentedPackCommands(List<AllocatorCommand> commands, int ranges)
    {
        for (var i = 0; i < ranges; i++)
            commands.Add(AllocatorCommand.Alloc(i, LinearAlignment, LinearMinSize + (i & 63), "large pack alloc"));

        for (var i = 0; i < ranges; i += 2)
            commands.Add(AllocatorCommand.Free(i, "large pack gap"));

        commands.Add(AllocatorCommand.Pack("pack large fragmented set"));
    }

    /// <summary>Returns a no-resize initial size for benchmark-style linear allocation.</summary>
    private static long LinearInitialSize(int ranges) =>
        FirstUsableIndex + (long)ranges * (LinearMinSize + LinearSizeMask + LinearAlignment) + 1;

    /// <summary>Returns a no-resize initial size for benchmark-style fragmented pack setup.</summary>
    private static long FragmentedPackInitialSize(int ranges) =>
        FirstUsableIndex + (long)ranges * (LinearMinSize + 63 + LinearAlignment) + 1;

    /// <summary>Returns a no-resize initial size for benchmark-style same-size holes.</summary>
    private static long SameSizeHoleInitialSize(int holes) =>
        FirstUsableIndex + (long)holes * (HoleSize + SeparatorSize) + 1;

    /// <summary>Returns a no-resize initial size for benchmark-style distinct-size holes.</summary>
    private static long DistinctSizeHoleInitialSize(int holes)
    {
        var size = FirstUsableIndex;
        for (var i = 0; i < holes; i++)
            size += 32 + i + SeparatorSize;

        return size + 1;
    }

    /// <summary>Returns a no-resize initial size for same-handle shrink-pack setup.</summary>
    private static long ShrinkPackInitialSize(int ranges) =>
        FirstUsableIndex + (long)ranges * (ShrinkPackCapacity + ShrinkPackSizeMask * ShrinkPackSizeStep + LinearAlignment) + 1;

    /// <summary>Returns a no-resize initial size for the large alignment mosaic.</summary>
    private static long AlignmentMosaicInitialSize(int ranges) =>
        FirstUsableIndex + (long)ranges * (24 + 255 + MaxPadding(64)) + 1;

    /// <summary>Returns an initial size with only a tiny tail block after threshold setup.</summary>
    private static long ThresholdInitialSize(int ranges)
    {
        var size = FirstUsableIndex;
        for (var i = 0; i < ranges; i++)
        {
            size += 48 + (i & ShrinkPackSizeMask) + MaxPadding(LinearAlignment);
            size += 8 + MaxPadding(LinearAlignment);
        }

        return size + 1;
    }

    /// <summary>Returns a no-resize initial size for the large variable realloc setup.</summary>
    private static long VariableReallocInitialSize(int ranges)
    {
        var size = FirstUsableIndex;
        for (var i = 0; i < ranges; i++)
            size += VariableReallocInitialByteSize(i) + MaxPadding(VariableReallocAlignment(i));

        return size + 1;
    }

    /// <summary>Returns the alignment pattern for the large variable realloc scenario.</summary>
    private static int VariableReallocAlignment(int slot) => 8 << (slot & 3);

    /// <summary>Returns the initial size for a slot in the large variable realloc scenario.</summary>
    private static long VariableReallocInitialByteSize(int slot) => 48 + ((slot * 37) & 511);

    /// <summary>Returns the normal sweep size for a slot in the large variable realloc scenario.</summary>
    private static long VariableReallocSweepByteSize(int slot, int pass)
    {
        var initialSize = VariableReallocInitialByteSize(slot);
        var delta = 16 + ((slot * 17 + pass * 59) & 255);

        return ((slot + pass) & 1) == 0
            ? Math.Max(16L, initialSize - delta)
            : initialSize + delta;
    }

    /// <summary>Returns one deterministic outlier slot from the large variable realloc live set.</summary>
    private static int VariableReallocOutlierSlot(int outlier) => outlier * 73 % LargeVariableReallocRanges;

    /// <summary>Returns the repeated extreme size for an outlier slot in the large variable realloc scenario.</summary>
    private static long VariableReallocOutlierByteSize(int outlier, int change) =>
        (change & 1) == 0
            ? 16 + ((outlier * 31 + change * 23) & 127)
            : 2_048 + ((outlier * 197 + change * 997) & 8191);

    /// <summary>Returns the maximum padding any index can require for the given alignment.</summary>
    private static long MaxPadding(int alignment) => alignment <= 1 ? 0 : alignment - 1L;
}
