namespace AlvorKit.UI.Test;

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
        var scale = new RootUiScale(new RootScale(screen));
        var gl = new RootGl(new UiTestGl());
        var sprites = new RootSprites(new SpriteBatch(gl));
        var traverse = new RootUiTraverse();
        var clipping = new RootUiClipping();
        var size = new RootUiSize(sprites, scale);
        var position = new RootUiPosition(sprites, scale);
        var draw = new RootUiDraw(sprites, scale, position, clipping);

        Focus = new RootUiFocus(Keyboard);
        UiMouse = new RootUiMouse(Mouse, scale, Focus, clipping);
        Ui = new RootUi();
        Script = new RootUiScript(canvas, scale, traverse, size, position, draw, Ui, UiMouse, Focus, new RootUiUpdate());

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
