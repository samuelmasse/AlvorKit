namespace AlvorKit.UI;

[Root]
public class RootUiScript(
    RootUiSurfaces surfaces,
    RootUiMouse mouse,
    RootUiFocus focus,
    RootUiUpdate update) : Script
{
    /// <summary>Gets the logical draw area of the backward-compatible default full-window surface.</summary>
    public override Vec2? DrawArea => surfaces.Default.Size;

    /// <summary>
    /// Runs UI layout, input dispatch, and cleanup in the logical update phase. Input work must happen here:
    /// button edges, wheel offsets, and text runes are tick-scoped and cleared before the render-phase frame
    /// event, so render-phase dispatch misses every input that does not span a render.
    /// </summary>
    public override void Update(double delta)
    {
        surfaces.PrepareAll();
        mouse.Hover(surfaces);
        mouse.Update(surfaces);
        focus.Update(surfaces);
        surfaces.UpdateAll(update);
    }

    public override void Draw()
    {
        surfaces.PrepareAll();
        mouse.Hover(surfaces);
        mouse.Draw();
        surfaces.DrawDefault();
    }

    /// <summary>
    /// Releases hover state and the hardware cursor shape when the script is removed, so a game that
    /// takes over input does not keep the last hovered control's cursor.
    /// </summary>
    public override void Unload() => mouse.Unload();
}
