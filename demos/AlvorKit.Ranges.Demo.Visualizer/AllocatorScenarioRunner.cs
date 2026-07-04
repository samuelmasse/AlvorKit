namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Runs one scenario against the real range allocator and captures visual snapshots.</summary>
internal sealed class AllocatorScenarioRunner
{
    private const long FirstUsableIndex = 1;
    private readonly List<AllocatorRangeVisual> ranges = [];
    private readonly List<AllocatorSpanVisual> freeSpans = [];
    private readonly AllocatorRangeComparer rangeComparer = new();
    private RangeAllocator allocator = null!;
    private int[] handles = [];
    private AllocatorScenario scenario = null!;
    private int packCount;
    private int resizeCount;

    /// <summary>Gets the current scenario.</summary>
    internal AllocatorScenario Scenario => scenario;

    /// <summary>Gets the number of applied commands.</summary>
    internal int StepIndex { get; private set; }

    /// <summary>Gets the snapshot captured before the latest operation.</summary>
    internal AllocatorSnapshot Previous { get; private set; } = AllocatorSnapshot.Empty;

    /// <summary>Gets the snapshot captured after the latest operation.</summary>
    internal AllocatorSnapshot Current { get; private set; } = AllocatorSnapshot.Empty;

    /// <summary>Gets the command applied most recently.</summary>
    internal AllocatorCommand LastCommand { get; private set; } = AllocatorCommand.Start();

    /// <summary>Gets the allocator method called by the latest operation.</summary>
    internal string LastMethodText { get; private set; } = "none";

    /// <summary>Gets the allocator arguments passed by the latest operation.</summary>
    internal string LastArgumentsText { get; private set; } = "scenario not stepped yet";

    /// <summary>Gets a compact allocator call display for the latest operation.</summary>
    internal string LastCallText { get; private set; } = "no allocator call yet";

    /// <summary>Loads a scenario from its initial allocator state.</summary>
    internal void Load(AllocatorScenario nextScenario)
    {
        scenario = nextScenario;
        packCount = 0;
        resizeCount = 0;
        handles = new int[scenario.HandleSlotCount];
        allocator = new RangeAllocator(() => packCount++, _ => resizeCount++, scenario.InitialSize);
        StepIndex = 0;
        LastCommand = AllocatorCommand.Start();
        ResetLastCallText();
        Current = Capture(0, 0);
        Previous = Current;
    }

    /// <summary>Applies the next scenario command when one exists.</summary>
    internal bool StepForward()
    {
        if (StepIndex >= scenario.Commands.Length)
            return false;

        Previous = Current;
        var command = scenario.Commands[StepIndex++];
        LastCommand = command;
        RecordLastCall(command);

        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        var started = Stopwatch.GetTimestamp();
        Apply(command);
        var elapsed = Stopwatch.GetTimestamp() - started;
        var allocatedAfter = GC.GetAllocatedBytesForCurrentThread();

        Current = Capture(elapsed, allocatedAfter - allocatedBefore);
        return true;
    }

    /// <summary>Replays the current scenario back one command.</summary>
    internal bool StepBackward()
    {
        if (StepIndex == 0)
            return false;

        ReplayTo(StepIndex - 1);
        return true;
    }

    /// <summary>Replays the current scenario to a specific applied command count.</summary>
    internal void ReplayTo(int targetStep)
    {
        targetStep = Math.Clamp(targetStep, 0, scenario.Commands.Length);
        var currentScenario = scenario;
        Load(currentScenario);
        if (targetStep == 0)
            return;

        for (var i = 0; i < targetStep - 1; i++)
        {
            LastCommand = scenario.Commands[i];
            RecordLastCall(LastCommand);
            Apply(LastCommand);
            StepIndex++;
        }

        Previous = Capture(0, 0);
        LastCommand = scenario.Commands[targetStep - 1];
        RecordLastCall(LastCommand);
        Apply(LastCommand);
        StepIndex++;
        Current = Capture(0, 0);
    }

    /// <summary>Jumps to the next pack command, applying it immediately when found.</summary>
    internal bool JumpToPack()
    {
        for (var i = StepIndex; i < scenario.Commands.Length; i++)
        {
            if (scenario.Commands[i].Kind != AllocatorCommandKind.Pack)
                continue;

            ReplayTo(i);
            return StepForward();
        }

        for (var i = 0; i < scenario.Commands.Length; i++)
        {
            if (scenario.Commands[i].Kind != AllocatorCommandKind.Pack)
                continue;

            ReplayTo(i);
            return StepForward();
        }

        return false;
    }

    /// <summary>Applies one allocator command to the real allocator.</summary>
    private void Apply(AllocatorCommand command)
    {
        switch (command.Kind)
        {
            case AllocatorCommandKind.Alloc:
            case AllocatorCommandKind.Realloc:
                allocator.Alloc(ref handles[command.Slot], command.Alignment, command.Size);
                break;
            case AllocatorCommandKind.Free:
                allocator.Free(handles[command.Slot]);
                handles[command.Slot] = 0;
                break;
            case AllocatorCommandKind.Pack:
                allocator.Pack();
                break;
            default:
                throw new InvalidOperationException($"Unsupported allocator command '{command.Kind}'.");
        }
    }

    /// <summary>Resets the displayed allocator call to the pre-script state.</summary>
    private void ResetLastCallText()
    {
        LastMethodText = "none";
        LastArgumentsText = "scenario not stepped yet";
        LastCallText = "no allocator call yet";
    }

    /// <summary>Records the allocator method and argument text before the command mutates handle state.</summary>
    private void RecordLastCall(AllocatorCommand command)
    {
        var handleBefore = command.Kind is AllocatorCommandKind.Alloc or AllocatorCommandKind.Realloc or AllocatorCommandKind.Free
            ? handles[command.Slot]
            : 0;

        switch (command.Kind)
        {
            case AllocatorCommandKind.Alloc:
            case AllocatorCommandKind.Realloc:
                LastMethodText = "allocator.Alloc";
                LastArgumentsText = $"ref handle[{command.Slot}]={handleBefore}, alignment={command.Alignment}, size={command.Size}";
                LastCallText = $"{LastMethodText}({LastArgumentsText})";
                break;
            case AllocatorCommandKind.Free:
                LastMethodText = "allocator.Free";
                LastArgumentsText = $"handle[{command.Slot}]={handleBefore}";
                LastCallText = $"{LastMethodText}({LastArgumentsText})";
                break;
            case AllocatorCommandKind.Pack:
                LastMethodText = "allocator.Pack";
                LastArgumentsText = "none";
                LastCallText = "allocator.Pack()";
                break;
            case AllocatorCommandKind.None:
                ResetLastCallText();
                break;
            default:
                throw new InvalidOperationException($"Unsupported allocator command '{command.Kind}'.");
        }
    }

    /// <summary>Captures allocator public state into sorted live and free ranges.</summary>
    private AllocatorSnapshot Capture(long elapsedTicks, long managedBytes)
    {
        ranges.Clear();
        freeSpans.Clear();

        var slots = allocator.AllocationSlots;
        for (var slotIndex = 0; slotIndex < handles.Length; slotIndex++)
        {
            var handle = handles[slotIndex];
            if (handle == 0)
                continue;

            var allocation = slots[handle];
            var reservedSize = ReservedSize(allocation);
            var payloadIndex = allocator.AlignedAddr(allocation.Index, allocation.Alignment);
            var leadingPadding = payloadIndex - allocation.Index;
            var trailingPadding = reservedSize - allocation.CapacitySize - leadingPadding;
            var retainedExtraSize = allocation.CapacitySize - allocation.Size;
            ranges.Add(new(
                slotIndex,
                handle,
                allocation.Index,
                payloadIndex,
                allocation.Size,
                allocation.CapacitySize,
                retainedExtraSize,
                reservedSize,
                leadingPadding,
                trailingPadding,
                allocation.Alignment));
        }

        ranges.Sort(rangeComparer);
        var cursor = FirstUsableIndex;
        for (var i = 0; i < ranges.Count; i++)
        {
            var range = ranges[i];
            if (range.Index > cursor)
                freeSpans.Add(new(cursor, range.Index - cursor));

            cursor = Math.Max(cursor, range.Index + range.ReservedSize);
        }

        if (cursor < allocator.Size)
            freeSpans.Add(new(cursor, allocator.Size - cursor));

        return new(
            [.. ranges],
            [.. freeSpans],
            allocator.Size,
            allocator.Used,
            allocator.FreeBlockCount,
            allocator.FreeSizeCount,
            allocator.IndexSetPoolCount,
            ranges.Count,
            packCount,
            resizeCount,
            allocator.PackTime,
            allocator.ResizeTime,
            elapsedTicks,
            managedBytes);
    }

    /// <summary>Returns the allocator's reserved size for one allocation slot.</summary>
    private static long ReservedSize(RangeAllocation allocation) =>
        allocation.CapacitySize + (allocation.Alignment <= 1 ? 0 : allocation.Alignment - 1L);

    /// <summary>Sorts visual ranges by backing-store index.</summary>
    private sealed class AllocatorRangeComparer : IComparer<AllocatorRangeVisual>
    {
        /// <inheritdoc />
        public int Compare(AllocatorRangeVisual x, AllocatorRangeVisual y) => x.Index.CompareTo(y.Index);
    }
}
