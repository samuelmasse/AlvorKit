namespace AlvorKit.UI.Blend.Demo.NoiseLab;

/// <summary>
/// App-local color ramps mapping normalized noise to pixels. Ramp colors are Noise Lab data semantics,
/// not Blend chrome, mirroring how the Ranges visualizer keeps its allocator palette app-side.
/// </summary>
[App]
public class AppRamps
{
    private static readonly (float T, Vec4u8 Color)[][] Stops =
    [
        [(0f, Rgb(0x05, 0x05, 0x05)), (1f, Rgb(0xFA, 0xFA, 0xFA))],
        [
            (0f, Rgb(0x0E, 0x2E, 0x4A)), (0.32f, Rgb(0x1C, 0x5A, 0x7A)), (0.44f, Rgb(0x2E, 0x86, 0xA0)),
            (0.5f, Rgb(0xC9, 0xB2, 0x7C)), (0.58f, Rgb(0x6F, 0x94, 0x40)), (0.7f, Rgb(0x4E, 0x7A, 0x38)),
            (0.82f, Rgb(0x6E, 0x66, 0x59)), (0.9f, Rgb(0x8C, 0x85, 0x78)), (1f, Rgb(0xED, 0xED, 0xE8)),
        ],
        [
            (0f, Rgb(0x00, 0x00, 0x04)), (0.2f, Rgb(0x1D, 0x11, 0x47)), (0.4f, Rgb(0x51, 0x12, 0x7C)),
            (0.55f, Rgb(0x82, 0x26, 0x81)), (0.7f, Rgb(0xB6, 0x36, 0x79)), (0.82f, Rgb(0xE6, 0x51, 0x64)),
            (0.9f, Rgb(0xFB, 0x88, 0x61)), (0.96f, Rgb(0xFE, 0xC2, 0x87)), (1f, Rgb(0xFC, 0xFD, 0xBF)),
        ],
        [
            (0f, Rgb(0x44, 0x01, 0x54)), (0.25f, Rgb(0x41, 0x44, 0x87)), (0.5f, Rgb(0x2A, 0x78, 0x8E)),
            (0.7f, Rgb(0x22, 0xA8, 0x84)), (0.87f, Rgb(0x7A, 0xD1, 0x51)), (1f, Rgb(0xFD, 0xE7, 0x25)),
        ],
        [(0f, Rgb(0x18, 0x18, 0x18)), (1f, Rgb(0xB9, 0x82, 0x45))],
    ];

    /// <summary>Gets the ramp options for the Post section dropdown, with midpoint swatches.</summary>
    public IReadOnlyList<BlendDropdownItem> Items { get; } =
    [
        new("Grayscale", Swatch(0)),
        new("Terrain", Swatch(1)),
        new("Magma", Swatch(2)),
        new("Viridis", Swatch(3)),
        new("Two-tone", Swatch(4)),
    ];

    /// <summary>Maps a normalized sample through a ramp; the caller clamps <paramref name="t"/> to [0, 1].</summary>
    public Vec4u8 Color(int ramp, float t)
    {
        var stops = Stops[ramp];
        for (var i = 1; i < stops.Length; i++)
        {
            if (t > stops[i].T)
                continue;

            var (t0, c0) = stops[i - 1];
            var (t1, c1) = stops[i];
            var f = t1 > t0 ? (t - t0) / (t1 - t0) : 0f;
            return Lerp(c0, c1, f);
        }

        return stops[^1].Color;
    }

    private static Vec4u8 Lerp(Vec4u8 a, Vec4u8 b, float f) => (
        Byte(a.X + ((b.X - a.X) * f)),
        Byte(a.Y + ((b.Y - a.Y) * f)),
        Byte(a.Z + ((b.Z - a.Z) * f)),
        255);

    private static Vec4 MidColor(int ramp)
    {
        var stops = Stops[ramp];
        var mid = stops[stops.Length / 2].Color;
        return (mid.X / 255f, mid.Y / 255f, mid.Z / 255f, 1f);
    }

    private static Vec4 Swatch(int ramp) => MidColor(ramp);

    private static Vec4u8 Rgb(byte r, byte g, byte b) => (r, g, b, 255);

    private static byte Byte(float value) => (byte)Math.Clamp(MathF.Round(value), 0f, 255f);
}
