using AlvorKit.FreeType;
using AlvorKit.MiniAudio;
using AlvorKit.OpenGL;
using AlvorKit.RGFW;

namespace AlvorKit.Demo;

public static class DemoApp
{
    private const int InitialWidth = 800;
    private const int InitialHeight = 450;
    private const int FrameWaitMs = 10;
    private const int GlyphScale = 4;
    private const uint GlyphPixelHeight = 64;
    private const char DemoCharacter = 'a';

    public static int Run()
    {
        Rgfw rgfw = new RgfwBackend();
        Ma ma = new MaBackend();
        Ft ft = new FtBackend();

        var fontPath = DemoFont.GetPath();
        var glyph = GlyphBitmap.Render(ft, fontPath, DemoCharacter, GlyphPixelHeight);
        ExportGlyphPreview(glyph);

        using var window = DemoWindow.TryCreate(rgfw, "AlvorKit.Demo", InitialWidth, InitialHeight);
        if (window is null)
        {
            Console.WriteLine("Failed to create window.");
            return 1;
        }

        var gl = new GlBackend(rgfw.GetProcAddressOpenGL);
        window.GetSize(out var width, out var height);
        Console.WriteLine($"Window created: {width}x{height} - press Escape or close it to exit.");

        var renderer = new GlyphRenderer(gl, glyph, width, height, GlyphScale);
        using var melody = new MelodyPlayer(ma);
        Console.WriteLine(melody.Playing ? "Playing Ode to Joy." : "Audio unavailable - running silent.");

        RunFrameLoop(rgfw, window, renderer);
        return 0;
    }

    private static void ExportGlyphPreview(GlyphBitmap glyph)
    {
        const string path = "out/a.png";
        glyph.ExportPng(path);
        Console.WriteLine($"Exported 'a' ({glyph.Width}x{glyph.Height}, gray) to {Path.GetFullPath(path)}");
    }

    private static void RunFrameLoop(Rgfw rgfw, DemoWindow window, GlyphRenderer renderer)
    {
        while (!rgfw.WindowShouldClose(window.Handle))
        {
            rgfw.WaitForEvent(FrameWaitMs);
            DrainEvents(rgfw, window, renderer);

            renderer.Draw();
            rgfw.WindowSwapBuffersOpenGL(window.Handle);
        }
    }

    private static void DrainEvents(Rgfw rgfw, DemoWindow window, GlyphRenderer renderer)
    {
        while (rgfw.WindowCheckEvent(window.Handle, out var ev))
            HandleEvent(window, renderer, in ev);
    }

    private static void HandleEvent(DemoWindow window, GlyphRenderer renderer, in RgfwEvent ev)
    {
        switch (ev.Type)
        {
            case RgfwEventType.WindowResized:
                window.GetSize(out var width, out var height);
                renderer.Resize(width, height);
                break;

            case RgfwEventType.Quit:
                Console.WriteLine("Window closed.");
                break;
        }
    }
}
