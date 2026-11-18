namespace AlvorKit;

/// <summary>Drives a standalone UI runtime with scripted window input and a recording sprite batch.</summary>
internal class UiTestHarness : IDisposable
{
    private readonly FakeWindowHost host = new();
    private readonly UiTestGl backend = new();
    private readonly GlLayer gl;
    private readonly SpriteBatch batch;
    private readonly WindowLoop window;
    private readonly Mouse mouse;
    private readonly Keyboard keyboard;
    private readonly UiRuntime runtime;

    internal FakeWindowHost Host => host;
    internal UiTestGl Backend => backend;
    internal GlLayer Gl => gl;
    internal SpriteBatch Batch => batch;
    internal WindowLoop Window => window;
    internal Mouse Mouse => mouse;
    internal Keyboard Keyboard => keyboard;
    internal UiRuntime Runtime => runtime;
    internal UiRoot Ui => Runtime.Default.Root;
    internal UiScale Scale => Runtime.Scale;
    internal UiContext Context => Runtime.Context;
    internal UiSurfaces Surfaces => Runtime.Surfaces;
    internal UiMouse UiMouse => Runtime.Mouse;
    internal UiFocus Focus => Runtime.Focus;

    internal UiTestHarness()
    {
        window = new(Host);
        mouse = new(Window);
        keyboard = new(Window);
        gl = new(Backend);
        batch = new(Gl);
        runtime = new(new WindowCanvas(Window), Keyboard, Mouse, Batch.Writer, 1f);
        Window.Update += Runtime.Update;
    }

    internal void Frame()
    {
        Host.RaiseUpdate();
        Host.RaiseRender();
    }

    internal void Update() => Host.RaiseUpdate();

    internal void Draw()
    {
        Batch.Begin(Runtime.Default.Size);
        Runtime.Draw(Runtime.Default);
        Batch.End();
    }

    internal void MoveMouse(Vec2 position)
    {
        Host.MousePosition = position;
        Host.RaiseMouseMove(position);
    }

    public void Dispose()
    {
        Window.Update -= Runtime.Update;
        Runtime.Dispose();
        Batch.Dispose();
        Gl.Dispose();
    }
}
