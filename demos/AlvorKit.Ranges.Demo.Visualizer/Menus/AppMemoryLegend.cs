namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Projects the active memory overlay mode into viewport-header legend entries.</summary>
[App]
public class AppMemoryLegend(AppStyle s, AppSession session)
{
    /// <summary>Gets the display name of the active memory overlay mode.</summary>
    public string ModeName() => session.MemoryOverlayMode switch
    {
        AppMemoryOverlayMode.Allocations => "allocations",
        AppMemoryOverlayMode.Occupancy => "occupancy",
        AppMemoryOverlayMode.Density => "density",
        AppMemoryOverlayMode.Efficiency => "efficiency",
        AppMemoryOverlayMode.Fragmentation => "fragmentation",
        AppMemoryOverlayMode.Slack => "slack",
        AppMemoryOverlayMode.Churn => "churn",
        AppMemoryOverlayMode.Outliers => "outliers",
        AppMemoryOverlayMode.Relocation => "relocation",
        _ => "memory",
    };

    /// <summary>Gets the swatch color of a legend entry for the active mode.</summary>
    public Vec4 EntryColor(int index) => session.MemoryOverlayMode switch
    {
        AppMemoryOverlayMode.Allocations => index switch
        {
            0 => s.FreeBlockColor,
            1 => s.AllocationColor(0),
            2 => s.RetainedColor,
            3 => s.PaddingColor,
            _ => s.LatestRequestFillColor,
        },
        AppMemoryOverlayMode.Occupancy => index switch
        {
            0 => s.OverlayFreeColor,
            1 => s.OccupancyReservedColor,
            2 => s.OccupancyPayloadColor,
            3 => s.DensityHighColor,
            _ => s.HighlightColor,
        },
        AppMemoryOverlayMode.Density => index switch
        {
            0 => s.OverlayFreeColor,
            1 => s.DensityLowColor,
            2 => s.DensityMidColor,
            3 => s.DensityHighColor,
            _ => default,
        },
        AppMemoryOverlayMode.Efficiency => index switch
        {
            0 => s.OverlayFreeColor,
            1 => s.EfficiencyWasteColor,
            2 => s.EfficiencyMixedColor,
            3 => s.EfficiencyGoodColor,
            _ => default,
        },
        AppMemoryOverlayMode.Fragmentation => index switch
        {
            0 => s.OverlayOccupiedColor,
            1 => s.FragmentTinyColor,
            2 => s.FragmentMediumColor,
            3 => s.FragmentLargeColor,
            _ => s.TailFreeBlockColor,
        },
        AppMemoryOverlayMode.Slack => index switch
        {
            0 => s.OverlayFreeColor,
            1 => s.AllocationColor(0),
            2 => s.RetainedColor,
            3 => s.PaddingColor,
            _ => s.LatestRequestFillColor,
        },
        AppMemoryOverlayMode.Churn => index switch
        {
            0 => s.OverlayFreeColor,
            1 => s.ChurnIdleColor,
            2 => s.ChurnRecentColor,
            3 => s.HighlightColor,
            _ => default,
        },
        AppMemoryOverlayMode.Outliers => index switch
        {
            0 => s.OverlayFreeColor,
            1 => s.OverlayOccupiedColor,
            2 => s.OutlierColor,
            3 => s.DensityHighColor,
            _ => default,
        },
        AppMemoryOverlayMode.Relocation => index switch
        {
            0 => s.OverlayFreeColor,
            1 => s.RelocationReusedColor,
            2 => s.RelocationMovedColor,
            3 => s.RelocationNewColor,
            _ => s.OverlayOccupiedColor,
        },
        _ => default,
    };

    /// <summary>Gets the label of a legend entry for the active mode.</summary>
    public string EntryLabel(int index) => session.MemoryOverlayMode switch
    {
        AppMemoryOverlayMode.Allocations => index switch
        {
            0 => "free block",
            1 => "live payload",
            2 => "retained",
            3 => "padding",
            _ => "latest request",
        },
        AppMemoryOverlayMode.Occupancy => index switch
        {
            0 => "free",
            1 => "reserved",
            2 => "payload",
            3 => "dense",
            _ => "active",
        },
        AppMemoryOverlayMode.Density => index switch
        {
            0 => "empty",
            1 => "low",
            2 => "mixed",
            3 => "full",
            _ => "",
        },
        AppMemoryOverlayMode.Efficiency => index switch
        {
            0 => "free",
            1 => "waste",
            2 => "mixed",
            3 => "efficient",
            _ => "",
        },
        AppMemoryOverlayMode.Fragmentation => index switch
        {
            0 => "occupied",
            1 => "tiny holes",
            2 => "medium holes",
            3 => "large holes",
            _ => "tail",
        },
        AppMemoryOverlayMode.Slack => index switch
        {
            0 => "free",
            1 => "payload",
            2 => "retained",
            3 => "padding",
            _ => "latest",
        },
        AppMemoryOverlayMode.Churn => index switch
        {
            0 => "free",
            1 => "idle live",
            2 => "recent",
            3 => "latest",
            _ => "",
        },
        AppMemoryOverlayMode.Outliers => index switch
        {
            0 => "free",
            1 => "normal",
            2 => "outlier",
            3 => "severe",
            _ => "",
        },
        AppMemoryOverlayMode.Relocation => index switch
        {
            0 => "free",
            1 => "reused",
            2 => "moved",
            3 => "new",
            _ => "unchanged",
        },
        _ => "",
    };
}
