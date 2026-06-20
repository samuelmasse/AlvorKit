const string WindowTitle = "AlvorKit azure tentacle GLB";

var fontPath = Path.Combine(ProjectRoot.ResDirectory(typeof(GlbModel)), "fonts", "RobotoMono-Regular.ttf");
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

var rawGl = new GlBackend(glfw.GetProcAddress);
var gl = new GlLayer(rawGl);
var sprites = new SpriteBatch(gl);
var fontContext = new FontContext(gl, sprites);
var font = new Font(fontContext, fontPath);
var fpsSize = font.Size(30);
var overlaySize = font.Size(18);

gl.GetString(GlStringName.Version, out var version);
gl.GetString(GlStringName.ShadingLanguageVersion, out var glsl);
Console.WriteLine($"OpenGL {version} (GLSL {glsl}) - close the window to exit.");
Console.WriteLine("Mouse look, WASD to move, Space up, Control down, Shift faster, Escape releases the cursor.");

var model = GlbModel.Load(gl);
var modelInfoLines = CreateModelInfoLines();
var animationLines = CreateAnimationLines();
var camera = new FlyCamera(new Vector3(0f, 0.08f, 4f), 0f, 0f);
var clock = Stopwatch.StartNew();
var previousSeconds = 0.0;
var rawMouseSupported = glfw.RawMouseMotionSupported();
var mouseTracking = false;
var escapeWasDown = false;
var leftMouseWasDown = false;
var previousAnimationWasDown = false;
var nextAnimationWasDown = false;
var frameCount = 0;
var elapsed = 0.0;
var fpsText = string.Empty;
var frameClock = Stopwatch.StartNew();

EnableMouseTracking();
gl.Enable(GlEnableCap.DepthTest);
gl.Enable(GlEnableCap.CullFace);

// Draws the animated GLB and the FPS overlay with transient OpenGL layer state reset before returning to the event loop.
void RenderFrame(int width, int height)
{
    if (width <= 0 || height <= 0)
        return;

    UpdateFps();
    PrepareOverlayGlyphs();

    gl.Viewport(0, 0, width, height);
    gl.ClearColor(0, 0, 0, 1f);
    gl.ClearDepth(1.0);
    gl.Clear(GlClearBufferMask.ColorBufferBit | GlClearBufferMask.DepthBufferBit);

    Span<float> view = stackalloc float[16];
    camera.WriteViewMatrix(view);
    model.Render(width, height, view);

    DrawOverlay(width, height);

    gl.ResetClearDepth();
    gl.ResetClearColor();
    gl.ResetViewport();
    glfw.SwapBuffers(window);
}

// The callback repaints during platform resize loops, where the normal poll loop may be paused.
glfw.SetFramebufferSizeCallback(window, (_, width, height) => RenderFrame(width, height));

while (!glfw.WindowShouldClose(window))
{
    glfw.PollEvents();
    UpdateMouseTracking();
    UpdateAnimationSelection();

    var currentSeconds = clock.Elapsed.TotalSeconds;
    var elapsedSeconds = (float)(currentSeconds - previousSeconds);
    model.Update(elapsedSeconds);
    camera.Update(glfw, window, elapsedSeconds, mouseTracking);
    previousSeconds = currentSeconds;

    glfw.GetFramebufferSize(window, out var width, out var height);
    RenderFrame(width, height);
}

gl.Disable(GlEnableCap.CullFace);
gl.Disable(GlEnableCap.DepthTest);
font.Dispose();
fontContext.Dispose();
sprites.Dispose();
model.Dispose();
gl.Dispose();
glfw.DestroyWindow(window);
glfw.Terminate();
return 0;

// Updates the FPS text once each accumulated second, using the same frame-count display as the fonts demo.
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

// Creates and packs glyphs for the current overlay strings before the 3D pass claims viewport state.
void PrepareOverlayGlyphs()
{
    if (fpsText.Length > 0)
        _ = sprites.Writer.Measure(fpsSize, fpsText);

    for (var index = 0; index < modelInfoLines.Length; index++)
        _ = sprites.Writer.Measure(overlaySize, modelInfoLines[index]);

    for (var index = 0; index < animationLines.Length; index++)
        _ = sprites.Writer.Measure(overlaySize, animationLines[index]);

    gl.Disable(GlEnableCap.CullFace);
    gl.Disable(GlEnableCap.DepthTest);
    font.Pack();
    gl.Enable(GlEnableCap.DepthTest);
    gl.Enable(GlEnableCap.CullFace);
}

// Draws the model stats, animation list, selected clip highlight, and FPS through the shared sprite batch.
void DrawOverlay(int framebufferWidth, int framebufferHeight)
{
    var clientSize = new Vector2(framebufferWidth, framebufferHeight);

    gl.Disable(GlEnableCap.CullFace);
    gl.Disable(GlEnableCap.DepthTest);
    gl.Enable(GlEnableCap.Blend);
    gl.BlendFunc(GlBlendingFactor.SrcAlpha, GlBlendingFactor.OneMinusSrcAlpha);

    sprites.Begin(clientSize);
    DrawModelInfo();
    DrawFps(clientSize);
    sprites.End();

    gl.ResetBlendFunc();
    gl.Disable(GlEnableCap.Blend);
    gl.Enable(GlEnableCap.DepthTest);
    gl.Enable(GlEnableCap.CullFace);
}

// Draws the model statistics and the animation list on the left side of the window.
void DrawModelInfo()
{
    var lineHeight = overlaySize.Metrics.Height;
    var position = new Vector2(10f, 8f + overlaySize.Metrics.Ascender + overlaySize.Metrics.Descender);
    var infoColor = new Vector4(0.78f, 0.86f, 0.95f, 1f);
    var headerColor = new Vector4(0.45f, 0.95f, 1f, 1f);
    var animationColor = new Vector4(0.62f, 0.68f, 0.76f, 1f);
    var selectedAnimationColor = new Vector4(1f, 0.86f, 0.18f, 1f);

    for (var index = 0; index < modelInfoLines.Length; index++)
    {
        sprites.Writer.Write(overlaySize, modelInfoLines[index], position, infoColor);
        position.Y += lineHeight;
    }

    position.Y += lineHeight * 0.5f;
    sprites.Writer.Write(overlaySize, "Animations", position, headerColor);
    position.Y += lineHeight;

    for (var index = 0; index < animationLines.Length; index++)
    {
        var color = index == model.SelectedAnimationIndex ? selectedAnimationColor : animationColor;
        sprites.Writer.Write(overlaySize, animationLines[index], position, color);
        position.Y += lineHeight;
    }
}

// Draws the FPS label in the top-right corner using the same font and sprite batching path as the fonts demo.
void DrawFps(Vector2 clientSize)
{
    if (fpsText.Length == 0)
        return;

    var fpsWidth = sprites.Writer.Measure(fpsSize, fpsText);
    var fpsPosition = new Vector2(
        clientSize.X - fpsWidth - 10f,
        fpsSize.Metrics.Ascender + fpsSize.Metrics.Descender);
    sprites.Writer.Write(fpsSize, fpsText, fpsPosition, new Vector4(0f, 1f, 0f, 1f));
}

// Reads edge-triggered animation cycling controls from the arrow keys.
void UpdateAnimationSelection()
{
    var previousDown =
        glfw.GetKey(window, GlfwKey.Left) == GlfwInputAction.Press ||
        glfw.GetKey(window, GlfwKey.Up) == GlfwInputAction.Press;
    var nextDown =
        glfw.GetKey(window, GlfwKey.Right) == GlfwInputAction.Press ||
        glfw.GetKey(window, GlfwKey.Down) == GlfwInputAction.Press;

    if (previousDown && !previousAnimationWasDown && !nextDown)
        model.SelectPreviousAnimation();
    else if (nextDown && !nextAnimationWasDown && !previousDown)
        model.SelectNextAnimation();

    previousAnimationWasDown = previousDown;
    nextAnimationWasDown = nextDown;
}

// Reads edge-triggered mouse tracking controls: Escape releases the cursor, and a left click recaptures it.
void UpdateMouseTracking()
{
    var escapeDown = glfw.GetKey(window, GlfwKey.Escape) == GlfwInputAction.Press;
    if (mouseTracking && escapeDown && !escapeWasDown)
        DisableMouseTracking();

    escapeWasDown = escapeDown;

    var leftMouseDown = glfw.GetMouseButton(window, 0) == GlfwInputAction.Press;
    if (!mouseTracking && leftMouseDown && !leftMouseWasDown)
        EnableMouseTracking();

    leftMouseWasDown = leftMouseDown;
}

// Captures the cursor and enables mouse-look deltas for the fly camera.
void EnableMouseTracking()
{
    glfw.SetInputMode(window, GlfwInputMode.Cursor, GlfwCursorMode.Disabled);
    if (rawMouseSupported)
        glfw.SetInputMode(window, GlfwInputMode.RawMouseMotion, true);

    camera.ResetMouseTracking();
    mouseTracking = true;
}

// Releases the cursor and pauses mouse-look plus keyboard camera movement.
void DisableMouseTracking()
{
    if (rawMouseSupported)
        glfw.SetInputMode(window, GlfwInputMode.RawMouseMotion, false);

    glfw.SetInputMode(window, GlfwInputMode.Cursor, GlfwCursorMode.Normal);
    camera.ResetMouseTracking();
    mouseTracking = false;
}

// Builds the fixed model-stat overlay text once after loading the GLB.
string[] CreateModelInfoLines() =>
[
    "Model: azure_tentacle_monster_tex256.glb",
    string.Format(CultureInfo.InvariantCulture, "Vertices: {0}", model.VertexCount),
    string.Format(CultureInfo.InvariantCulture, "Triangles: {0}", model.TriangleCount),
    string.Format(CultureInfo.InvariantCulture, "Texture: {0} x {1} nearest", model.TextureWidth, model.TextureHeight),
    string.Format(CultureInfo.InvariantCulture, "Joints: {0}", model.JointCount),
    string.Format(CultureInfo.InvariantCulture, "Animation slots: {0}", model.AnimationCount),
];

// Builds the fixed animation-list overlay text once after loading the GLB.
string[] CreateAnimationLines()
{
    var lines = new string[model.AnimationCount];
    for (var index = 0; index < lines.Length; index++)
    {
        var duration = model.GetAnimationDuration(index);
        lines[index] = duration <= 0f
            ? string.Format(CultureInfo.InvariantCulture, "{0}. {1} (no animation)", index + 1, model.GetAnimationName(index))
            : string.Format(CultureInfo.InvariantCulture, "{0}. {1} ({2:0.###}s)", index + 1, model.GetAnimationName(index), duration);
    }

    return lines;
}
