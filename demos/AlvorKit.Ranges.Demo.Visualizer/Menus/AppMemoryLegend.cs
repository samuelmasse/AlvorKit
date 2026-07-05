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

    /// <summary>Gets the tooltip text of a legend entry for the active mode.</summary>
    public string EntryTooltip(int index) => session.MemoryOverlayMode switch
    {
        AppMemoryOverlayMode.Allocations => index switch
        {
            0 => "free block\nreusable gap between reservations",
            1 => "live payload\nbytes a caller allocated and can use\neach slot gets its own color",
            2 => "retained capacity\nspare capacity kept after a shrink",
            3 => "padding\nalignment overhead around payloads",
            _ => "latest request\nwhite overlay marks the block the last command touched",
        },
        AppMemoryOverlayMode.Occupancy => index switch
        {
            0 => "free\nno reservation covers these bytes",
            1 => "reserved\nblock footprint without live payload",
            2 => "payload\nbytes callers are actively using",
            3 => "dense\npayload dominates this stretch of the store",
            _ => "active\nbytes touched by the latest command",
        },
        AppMemoryOverlayMode.Density => index switch
        {
            0 => "empty\nno payload in this stretch",
            1 => "low density\nmostly free with some payload",
            2 => "mixed density\nroughly half payload",
            3 => "full density\npayload fills this stretch",
            _ => "",
        },
        AppMemoryOverlayMode.Efficiency => index switch
        {
            0 => "free\nno reservation here",
            1 => "waste\nreserved but barely used: padding or retained slack",
            2 => "mixed\npartly efficient reservations",
            3 => "efficient\npayload fills the reservation",
            _ => "",
        },
        AppMemoryOverlayMode.Fragmentation => index switch
        {
            0 => "occupied\nreserved by live blocks",
            1 => "tiny holes\nfree gaps too small for most requests",
            2 => "medium holes\nmid-sized free gaps",
            3 => "large holes\nbig reusable free gaps",
            _ => "tail\nthe unused tail of the store",
        },
        AppMemoryOverlayMode.Slack => index switch
        {
            0 => "free\nno reservation here",
            1 => "payload\nbytes callers use",
            2 => "retained\nshrink slack kept for regrowth",
            3 => "padding\nalignment slack",
            _ => "latest\nblock touched by the last command",
        },
        AppMemoryOverlayMode.Churn => index switch
        {
            0 => "free\nno reservation here",
            1 => "idle live\nallocations untouched by recent commands",
            2 => "recent\nslots touched by the last commands",
            3 => "latest\nslot touched by the newest command",
            _ => "",
        },
        AppMemoryOverlayMode.Outliers => index switch
        {
            0 => "free\nno reservation here",
            1 => "normal\ntypical slots",
            2 => "outlier\nheavy slots: many ops, reallocs, or large sizes",
            3 => "severe\nstrongest outlier signal",
            _ => "",
        },
        AppMemoryOverlayMode.Relocation => index switch
        {
            0 => "free\nno reservation here",
            1 => "reused\nblock stayed at its previous address",
            2 => "moved\nblock relocated by the last command",
            3 => "new\nblock created by the last command",
            _ => "unchanged\nbytes not touched by the last command",
        },
        _ => "",
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
