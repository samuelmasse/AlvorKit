namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>How a live allocation's backing range changed between two visualizer snapshots.</summary>
public enum AppRangeMotionKind
{
    None,
    New,
    Reused,
    Moved,
}
