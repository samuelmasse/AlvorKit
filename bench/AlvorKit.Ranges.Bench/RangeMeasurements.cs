using static AlvorKit.RangePreparation;

namespace AlvorKit;

[Bench]
public class RangeMeasurements
{
    private long retained;

    public BenchResult SingleRangeAllocFree(int operations)
    {
        var counters = new RangeCounters();
        var allocator = new RangeAllocator(counters.Pack, counters.Resize);
        var allocation = 0;
        var address = 0L;
        var units = operations;
        var timer = BenchTimer.Start();
        var observed = RangeSingleRangeAllocFree.Run(allocator, operations, ref allocation, address);
        var result = timer.Stop(units, "allocation cycle");
        retained = observed;

        if (allocation != 0)
            allocator.Free(allocation);
        return result;
    }

    public BenchResult SteadyWindowChurn(int operations, int window)
    {
        var counters = new RangeCounters();
        var allocator = new RangeAllocator(counters.Pack, counters.Resize);
        var handles = new int[window];
        var address = 0L;
        var units = operations;
        var timer = BenchTimer.Start();
        var observed = RangeSteadyWindowChurn.Run(allocator, handles, operations, address);
        var result = timer.Stop(units, "allocation cycle");
        retained = observed;
        FreeLiveHandles(allocator, handles);
        return result;
    }

    public BenchResult SteadyWindowPrefilled(int operations, int window)
    {
        var counters = new RangeCounters();
        var allocator = new RangeAllocator(counters.Pack, counters.Resize);
        var handles = new int[window];
        var address = 0L;

        for (var i = 0; i < handles.Length; i++)
        {
            allocator.Alloc(ref handles[i], 8 << (i & 3), 24 + (i * 13 & 255));
            address ^= allocator.Addr(handles[i]);
        }

        var units = operations;
        var timer = BenchTimer.Start();
        var observed = RangeSteadyWindowPrefilled.Run(allocator, handles, operations, address);
        var result = timer.Stop(units, "allocation cycle");
        retained = observed;
        FreeLiveHandles(allocator, handles);
        return result;
    }

    public BenchResult LinearAllocNoResize(int operations)
    {
        var counters = new RangeCounters();
        var allocator = new RangeAllocator(
            counters.Pack,
            counters.Resize,
            FirstUsableIndex + (long)operations * (LinearMinSize + LinearSizeMask + LinearAlignment) + 1);
        var handles = new int[operations];
        var address = 0L;
        var units = operations;
        var timer = BenchTimer.Start();
        var observed = RangeLinearAllocNoResize.Run(allocator, handles, address);
        var result = timer.Stop(units, "allocation cycle");
        retained = observed;
        FreeLiveHandles(allocator, handles);
        return result;
    }

    public BenchResult LinearAllocWithResize(int operations)
    {
        var counters = new RangeCounters();
        var allocator = new RangeAllocator(counters.Pack, counters.Resize);
        var handles = new int[operations];
        var address = 0L;
        var units = operations;
        var timer = BenchTimer.Start();
        var observed = RangeLinearAllocWithResize.Run(allocator, handles, address);
        var result = timer.Stop(units, "allocation cycle");
        retained = observed;
        FreeLiveHandles(allocator, handles);
        return result;
    }

    public BenchResult SameHandleReuseHit(int operations)
    {
        var counters = new RangeCounters();
        var allocator = new RangeAllocator(counters.Pack, counters.Resize);
        var allocation = 0;
        var address = 0L;
        allocator.Alloc(ref allocation, LinearAlignment, 256);
        const int passes = 32;
        var units = operations * passes;
        var timer = BenchTimer.Start();
        var observed = RangeSameHandleReuseHit.Run(allocator, operations, passes, ref allocation, address);
        var result = timer.Stop(units, "request");
        retained = observed;
        allocator.Free(allocation);
        return result;
    }

    public BenchResult SameHandleGrowReplace(int operations)
    {
        var counters = new RangeCounters();
        var allocator = new RangeAllocator(counters.Pack, counters.Resize);
        var allocation = 0;
        var address = 0L;
        var units = operations;
        var timer = BenchTimer.Start();
        var observed = RangeSameHandleGrowReplace.Run(allocator, operations, ref allocation, address);
        var result = timer.Stop(units, "request");
        retained = observed;
        allocator.Free(allocation);
        return result;
    }

    public BenchResult SameHandleShrinkPack(int window)
    {
        var fixtures = new RangePackFixture[64];
        var allocators = new RangeAllocator[fixtures.Length];

        for (var i = 0; i < fixtures.Length; i++)
        {
            fixtures[i] = RangePackFixture.Shrunk(window);
            allocators[i] = fixtures[i].Allocator;
        }

        var units = window * fixtures.Length;
        var timer = BenchTimer.Start();
        RangeSameHandleShrinkPack.Run(allocators);
        var result = timer.Stop(units, "range");

        foreach (var fixture in fixtures)
        {
            retained += fixture.Observation;
            fixture.Dispose();
        }

        return result;
    }

    public BenchResult FragmentedSameSizeHoles(int operations, int window)
    {
        var counters = new RangeCounters();
        var allocator = new RangeAllocator(counters.Pack, counters.Resize, SameSizeHoleInitialSize(window));
        var separators = new int[window];
        var address = 0L;
        CreateSameSizeHoles(allocator, separators);
        var units = operations;
        var timer = BenchTimer.Start();
        var observed = RangeFragmentedSameSizeHoles.Run(allocator, operations, address);
        var result = timer.Stop(units, "allocation cycle");
        retained = observed;
        FreeLiveHandles(allocator, separators);
        return result;
    }

    public BenchResult FragmentedDistinctSizeHoles(int operations, int window)
    {
        var counters = new RangeCounters();
        var allocator = new RangeAllocator(counters.Pack, counters.Resize, DistinctSizeHoleInitialSize(window));
        var separators = new int[window];
        var address = 0L;
        CreateDistinctSizeHoles(allocator, separators);
        var units = operations;
        var timer = BenchTimer.Start();
        var observed = RangeFragmentedDistinctSizeHoles.Run(allocator, operations, window, address);
        var result = timer.Stop(units, "allocation cycle");
        retained = observed;
        FreeLiveHandles(allocator, separators);
        return result;
    }

    public BenchResult FragmentedPackScenario(int operations)
    {
        var counters = new RangeCounters();
        var allocator = new RangeAllocator(counters.Pack, counters.Resize);
        var handles = new int[operations];
        var address = 0L;
        var units = operations;
        var timer = BenchTimer.Start();
        var observed = RangeFragmentedPackScenario.Run(allocator, handles, address);
        var result = timer.Stop(units, "allocation cycle");
        retained = observed;
        FreeLiveHandles(allocator, handles);
        return result;
    }

    public BenchResult PackOnlyFragmented(int operations)
    {
        var fixtures = new RangePackFixture[8];
        var allocators = new RangeAllocator[fixtures.Length];

        for (var i = 0; i < fixtures.Length; i++)
        {
            fixtures[i] = RangePackFixture.Fragmented(operations);
            allocators[i] = fixtures[i].Allocator;
        }

        var units = PackLiveRangeCount(operations) * fixtures.Length;
        var timer = BenchTimer.Start();
        RangePackOnlyFragmented.Run(allocators);
        var result = timer.Stop(units, "range");

        foreach (var fixture in fixtures)
        {
            retained += fixture.Observation;
            fixture.Dispose();
        }

        return result;
    }

    public BenchResult PackCallbackSimulatedCopy(int operations)
    {
        var fixtures = new RangePackFixture[8];
        var allocators = new RangeAllocator[fixtures.Length];

        for (var i = 0; i < fixtures.Length; i++)
        {
            fixtures[i] = RangePackFixture.Relocated(operations);
            allocators[i] = fixtures[i].Allocator;
        }

        var units = PackLiveBytes(operations) * fixtures.Length;
        var timer = BenchTimer.Start();
        RangePackCallbackSimulatedCopy.Run(allocators);
        var result = timer.Stop(units, "logical byte visited");

        foreach (var fixture in fixtures)
        {
            retained += fixture.Observation;
            fixture.Dispose();
        }

        return result;
    }
}
