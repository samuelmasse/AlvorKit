using AlvorKit.FreeType;
using AlvorKit.GLFW;
using AlvorKit.MiniAudio;
using AlvorKit.OpenGL;
using AlvorKit.OpenGL.Layer;

namespace AlvorKit.Demo;

public static class DemoApp
{
    private const int InitialWidth = 800;
    private const int InitialHeight = 450;
    private const double FrameWaitSeconds = 0.01;
    private const int GlyphScale = 4;
    private const uint GlyphPixelHeight = 64;
    private const char DemoCharacter = 'a';

    public static int Run()
    {
        var glfw = new GlfwBackend();
        if (!glfw.Init())
        {
            Console.WriteLine("Failed to initialize GLFW.");
            return 1;
        }

        try
        {
            Ma ma = new MaBackend();
            Ft ft = new FtBackend();

            var fontPath = DemoFont.GetPath();
            var glyph = GlyphBitmap.Render(ft, fontPath, DemoCharacter, GlyphPixelHeight);
            ExportGlyphPreview(glyph);

            using var window = DemoWindow.TryCreate(glfw, "AlvorKit.Demo", InitialWidth, InitialHeight);
            if (window is null)
            {
                Console.WriteLine("Failed to create window.");
                return 1;
            }

            using var gl = new GlLayer(new GlBackend(glfw.GetProcAddress));
            glfw.GetFramebufferSize(window.Handle, out var width, out var height);
            Console.WriteLine($"Window created: {width}x{height} - press Escape or close it to exit.");

            var renderer = new GlyphRenderer(gl, glyph, GlyphScale);
            using var melody = new MelodyPlayer(ma);
            Console.WriteLine(melody.Playing ? "Playing Ode to Joy." : "Audio unavailable - running silent.");

            RunFrameLoop(glfw, window, renderer);
            Console.WriteLine($"GPU memory tracked: {gl.BufferUsage} buffer byte(s), {gl.TextureUsage} texture byte(s).");
            Console.WriteLine($"Closing - disposing {gl.Textures.Count} texture(s), {gl.Buffers.Count} buffer(s), " +
                $"{gl.VertexArrays.Count} VAO(s), {gl.Shaders.Count} shader(s), {gl.Programs.Count} program(s).");
            return 0;
        }
        finally
        {
            glfw.Terminate();
        }
    }

    private static void ExportGlyphPreview(GlyphBitmap glyph)
    {
        const string path = "out/a.png";
        glyph.ExportPng(path);
        Console.WriteLine($"Exported 'a' ({glyph.Width}x{glyph.Height}, gray) to {Path.GetFullPath(path)}");
    }

    private static void RunFrameLoop(Glfw glfw, DemoWindow window, GlyphRenderer renderer)
    {
        while (!glfw.WindowShouldClose(window.Handle))
        {
            glfw.WaitEventsTimeout(FrameWaitSeconds);
            if (glfw.GetKey(window.Handle, GlfwKey.Escape) == GlfwInputAction.Press)
                glfw.SetWindowShouldClose(window.Handle, true);

            glfw.GetFramebufferSize(window.Handle, out var width, out var height);
            renderer.Draw(width, height);
            glfw.SwapBuffers(window.Handle);
        }
    }
}
