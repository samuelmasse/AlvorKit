using AlvorKit.Demo;
using AlvorKit.FreeType;
using AlvorKit.GLFW;
using AlvorKit.MiniAudio;
using AlvorKit.OpenGL;
using AlvorKit.OpenGL.Layer;

const int initialWidth = 800;
const int initialHeight = 450;
const double frameWaitSeconds = 0.01;
const int glyphScale = 4;
const uint glyphPixelHeight = 64;
const char demoCharacter = 'a';

var glfw = new GlfwBackend();
glfw.Init();

Ma audio = new MaBackend();
Ft freeType = new FtBackend();

var fontPath = Path.GetFullPath(Path.Combine("res", "fonts", "Inter.ttf"));
if (!File.Exists(fontPath))
    throw new FileNotFoundException("Required demo font is missing.", fontPath);

var glyph = GlyphBitmap.Render(freeType, fontPath, demoCharacter, glyphPixelHeight);
ExportGlyphPreview(glyph);

var window = glfw.CreateWindow(initialWidth, initialHeight, "AlvorKit.Demo", default, default);
glfw.MakeContextCurrent(window);

var gl = new GlLayer(new GlBackend(glfw.GetProcAddress));
ReportWindowCreated(glfw, window);

var renderer = new GlyphRenderer(gl, glyph, glyphScale);
var melody = new MelodyPlayer(audio);
Console.WriteLine("Playing Ode to Joy.");

RunFrameLoop(glfw, window, renderer, frameWaitSeconds);
ReportTrackedResources(gl);

melody.Dispose();
gl.Dispose();
glfw.DestroyWindow(window);
glfw.Terminate();
return 0;

static void ExportGlyphPreview(GlyphBitmap glyph)
{
    const string path = "out/a.png";
    glyph.ExportPng(path);
    Console.WriteLine($"Exported 'a' ({glyph.Width}x{glyph.Height}, gray) to {Path.GetFullPath(path)}");
}

static void ReportWindowCreated(Glfw glfw, GlfwWindow window)
{
    glfw.GetFramebufferSize(window, out var width, out var height);
    Console.WriteLine($"Window created: {width}x{height} - press Escape or close it to exit.");
}

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

static void ReportTrackedResources(GlLayer gl)
{
    Console.WriteLine($"GPU memory tracked: {gl.BufferUsage} buffer byte(s), {gl.TextureUsage} texture byte(s).");
    Console.WriteLine($"Closing - disposing {gl.Textures.Count} texture(s), {gl.Buffers.Count} buffer(s), " +
        $"{gl.VertexArrays.Count} VAO(s), {gl.Shaders.Count} shader(s), {gl.Programs.Count} program(s).");
}
