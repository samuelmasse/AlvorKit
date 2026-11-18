namespace AlvorKit;

/// <summary>Runs UI input on engine ticks and gives each surface its own ordered draw pass.</summary>
[Root]
public class RootUiScript(RootUi ui, RootScripts scripts) : Script
{
    private readonly Dictionary<UiSurface, UiSurfaceScript> passes = [];

    public override Vec2? DrawArea => ui.Default.Size;

    public override void Load()
    {
        ui.Surfaces.Added += AddSurface;
        ui.Surfaces.Removed += RemoveSurface;

        foreach (var surface in ui.Surfaces.Span)
        {
            if (surface != ui.Default)
                AddSurface(surface);
        }
    }

    public override void Update(double delta) => ui.Update(delta);

    public override void Draw() => ui.Draw(ui.Default);

    public override void Unload()
    {
        ui.Surfaces.Added -= AddSurface;
        ui.Surfaces.Removed -= RemoveSurface;

        foreach (var pass in passes.Values)
            scripts.Remove(pass);

        passes.Clear();
        ui.Mouse.Release();
    }

    private void AddSurface(UiSurface surface)
    {
        var pass = new UiSurfaceScript(ui, surface);
        passes.Add(surface, pass);
        scripts.Add(pass);
    }

    private void RemoveSurface(UiSurface surface)
    {
        if (passes.Remove(surface, out var pass))
            scripts.Remove(pass);
    }
}
