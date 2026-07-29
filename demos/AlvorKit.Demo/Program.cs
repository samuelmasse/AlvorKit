const int InitialWidth = 800;
const int InitialHeight = 450;
const double FrameWaitSeconds = 0.01;
const int GlyphScale = 4;
const uint GlyphPixelHeight = 64;
const char DemoCharacter = 'a';
const string GlyphPreviewPath = "out/a.png";

using var logging = new LogRuntime();
logging.Start();
var log = logging.Log;

var glfw = new GlfwBackend();
if (!glfw.Init())
    throw new InvalidOperationException("Failed to initialize GLFW.");

glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 3);
glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 3);
glfw.WindowHint(GlfwWindowHint.OpenGLProfile, GlfwOpenGLProfile.CoreProfile);
glfw.WindowHint(GlfwWindowHint.OpenGLForwardCompat, true);

Ma audio = new MaBackend();
Ft freeType = new FtBackend();

var fontPath = ResolveDemoFontPath();

// FreeType runs first so the OpenGL half receives a plain managed bitmap instead of a live face.
var glyph = GlyphBitmap.Render(freeType, fontPath, DemoCharacter, GlyphPixelHeight);
ExportGlyphPreview(log, glyph, DemoCharacter, GlyphPreviewPath);

// The OpenGL layer needs a current GLFW context because it resolves function pointers from that context.
var window = glfw.CreateWindow(InitialWidth, InitialHeight, "AlvorKit.Demo", default, default);
if (window == default)
{
    glfw.Terminate();
    throw new InvalidOperationException("Failed to create the GLFW window.");
}

glfw.MakeContextCurrent(window);

var gl = new GlLayer(new GlBackend(glfw.GetProcAddress));
ReportWindowCreated(log, glfw, window);

var renderer = new GlyphRenderer(gl, glyph, GlyphScale);
var melody = new MelodyPlayer(audio);
log.Info("Playing Ode to Joy.");

// The frame loop keeps the demo responsive while the melody thread advances miniaudio's waveform frequency.
RunFrameLoop(glfw, window, renderer, FrameWaitSeconds);
ReportTrackedResources(log, gl);

// Cleanup is deliberately direct here so the native lifetime order stays visible in the demo path.
melody.Dispose();
gl.Dispose();
glfw.DestroyWindow(window);
glfw.Terminate();
return 0;

// Resolves the checked-in Inter font used by the FreeType step.
static string ResolveDemoFontPath()
{
    var fontPath = Path.Combine(ProjectRoot.ResDirectory(typeof(GlyphBitmap)), "fonts", "Inter.ttf");
    if (!File.Exists(fontPath))
        throw new FileNotFoundException("Required demo font is missing.", fontPath);

    return fontPath;
}

// Writes the rasterized glyph as a PNG so the FreeType output can be inspected without the window.
static void ExportGlyphPreview(Log log, GlyphBitmap glyph, char character, string path)
{
    glyph.ExportPng(path);
    log.Info($"Exported '{character}' ({glyph.Width}x{glyph.Height}, gray) to {Path.GetFullPath(path)}");
}

// Reports the framebuffer size GLFW created for the demo window.
static void ReportWindowCreated(Log log, Glfw glfw, GlfwWindow window)
{
    glfw.GetFramebufferSize(window, out var width, out var height);
    log.Info("Window created: {0}x{1} - press Escape or close it to exit.", width, height);
}

// Waits for input, closes on Escape, and renders the glyph at the current framebuffer size.
static void RunFrameLoop(Glfw glfw, GlfwWindow window, GlyphRenderer renderer, double frameWaitSeconds)
{
    while (!glfw.WindowShouldClose(window))
    {
        glfw.WaitEventsTimeout(frameWaitSeconds);
        if (glfw.GetKey(window, GlfwKey.Escape) == GlfwInputAction.Press)
            glfw.SetWindowShouldClose(window, true);

        glfw.GetFramebufferSize(window, out var width, out var height);
        renderer.Draw(width, height);
        glfw.SwapBuffers(window);
    }
}

// Prints the layer's tracked GPU resources before the layer disposes them.
static void ReportTrackedResources(Log log, GlLayer gl)
{
    log.Info("GPU memory tracked: {0} buffer byte(s), {1} texture byte(s).", gl.BufferUsage, gl.TextureUsage);
    log.Info($"Closing - disposing {gl.Textures.Count} texture(s), {gl.Buffers.Count} buffer(s), " +
        $"{gl.VertexArrays.Count} VAO(s), {gl.Shaders.Count} shader(s), {gl.Programs.Count} program(s).");
}
