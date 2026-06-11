using AlvorKit.Demo;
using AlvorKit.OpenGL;
using AlvorKit.RGFW;

var glyph = Glyph.Render(await DemoFont.DownloadAsync(), 'a', 64);
glyph.ExportPng("out/a.png");
Console.WriteLine($"Exported 'a' ({glyph.Width}x{glyph.Height}, gray) to {Path.GetFullPath("out/a.png")}");

var window = Rgfw.CreateWindow("AlvorKit.Demo", 0, 0, 800, 450, RgfwWindowFlags.Center | RgfwWindowFlags.OpenGL);
if (window == 0)
{
    Console.WriteLine("Failed to create window.");
    return 1;
}
Rgfw.WindowSetExitKey(window, RgfwKey.Escape);
Rgfw.WindowMakeCurrentContextOpenGL(window);
Gl.Load(Rgfw.GetProcAddressOpenGL);
Rgfw.WindowGetSize(window, out var width, out var height);
Console.WriteLine($"Window created: {width}x{height} — press Escape or close it to exit.");

var renderer = new GlyphRenderer(glyph, width, height, scale: 4);
using var melody = new MelodyPlayer();
Console.WriteLine(melody.Playing ? "Playing Ode to Joy." : "Audio unavailable — running silent.");

while (!Rgfw.WindowShouldClose(window))
{
    Rgfw.WaitForEvent(10);
    while (Rgfw.WindowCheckEvent(window, out var ev))
    {
        if (ev.Type == RgfwEventType.WindowResized)
        {
            Rgfw.WindowGetSize(window, out width, out height);
            renderer.Resize(width, height);
        }
        else if (ev.Type == RgfwEventType.Quit)
            Console.WriteLine("Window closed.");
    }

    renderer.Draw();
    Rgfw.WindowSwapBuffersOpenGL(window);
}

Rgfw.WindowClose(window);
return 0;
