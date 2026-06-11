using AlvorKit.FreeType;
using AlvorKit.MiniAudio;
using AlvorKit.RGFW;
using BigGustave;

// FreeType: render the letter 'a' from a downloaded font and export it as a PNG.
var fontPath = Path.Combine(Path.GetTempPath(), "Inter.ttf");
if (!File.Exists(fontPath))
    File.WriteAllBytes(fontPath, await new HttpClient().GetByteArrayAsync(
        "https://github.com/google/fonts/raw/main/ofl/inter/Inter%5Bopsz,wght%5D.ttf"));

if (Ft.InitFreeType(out var freetype) != 0)
{
    Console.WriteLine("Failed to initialize FreeType.");
    return 1;
}
if (Ft.NewFace(freetype, fontPath, new(0), out var face) != 0
    || Ft.SetPixelSizes(face, 0, 64) != 0
    || Ft.LoadChar(face, new('a'), Ft.LoadRender) != 0)
{
    Console.WriteLine("Failed to render the glyph.");
    return 1;
}

var slot = Marshal.PtrToStructure<FtGlyphSlotRec>(Marshal.PtrToStructure<FtFaceRec>(face).Glyph);
var bitmap = slot.Bitmap;
var png = PngBuilder.Create((int)bitmap.Width, (int)bitmap.Rows, false);
var row = new byte[bitmap.Width];
for (var y = 0; y < bitmap.Rows; y++)
{
    Marshal.Copy(bitmap.Buffer + y * bitmap.Pitch, row, 0, row.Length);
    for (var x = 0; x < row.Length; x++)
        png.SetPixel(new Pixel(row[x], row[x], row[x]), x, y);
}
var pngPath = Path.GetFullPath("out/a.png");
Directory.CreateDirectory("out");
using (var stream = File.Create(pngPath))
    png.Save(stream);
Console.WriteLine($"Exported 'a' ({bitmap.Width}x{bitmap.Rows}, gray) to {pngPath}");
Ft.DoneFace(face);
Ft.DoneFreeType(freetype);

var window = Rgfw.CreateWindow("AlvorKit.Demo", 0, 0, 800, 450, RgfwWindowFlags.Center);
if (window == 0)
{
    Console.WriteLine("Failed to create window.");
    return 1;
}

Rgfw.WindowSetExitKey(window, RgfwKey.Escape);
Rgfw.WindowGetSize(window, out var w, out var h);
Console.WriteLine($"Window created: {w}x{h} — press Escape or close it to exit.");

// Audio: a sine waveform routed through the engine, retuned per note.
var engine = Marshal.AllocHGlobal((int)Ma.SizeofEngine());
var waveform = Marshal.AllocHGlobal((int)Ma.SizeofWaveform());
var sound = Marshal.AllocHGlobal((int)Ma.SizeofSound());
var playing = Ma.EngineInit(0, engine) == MaResult.Success;
if (playing)
{
    var config = Ma.WaveformConfigInit(MaFormat.F32, 2, 48000, MaWaveformType.Sine, 0.2, 330);
    playing = Ma.WaveformInit(in config, waveform) == MaResult.Success
        && Ma.SoundInitFromDataSource(engine, waveform, 0, 0, sound) == MaResult.Success
        && Ma.SoundStart(sound) == MaResult.Success;
}
Console.WriteLine(playing ? "Playing Ode to Joy." : "Audio unavailable — running silent.");

var melody = playing ? new Thread(PlayMelody) { IsBackground = true } : null;
melody?.Start();

while (!Rgfw.WindowShouldClose(window))
{
    Rgfw.WaitForEvent(Rgfw.EventWaitNext);
    while (Rgfw.WindowCheckEvent(window, out var ev))
    {
        if (ev.Type == RgfwEventType.Quit)
            Console.WriteLine("Window closed.");
    }
}

if (playing)
{
    Ma.SoundStop(sound);
    Ma.SoundUninit(sound);
    Ma.WaveformUninit(waveform);
    Ma.EngineUninit(engine);
}
Marshal.FreeHGlobal(sound);
Marshal.FreeHGlobal(waveform);
Marshal.FreeHGlobal(engine);
Rgfw.WindowClose(window);
return 0;

void PlayMelody()
{
    double[] notes = [330, 330, 349, 392, 392, 349, 330, 294, 262, 262, 294, 330, 330, 294, 294];
    for (var i = 0; ; i = (i + 1) % notes.Length)
    {
        Ma.WaveformSetFrequency(waveform, notes[i]);
        Thread.Sleep(i == notes.Length - 1 ? 700 : 350);
    }
}
