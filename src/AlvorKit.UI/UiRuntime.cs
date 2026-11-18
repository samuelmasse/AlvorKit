namespace AlvorKit;

/// <summary>Owns UI trees, routes window input, and draws into the host's active sprite batch.</summary>
public class UiRuntime : IDisposable
{
    private readonly UiUpdate update = new();
    private readonly Keyboard keyboard;
    private readonly SpriteBatchWriter sprites;
    private readonly UiScale scale;
    private readonly UiContext context;
    private readonly UiFocus focus;
    private readonly UiMouse mouse;
    private readonly UiSurfaces surfaces;

    /// <summary>Gets the keyboard used for focus and text input.</summary>
    public Keyboard Keyboard => keyboard;
    /// <summary>Gets the writer used to measure and draw UI content.</summary>
    public SpriteBatchWriter Sprites => sprites;
    /// <summary>Gets the scale used for layout on the active surface.</summary>
    public UiScale Scale => scale;
    /// <summary>Gets the current surface viewport and logical size.</summary>
    public UiContext Context => context;
    /// <summary>Gets keyboard focus and navigation for this runtime.</summary>
    public UiFocus Focus => focus;
    /// <summary>Gets pointer dispatch and hover state for this runtime.</summary>
    public UiMouse Mouse => mouse;
    /// <summary>Gets the independently ordered surfaces in this window.</summary>
    public UiSurfaces Surfaces => surfaces;
    /// <summary>Gets the default full-window surface.</summary>
    public UiSurface Default => surfaces.Default;

    /// <summary>Creates a window-sized UI with the supplied input, drawing target, and initial pixel scale.</summary>
    public UiRuntime(WindowCanvas canvas, Keyboard keyboard, Mouse mouse, SpriteBatchWriter sprites, float initialScale)
    {
        this.keyboard = keyboard;
        this.sprites = sprites;
        scale = new(initialScale);
        context = new(canvas, scale);
        focus = new(keyboard);
        var clipping = new UiClipping();
        this.mouse = new(mouse, focus, clipping);
        surfaces = new(canvas, scale, context, update, sprites, clipping);
    }

    /// <summary>Dispatches input during the window update while key, button, wheel, and text edges are available.</summary>
    public void Update(double delta)
    {
        ObjectDisposedException.ThrowIf(surfaces.Span.IsEmpty, this);
        update.Advance(delta);
        surfaces.PrepareAll();
        mouse.Hover(surfaces);
        mouse.Update(surfaces);
        focus.Update(surfaces);
        surfaces.UpdateAll(update);
    }

    /// <summary>Draws a surface in its logical draw area. The host applies its viewport and begins and ends the batch.</summary>
    public void Draw(UiSurface surface)
    {
        if (surface.Owner != surfaces)
            throw new ArgumentException("The surface belongs to another UI runtime.", nameof(surface));

        ObjectDisposedException.ThrowIf(!surface.Root.IsAlive, surface);

        if (surface == Default)
        {
            surfaces.PrepareAll();
            mouse.Hover(surfaces);
            mouse.Draw();
        }
        else if (surface.Order <= Default.Order)
            surfaces.Prepare(surface);

        surfaces.Draw(surface);
    }

    /// <summary>Releases input and disposes every remaining surface tree.</summary>
    public void Dispose()
    {
        if (surfaces.Span.IsEmpty)
            return;

        mouse.Release();
        surfaces.Dispose();
    }
}
