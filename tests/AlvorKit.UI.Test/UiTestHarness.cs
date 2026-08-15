namespace AlvorKit;

/// <summary>Drives <see cref="RootUiScript"/> frames over fake windowing, GL, and input roots, mirroring the RootLoop wiring.</summary>
internal sealed class UiTestHarness
{
    /// <summary>Creates the windowing roots, UI systems, and script exactly as the root loop wires them.</summary>
    internal UiTestHarness()
    {
        Host = new FakeWindowHost();
        Window = new WindowLoop(Host);
        Mouse = new RootMouse(Window);
        Keyboard = new RootKeyboard(Window);

        var canvas = new RootCanvas(Window);
        var screen = new RootScreen(Window);
        Scale = new RootUiScale(new RootScale(screen));
        var gl = new RootGl(new UiTestGl());
        var sprites = new RootSprites(new SpriteBatch(gl));
        var traverse = new RootUiTraverse();
        var clipping = new RootUiClipping();
        var size = new RootUiSize(sprites, Scale);
        var position = new RootUiPosition(sprites, Scale);
        var draw = new RootUiDraw(sprites, Scale, position, clipping);

        Focus = new RootUiFocus(Keyboard);
        UiMouse = new RootUiMouse(Mouse, Focus, clipping);
        Ui = new RootUi();
        Scripts = new RootScripts();
        Context = new RootUiContext(canvas, Scale);
        Surfaces = new RootUiSurfaces(
            canvas,
            Scripts,
            Ui,
            Scale,
            Context,
            traverse,
            size,
            position,
            draw);
        Script = new RootUiScript(Surfaces, UiMouse, Focus, new RootUiUpdate());

        Window.Update += Script.Update;
    }

    /// <summary>Gets the scriptable window host feeding input events.</summary>
    internal FakeWindowHost Host { get; }

    /// <summary>Gets the window loop that owns tick state.</summary>
    internal WindowLoop Window { get; }

    /// <summary>Gets the mouse root read by the UI mouse system.</summary>
    internal RootMouse Mouse { get; }

    /// <summary>Gets the keyboard root read by the UI focus system.</summary>
    internal RootKeyboard Keyboard { get; }

    /// <summary>Gets the UI entity root that test trees mount under.</summary>
    internal RootUi Ui { get; }

    /// <summary>Gets the default surface scale shared by the test UI systems.</summary>
    internal RootUiScale Scale { get; }

    /// <summary>Gets the active surface context.</summary>
    internal RootUiContext Context { get; }

    /// <summary>Gets the surface registry used by the UI script.</summary>
    internal RootUiSurfaces Surfaces { get; }

    /// <summary>Gets the script registry receiving independent surface draw scripts.</summary>
    internal RootScripts Scripts { get; }

    /// <summary>Gets the UI mouse system under test.</summary>
    internal RootUiMouse UiMouse { get; }

    /// <summary>Gets the UI focus system.</summary>
    internal RootUiFocus Focus { get; }

    /// <summary>Gets the UI frame script under test.</summary>
    internal RootUiScript Script { get; }

    /// <summary>Runs one host loop iteration: the update phase (with input ticks) then the render phase.</summary>
    internal void Frame()
    {
        Host.RaiseUpdate();
        Host.RaiseRender();
    }

    /// <summary>Runs only the logical update phase, like agent gesture updates that render later.</summary>
    internal void Update() => Host.RaiseUpdate();

    /// <summary>Moves the cursor to a window-space position before the next frame.</summary>
    internal void MoveMouse(Vec2 position)
    {
        Host.MousePosition = position;
        Host.RaiseMouseMove(position);
    }
}
