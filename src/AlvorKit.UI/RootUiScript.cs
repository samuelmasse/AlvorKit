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

    public override void Frame(double time)
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
