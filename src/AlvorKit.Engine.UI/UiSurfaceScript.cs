namespace AlvorKit;

/// <summary>Draws one additional surface at its position among all engine scripts.</summary>
internal class UiSurfaceScript(UiRuntime ui, UiSurface surface) : Script
{
    public override float Order => surface.Order;
    public override Vec2? DrawArea => surface.Size;
    public override Box2? DrawViewport => surface.CurrentViewport;
    public override void Draw() => ui.Draw(surface);
}
