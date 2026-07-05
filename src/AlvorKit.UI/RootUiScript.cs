namespace AlvorKit.UI;

[Root]
public class RootUiScript(
    RootCanvas canvas,
    RootUiScale scale,
    RootUiTraverse traverse,
    RootUiSize size,
    RootUiPosition position,
    RootUiDraw draw,
    RootUi ui,
    RootUiMouse mouse,
    RootUiFocus focus,
    RootUiUpdate update) : Script
{
    public override Vec2? DrawArea => canvas.Size / scale.Scale;

    /// <summary>
    /// Runs UI layout, input dispatch, and cleanup in the logical update phase. Input work must happen here:
    /// button edges, wheel offsets, and text runes are tick-scoped and cleared before the render-phase frame
    /// event, so render-phase dispatch misses every input that does not span a render.
    /// </summary>
    public override void Update(double delta)
    {
        ResetRoot();
        Traverse();
        mouse.Hover(ui);
        mouse.Update(ui);
        focus.Update(ui);
        update.Update(ui);
        ui.Cleanup();
    }

    public override void Draw()
    {
        ResetRoot();
        Traverse();
        mouse.Hover(ui);
        mouse.Draw();
        draw.Draw(ui);
    }

    /// <summary>
    /// Releases hover state and the hardware cursor shape when the script is removed, so a game that
    /// takes over input does not keep the last hovered control's cursor.
    /// </summary>
    public override void Unload() => mouse.Unload();

    private void ResetRoot()
    {
        ui.IsOrderedFV = true;
        ui.SizeFV = DrawArea.GetValueOrDefault();
        ui.SizeRelativeFV = (Vec2?)(0, 0);
    }

    private void Traverse()
    {
        do
        {
            traverse.Traverse(ui, null, 0);
            size.Size(ui.SizeR, ui);
            position.Position(ui.SizeR, default, ui);
            position.Finalize(ui.OffsetR, ui);
        }
        while (traverse.Delay(ui));
    }
}
