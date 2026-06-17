const string WindowTitle = "AlvorKit.Graphics2D.Fonts.Demo";
const int AtlasPreviewSize = 256;
const int KeyStateCount = 512;

var fontPath = Path.Combine(ProjectRoot.ResDirectory(typeof(FontDemoMarker)), "fonts", "RobotoMono-Regular.ttf");
if (!File.Exists(fontPath))
    throw new FileNotFoundException("Required demo font is missing.", fontPath);

var glfw = new GlfwBackend();
if (!glfw.Init())
    throw new InvalidOperationException("Failed to initialize GLFW.");

glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 3);
glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 3);
glfw.WindowHint(GlfwWindowHint.OpenGLProfile, GlfwOpenGLProfile.CoreProfile);

var primaryMonitor = glfw.GetPrimaryMonitor();
glfw.GetMonitorWorkarea(primaryMonitor, out _, out _, out var monitorWidth, out var monitorHeight);
var window = glfw.CreateWindow(monitorWidth * 3 / 4, monitorHeight * 3 / 4, WindowTitle, default, default);
if (window == default)
{
    glfw.Terminate();
    throw new InvalidOperationException("Failed to create the GLFW window.");
}

glfw.MakeContextCurrent(window);
glfw.SwapInterval(0);

var gl = new GlLayer(new GlBackend(glfw.GetProcAddress));
var sprites = new SpriteBatch(gl);
var context = new FontContext(gl, sprites);
var font = new Font(context, fontPath);

var freeText = new List<string> { string.Empty };
var fillSize = 8;
var frameCount = 0;
var elapsed = 0.0;
var fpsText = string.Empty;
var frameClock = Stopwatch.StartNew();
var keyWasDown = new bool[KeyStateCount];

// Draws one frame with the same layout, colors, and atlas preview as the original font demo.
void RenderFrame(int framebufferWidth, int framebufferHeight)
{
    if (framebufferWidth <= 0 || framebufferHeight <= 0)
        return;

    UpdateFps();
    Input();

    var clientSize = new Vector2(framebufferWidth, framebufferHeight);
    gl.Viewport(0, 0, framebufferWidth, framebufferHeight);
    gl.ClearColor(0.2f, 0.05f, 0.3f, 0f);
    gl.Clear(GlClearBufferMask.ColorBufferBit);
    gl.ResetClearColor();

    gl.Enable(GlEnableCap.Blend);
    gl.BlendFunc(GlBlendingFactor.SrcAlpha, GlBlendingFactor.OneMinusSrcAlpha);

    sprites.Begin(clientSize);
    Draw(clientSize);
    sprites.End();

    gl.Disable(GlEnableCap.Blend);
    gl.ResetBlendFunc();
    gl.ResetViewport();
    glfw.SwapBuffers(window);
}

// Draws title text, FPS text, typed red text, and the raw atlas texture strip.
void Draw(Vector2 clientSize)
{
    var titleSize = font.Size(60);
    var titleMetrics = titleSize.Metrics;
    sprites.Writer.Write(titleSize, WindowTitle, new Vector2(0f, titleMetrics.Ascender + titleMetrics.Descender), new Vector4(1f, 1f, 0f, 0.5f));

    var fpsSize = font.Size(30);
    var fpsWidth = sprites.Writer.Measure(fpsSize, fpsText);
    var fpsPosition = new Vector2(clientSize.X - fpsWidth - 10f, fpsSize.Metrics.Ascender + fpsSize.Metrics.Descender);
    sprites.Writer.Write(fpsSize, fpsText, fpsPosition, new Vector4(0f, 1f, 0f, 1f));

    var freeTextFontSize = font.Size(120);
    var freeTextStart = new Vector2(20f, 200f);

    for (var i = 0; i < freeText.Count; i++)
    {
        var position = freeTextStart + i * new Vector2(0f, freeTextFontSize.Metrics.Height);
        sprites.Writer.Write(freeTextFontSize, freeText[i], position, new Vector4(1f, 0f, 0f, 1f));
    }

    sprites.Writer.Draw(
        new Vector2(0f, clientSize.Y - AtlasPreviewSize),
        new Vector2(clientSize.X, AtlasPreviewSize),
        new Vector4(0f, 0f, 0f, 1f));

    var textures = font.Textures;
    for (var i = 0; i < textures.Length; i++)
    {
        sprites.Writer.Draw(
            textures[i],
            new Vector2(i * AtlasPreviewSize, clientSize.Y - AtlasPreviewSize),
            new Vector2(AtlasPreviewSize, AtlasPreviewSize));
    }
}

// Handles per-frame packing plus edge-triggered F1/F2 atlas controls.
void Input()
{
    font.Pack();

    if (KeyPressed(GlfwKey.F1))
    {
        Console.WriteLine(fillSize);

        var size = font.Size(fillSize);
        for (var i = 0; i < 512; i++)
            _ = size.GlyphSlot(new Rune(i));

        fillSize *= 2;
    }

    if (KeyPressed(GlfwKey.F2))
        font.ForcePack();
}

// Appends one text-input scalar, matching the original append-only behavior.
void AcceptCharacter(uint codepoint)
{
    if (codepoint <= int.MaxValue && Rune.IsValid((int)codepoint))
        freeText[^1] += char.ConvertFromUtf32((int)codepoint);
}

// Handles key press and repeat events for Backspace and Enter.
void AcceptKey(GlfwKey key, GlfwInputAction action)
{
    if (action is not (GlfwInputAction.Press or GlfwInputAction.Repeat))
        return;

    if (key == GlfwKey.Backspace && freeText[^1].Length > 0)
    {
        freeText[^1] = freeText[^1][..^1];
        if (freeText[^1].Length == 0 && freeText.Count > 1)
            freeText.RemoveAt(freeText.Count - 1);
    }

    if (key == GlfwKey.Enter)
        freeText.Add(string.Empty);
}

// Returns true once when a key transitions from released to pressed.
bool KeyPressed(GlfwKey key)
{
    var index = (int)key;
    var down = glfw.GetKey(window, key) == GlfwInputAction.Press;
    if ((uint)index >= keyWasDown.Length)
        return down;

    var pressed = down && !keyWasDown[index];
    keyWasDown[index] = down;
    return pressed;
}

// Updates the FPS text once each accumulated second, using the old demo's frame-count display.
void UpdateFps()
{
    frameCount++;
    elapsed += frameClock.Elapsed.TotalSeconds;
    frameClock.Restart();

    if (elapsed < 1.0)
        return;

    fpsText = $"FPS: {frameCount}";
    frameCount = 0;
    elapsed = 0.0;
}

glfw.SetCharCallback(window, (_, codepoint) => AcceptCharacter(codepoint));
glfw.SetKeyCallback(window, (_, key, _, action, _) => AcceptKey(key, action));
glfw.SetFramebufferSizeCallback(window, (_, width, height) => RenderFrame(width, height));

while (!glfw.WindowShouldClose(window))
{
    glfw.PollEvents();
    glfw.GetFramebufferSize(window, out var width, out var height);
    RenderFrame(width, height);
}

font.Dispose();
context.Dispose();
sprites.Dispose();
gl.Dispose();
glfw.DestroyWindow(window);
glfw.Terminate();
return 0;

/// <summary>Marker type used to resolve repository resources for the Fonts demo.</summary>
internal sealed class FontDemoMarker;
