using AlvorKit.Demo;
using AlvorKit.FreeType;
using AlvorKit.MiniAudio;
using AlvorKit.OpenGL;
using AlvorKit.RGFW;

Rgfw rgfw = new RgfwBackend();
Ma ma = new MaBackend();
Ft ft = new FtBackend();

var glyph = Glyph.Render(ft, await DemoFont.DownloadAsync(), 'a', 64);
glyph.ExportPng("out/a.png");
Console.WriteLine($"Exported 'a' ({glyph.Width}x{glyph.Height}, gray) to {Path.GetFullPath("out/a.png")}");

var window = rgfw.CreateWindow("AlvorKit.Demo", 0, 0, 800, 450, RgfwWindowFlags.Center | RgfwWindowFlags.OpenGL);
if (window == 0)
{
    Console.WriteLine("Failed to create window.");
    return 1;
}
rgfw.WindowSetExitKey(window, RgfwKey.Escape);
rgfw.WindowMakeCurrentContextOpenGL(window);
Gl.Load(rgfw.GetProcAddressOpenGL);
rgfw.WindowGetSize(window, out var width, out var height);
Console.WriteLine($"Window created: {width}x{height} — press Escape or close it to exit.");

var renderer = new GlyphRenderer(glyph, width, height, scale: 4);
using var melody = new MelodyPlayer(ma);
Console.WriteLine(melody.Playing ? "Playing Ode to Joy." : "Audio unavailable — running silent.");

while (!rgfw.WindowShouldClose(window))
{
    rgfw.WaitForEvent(10);
    while (rgfw.WindowCheckEvent(window, out var ev))
    {
        if (ev.Type == RgfwEventType.WindowResized)
        {
            rgfw.WindowGetSize(window, out width, out height);
            renderer.Resize(width, height);
        }
        else if (ev.Type == RgfwEventType.Quit)
            Console.WriteLine("Window closed.");
    }

    renderer.Draw();
    rgfw.WindowSwapBuffersOpenGL(window);
}

rgfw.WindowClose(window);
return 0;
