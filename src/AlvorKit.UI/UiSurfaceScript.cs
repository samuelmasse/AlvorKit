namespace AlvorKit.UI;

/// <summary>Connects one non-default UI surface to the ordered root drawing pipeline.</summary>
internal sealed class UiSurfaceScript(
    RootUiSurfaces owner,
    UiSurface surface) : Script
{
    /// <inheritdoc />
    public override float Order => surface.Order;

    /// <inheritdoc />
    public override Vec2? DrawArea => surface.Size;

    /// <inheritdoc />
    public override Box2? DrawViewport => surface.CurrentViewport;

    /// <inheritdoc />
    public override void Draw()
    {
        // The default RootUiScript prepares every surface before surfaces ordered above it.
        // A surface ordered below the default must prepare itself before its earlier draw pass.
        if (surface.Order <= owner.Default.Order)
            owner.Prepare(surface);

        owner.Draw(surface);
    }

    /// <inheritdoc />
    public override void Unload() => owner.RemoveDrawScript(surface);
}
