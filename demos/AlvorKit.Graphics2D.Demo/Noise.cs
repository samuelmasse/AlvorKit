namespace AlvorKit.Graphics2D.Demo;

/// <summary>Creates deterministic RGBA noise textures used by the Graphics2D sprite-batch demo.</summary>
public static class Noise
{
    /// <summary>The seeded random source that keeps the generated demo textures stable across runs.</summary>
    private static readonly Random Random = new(2353);

    /// <summary>Generates a square RGBA texture with coloured orientation markers and random interior pixels.</summary>
    public static Vec4u8[] Generate(int size)
    {
        var pixels = new Vec4u8[size * size];

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                Vec4u8 rgba;

                if (x < size / 4 && size - y < size / 4)
                    rgba = (0, byte.MaxValue, 0, byte.MaxValue);
                else if (size - x < size / 4 && y < size / 4)
                    rgba = (byte.MaxValue, 0, 0, byte.MaxValue);
                else
                    rgba = ((byte)Random.Next(256), (byte)Random.Next(256), (byte)Random.Next(256), byte.MaxValue);

                pixels[(y * size) + x] = rgba;
            }
        }

        return pixels;
    }
}
