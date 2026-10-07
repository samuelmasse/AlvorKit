namespace AlvorKit;

[Bench]
public class RangeBenchmarks(RangeMeasurements measurements) : IBenchSuiteProvider
{
    public BenchSuite Create() => new("AlvorKit.Ranges.Bench", [Input("Small", 20000, 256), Input("Large", 200000, 2048)]);
    private BenchNode Input(string name, int operations, int window) => BenchNode.Group(
        name,
        $"{operations:N0} operations; {window:N0} live window or holes",
        [
            BenchNode.Measure(
                "SingleRangeAllocFree",
                "Measures repeated allocation and freeing of one reusable handle.",
                () => measurements.SingleRangeAllocFree(operations)),
            BenchNode.Measure(
                "SteadyWindowChurn",
                "Measures allocation churn with a live-handle window that starts empty.",
                () => measurements.SteadyWindowChurn(operations, window)),
            BenchNode.Measure(
                "SteadyWindowPrefilled",
                "Measures true steady-state allocation churn after the live-handle window is prefilled.",
                () => measurements.SteadyWindowPrefilled(operations, window)),
            BenchNode.Measure(
                "LinearAllocNoResize",
                "Measures linear allocation when the backing store is already large enough.",
                () => measurements.LinearAllocNoResize(operations)),
            BenchNode.Measure(
                "LinearAllocWithResize",
                "Measures linear allocation from the default backing size so growth remains visible.",
                () => measurements.LinearAllocWithResize(operations)),
            BenchNode.Measure(
                "SameHandleReuseHit",
                "32 passes of requests that hit the existing-handle reuse fast path.",
                () => measurements.SameHandleReuseHit(operations)),
            BenchNode.Measure(
                "SameHandleGrowReplace",
                "Measures repeated replacement of one handle with a monotonically growing request.",
                () => measurements.SameHandleGrowReplace(operations)),
            BenchNode.Measure(
                "SameHandleShrinkPack",
                "Measures packing retained slack created by same-handle shrink requests.",
                () => measurements.SameHandleShrinkPack(window)),
            BenchNode.Measure(
                "FragmentedSameSizeHoles",
                "Measures churn against many same-sized free blocks separated by live ranges.",
                () => measurements.FragmentedSameSizeHoles(operations, window)),
            BenchNode.Measure(
                "FragmentedDistinctSizeHoles",
                "Measures churn against many distinct-size free blocks separated by live ranges.",
                () => measurements.FragmentedDistinctSizeHoles(operations, window)),
            BenchNode.Measure(
                "FragmentedPackScenario",
                "Measures allocating, fragmenting, and packing one range set without timed teardown.",
                () => measurements.FragmentedPackScenario(operations)),
            BenchNode.Measure(
                "PackOnlyFragmented",
                "Measures only the allocator pack operation after fragmentation setup has completed.",
                () => measurements.PackOnlyFragmented(operations)),
            BenchNode.Measure(
                "PackCallbackSimulatedCopy",
                "Pack and visit relocation addresses and byte counts; no payload bytes are copied.",
                () => measurements.PackCallbackSimulatedCopy(operations))
    ]);
}
