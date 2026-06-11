using AlvorKit.RGFW;

var window = Rgfw.CreateWindow("AlvorKit.Demo", 0, 0, 800, 450, RgfwWindowFlags.Center);
if (window == 0)
{
    Console.WriteLine("Failed to create window.");
    return 1;
}

Rgfw.WindowSetExitKey(window, RgfwKey.Escape);
Rgfw.WindowGetSize(window, out var w, out var h);
Rgfw.WindowGetPosition(window, out var x, out var y);
Console.WriteLine($"Window created: {w}x{h} at ({x}, {y}) — press Escape or close it to exit.");

while (!Rgfw.WindowShouldClose(window))
{
    Rgfw.WaitForEvent(Rgfw.EventWaitNext);
    while (Rgfw.WindowCheckEvent(window, out var ev))
    {
        switch (ev.Type)
        {
            case RgfwEventType.KeyPressed:
                Console.WriteLine($"Key pressed: {ev.Key} (sym '{(char)ev.KeySym}')");
                break;
            case RgfwEventType.MouseButtonPressed:
                Console.WriteLine($"Mouse button pressed: {ev.Button}");
                break;
            case RgfwEventType.WindowResized:
                Rgfw.WindowGetSize(window, out w, out h);
                Console.WriteLine($"Window resized: {w}x{h}");
                break;
            case RgfwEventType.Quit:
                Console.WriteLine("Window closed.");
                break;
        }
    }
}

Rgfw.WindowClose(window);
return 0;
