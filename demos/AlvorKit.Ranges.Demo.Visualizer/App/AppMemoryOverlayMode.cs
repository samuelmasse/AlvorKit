namespace AlvorKit;

/// <summary>Aggregate render modes for allocator backing-store strips.</summary>
public enum AppMemoryOverlayMode
{
    Allocations,
    Occupancy,
    Density,
    Efficiency,
    Fragmentation,
    Slack,
    Churn,
    Outliers,
    Relocation,
}
