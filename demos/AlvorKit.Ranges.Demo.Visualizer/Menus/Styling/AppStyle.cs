namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Blend-backed UI style plus the allocator data-visualization palette.</summary>
[App]
public class AppStyle(
    RootFonts fonts,
    RootGl gl,
    RootUiScale scale,
    RootKeyboard keyboard) : BlendStyle(new()
{
    Font = fonts.Open(new() { File = Path.Combine(ProjectRoot.ResDirectory(typeof(AppStyle)), "fonts", "Inter.ttf") }),
    EmphasisFont = fonts.Open(new() { File = Path.Combine(ProjectRoot.ResDirectory(typeof(AppStyle)), "fonts", "Inter-SemiBold.ttf") }),
    Chrome = new BlendControlChrome(gl, scale),
    Keyboard = keyboard,
})
{
    /// <summary>Gets the backdrop color behind memory strips and the timeline lane.</summary>
    public Vec4 PanelInsetColor => Palette.AppBackground;

    /// <summary>Gets the strip outline thickness used by the data visualizations.</summary>
    public float RuleWidth => Metrics.Hairline;

    public Vec4 AccentColor => (0.25f, 0.8f, 0.95f, 1f);
    public Vec4 WarmAccentColor => (0.98f, 0.72f, 0.3f, 1f);
    public Vec4 FreeBlockColor => (0.13f, 0.25f, 0.22f, 1f);
    public Vec4 TailFreeBlockColor => (0.09f, 0.14f, 0.14f, 1f);
    public Vec4 PaddingColor => (0.95f, 0.62f, 0.2f, 0.78f);
    public Vec4 RetainedColor => (0.42f, 0.58f, 0.74f, 0.66f);
    public Vec4 LatestRequestFillColor => (0.9f, 1f, 1f, 0.38f);
    public Vec4 HighlightColor => (1f, 0.96f, 0.55f, 1f);
    public Vec4 OverlayFreeColor => (0.006f, 0.008f, 0.01f, 1f);
    public Vec4 OverlayOccupiedColor => (0.12f, 0.12f, 0.13f, 1f);
    public Vec4 OccupancyReservedColor => (0.32f, 0.02f, 0.025f, 1f);
    public Vec4 OccupancyPayloadColor => (0.96f, 0.06f, 0.035f, 1f);
    public Vec4 DensityLowColor => (0.18f, 0.025f, 0.03f, 1f);
    public Vec4 DensityMidColor => (0.85f, 0.2f, 0.08f, 1f);
    public Vec4 DensityHighColor => (1f, 0.92f, 0.72f, 1f);
    public Vec4 EfficiencyWasteColor => (0.9f, 0.08f, 0.05f, 1f);
    public Vec4 EfficiencyMixedColor => (0.96f, 0.72f, 0.12f, 1f);
    public Vec4 EfficiencyGoodColor => (0.28f, 0.9f, 0.32f, 1f);
    public Vec4 FragmentTinyColor => (0.95f, 0.1f, 0.06f, 1f);
    public Vec4 FragmentMediumColor => (0.98f, 0.64f, 0.12f, 1f);
    public Vec4 FragmentLargeColor => (0.2f, 0.7f, 0.84f, 1f);
    public Vec4 ChurnIdleColor => (0.16f, 0.17f, 0.18f, 1f);
    public Vec4 ChurnRecentColor => (1f, 0.78f, 0.2f, 1f);
    public Vec4 OutlierColor => (1f, 0.22f, 0.82f, 1f);
    public Vec4 RelocationNewColor => (0.22f, 0.84f, 1f, 1f);
    public Vec4 RelocationReusedColor => (0.28f, 0.88f, 0.44f, 1f);
    public Vec4 RelocationMovedColor => (0.96f, 0.34f, 1f, 1f);
    public Vec4 TimelineHoverColor => (1f, 1f, 1f, 0.18f);
    public Vec4 TimelineIdleOutlineColor => (0.16f, 0.2f, 0.23f, 1f);
    public Vec4 MemoryStripOutlineColor => (0.22f, 0.25f, 0.28f, 1f);
    public Vec4 MemoryActiveFrameFillColor => (1f, 0.96f, 0.55f, 0.16f);

    public Vec4 AllocationColor(int slot) => (slot % 8) switch
    {
        0 => (0.26f, 0.78f, 0.95f, 1f),
        1 => (0.95f, 0.46f, 0.52f, 1f),
        2 => (0.58f, 0.88f, 0.42f, 1f),
        3 => (0.98f, 0.72f, 0.3f, 1f),
        4 => (0.66f, 0.55f, 0.96f, 1f),
        5 => (0.35f, 0.88f, 0.72f, 1f),
        6 => (0.92f, 0.62f, 0.9f, 1f),
        _ => (0.68f, 0.78f, 0.92f, 1f),
    };

    public Vec4 CommandColor(AllocatorCommandKind kind) => kind switch
    {
        AllocatorCommandKind.Alloc => AccentColor,
        AllocatorCommandKind.Realloc => (0.55f, 0.9f, 0.42f, 1f),
        AllocatorCommandKind.Free => (0.95f, 0.34f, 0.36f, 1f),
        AllocatorCommandKind.Pack => (0.78f, 0.56f, 1f, 1f),
        _ => Palette.Text,
    };

    public Vec4 Dim(Vec4 color, float factor) => (color.X * factor, color.Y * factor, color.Z * factor, color.W);
}
