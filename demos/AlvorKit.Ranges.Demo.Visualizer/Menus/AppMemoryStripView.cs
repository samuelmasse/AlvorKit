namespace AlvorKit.Ranges.Demo.Visualizer;

public readonly record struct AppMemoryStripView(
    AllocatorSnapshot Snapshot,
    long ViewStart,
    long ViewEnd,
    string ViewName,
    bool MuteTail,
    bool DetailedLabels);
