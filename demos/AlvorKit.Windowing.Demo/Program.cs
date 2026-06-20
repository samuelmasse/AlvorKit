const int MaxColorStep = 10;
var random = new Random(43);

var glfw = new GlfwBackend();
if (!glfw.Init())
    throw new InvalidOperationException("Failed to initialize GLFW.");

glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 3);
glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 3);
glfw.WindowHint(GlfwWindowHint.OpenGLProfile, GlfwOpenGLProfile.CoreProfile);
glfw.WindowHint(GlfwWindowHint.Visible, false);
var window = glfw.CreateWindow(960, 640, "AlvorKit.Windowing demo", default, default);
if (window == default)
{
    glfw.Terminate();
    throw new InvalidOperationException("Failed to create the GLFW window.");
}

glfw.MakeContextCurrent(window);
var gl = new GlLayer(new GlBackend(glfw.GetProcAddress));
using var host = new AgentGlfwWindowHost(glfw, window, gl)
{
    IsVSyncEnabled = true
};
using var loop = new WindowLoop(host);
var canvas = new WindowCanvas(loop);

gl.GetString(GlStringName.Version, out var version);
Console.WriteLine($"OpenGL {version} - Esc exits; F11 fullscreen; F12 vsync; arrows, mouse, wheel, and text are tracked.");

RunDemoLoop(loop, canvas, gl);
return 0;

// Wires the windowing facades, demo controls, update logic, and OpenGL rendering.
void RunDemoLoop(WindowLoop loop, WindowCanvas canvas, GlLayer gl)
{
    var mouse = new Mouse(loop);
    var keyboard = new Keyboard(loop);
    var controls = new Controls(loop);
    var screen = new WindowScreen(loop);
    var index = MaxColorStep / 2;
    var lastCanvasSize = Vector2.Zero;
    var lastMousePosition = Vector2.Zero;
    var lastFullscreen = false;
    var lastVSync = true;
    var frameTime = 0d;

    BindDemoControls(controls);
    mouse.Track = true;
    keyboard.Clipboard = "AlvorKit.Windowing clipboard text";
    loop.Update += Update;
    loop.Frame += (dt) => frameTime = dt;
    loop.Render += Render;

    screen.Title = "AlvorKit.Windowing demo";
    screen.IsVSyncEnabled = true;
    screen.IsVisible = true;
    loop.Run();

    // Reads the facade objects so the demo shows the windowing layer rather than raw host calls.
    void Update(double dt)
    {
        if (keyboard.IsKeyPressed(WindowKey.Escape))
            screen.Close();

        if (canvas.Size != lastCanvasSize)
        {
            Console.WriteLine($"Canvas {canvas.Size}, monitor {screen.MonitorSize}, scale {screen.MonitorScale:0.##}");
            lastCanvasSize = canvas.Size;
        }

        if (mouse.Position != lastMousePosition)
        {
            Console.WriteLine($"Mouse {mouse.Position}, delta {mouse.Delta}, wheel {mouse.Wheel}");
            lastMousePosition = mouse.Position;
        }

        if (keyboard.Text.Count > 0)
        {
            foreach (var rune in keyboard.Text)
                Console.WriteLine($"Text '{rune}', clipboard '{keyboard.Clipboard}'");
        }

        if (screen.IsFullscreen != lastFullscreen || screen.IsVSyncEnabled != lastVSync)
        {
            Console.WriteLine($"Fullscreen {screen.IsFullscreen}, vsync {screen.IsVSyncEnabled}");
            lastFullscreen = screen.IsFullscreen;
            lastVSync = screen.IsVSyncEnabled;
        }

        if (controls["Previous"].Run() || mouse.IsMainPressed())
            index--;

        if (controls["Next"].Run() || mouse.IsSecondaryPressed())
            index++;

        if (controls["Randomize"].Run())
            index += random.Next(MaxColorStep + 1);
    }

    // Clears the OpenGL backbuffer using current input state to make the loop visibly alive.
    void Render()
    {
        var step = Math.Abs(index % MaxColorStep) / (float)MaxColorStep;
        var pulse = Math.Clamp((float)frameTime * 60f, 0f, 1f);
        gl.Viewport(0, 0, Math.Max(1, (int)canvas.Size.X), Math.Max(1, (int)canvas.Size.Y));
        gl.ClearColor(0.05f + pulse * 0.05f, step, 0.12f, 1f);
        gl.Clear(GlClearBufferMask.ColorBufferBit);
        gl.ResetClearColor();
        gl.ResetViewport();
    }
}

// Adds a few named controls so key, repeat, mouse, wheel, and modifier behavior are easy to try.
void BindDemoControls(Controls controls)
{
    var previous = controls["Previous"];
    previous.Bind(new() { KeyPress = WindowKey.Comma });
    previous.Bind(new() { KeyPressRepeat = WindowKey.Left });
    previous.Bind(new() { MouseScroll = MouseScrollDirection.Up });

    var next = controls["Next"];
    next.Bind(new() { KeyPress = WindowKey.Period });
    next.Bind(new() { KeyPressRepeat = WindowKey.Right });
    next.Bind(new() { MouseScroll = MouseScrollDirection.Down });

    var randomize = controls["Randomize"];
    randomize.Bind(new() { KeyPress = WindowKey.R });
    randomize.Bind(new()
    {
        KeyDown = WindowKey.E,
        Alt = KeyModifierState.Down,
        Control = KeyModifierState.Down,
        Shift = KeyModifierState.Up
    });
}
