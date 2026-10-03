namespace AlvorKit;

/// <summary>Runs real Blend composition and UI phases over embedded Inter fonts, scripted input, and recording GL.</summary>
internal class BlendTestHarness : IDisposable
{
    private readonly FakeWindowHost host = new();
    private readonly BlendRecordingGl backend = new();
    private readonly RootScope root;
    private readonly GlLayer appGl;
    private readonly BlendUi bl;
    private readonly RootUiScript script;
    private readonly RootGraphics2D graphics;

    internal FakeWindowHost Host => host;
    internal BlendRecordingGl Backend => backend;
    internal RootScope Root => root;
    internal GlLayer AppGl => appGl;
    internal BlendUi Blend => bl;
    internal RootUi Ui => root.Get<RootUi>();

    internal BlendTestHarness() : this(BlendPalette.Default, new BlendMetrics()) { }

    internal BlendTestHarness(BlendPalette palette, BlendMetrics metrics)
    {
        var window = new WindowLoop(host);
        var gl = new RootGl(backend);
        var injector = new Injector();
        injector.Add(window);
        injector.Add<Ft>(new FtBackend());
        root = injector.Scope<RootScope>()
            .With(gl)
            .With(new RootCanvas(window))
            .With(new RootScreen(window))
            .With(new RootKeyboard(window))
            .With(new RootMouse(window))
            .With(new RootSprites(new SpriteBatch(gl)));
        appGl = new BlendTestGlLayer(gl);
        bl = new BlendUi(root.Get<RootBlend>(), appGl, palette, metrics);
        script = root.Get<RootUiScript>();
        root.Get<RootScripts>().Add(script);
        graphics = root.Get<RootGraphics2D>();
        window.Update += script.Update;
        host.MousePosition = (790, 590);
        host.RaiseMouseMove(host.MousePosition);
    }

    internal void Tick() => host.RaiseUpdate();
    internal void Draw() => graphics.Render();

    internal void Press(Keys key)
    {
        host.RaiseKeyDown(key);
        Tick();
        host.RaiseKeyUp(key);
        Tick();
    }

    public void Dispose()
    {
        graphics.Unload();
        root.Get<RootGl>().Dispose();
    }
}
