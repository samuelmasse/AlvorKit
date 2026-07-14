namespace AlvorKit.UI.Blend;

/// <summary>Generates reusable rounded control edge textures for the Blend style.</summary>
public class BlendControlChrome(GlLayer gl, RootUiScale scale)
{
    private const int CoverageSamples = 64;
    private readonly Dictionary<BlendControlCapKey, Texture2D> caps = [];

    /// <summary>Converts a physical pixel count to logical UI units for the active UI scale.</summary>
    public float UiPixels(int pixels) => pixels / scale.Scale;

    /// <summary>Converts logical UI units to physical pixels for generated textures.</summary>
    public int PhysicalPixels(float value) => Math.Max(1, (int)MathF.Round(value * scale.Scale));

    /// <summary>Gets a generated left cap texture for a rounded control surface.</summary>
    public Texture2D Cap(
        float height,
        float radius,
        float borderWidth,
        Vec4 fill,
        Vec4 border)
    {
        var key = new BlendControlCapKey(
            PhysicalPixels(height),
            PhysicalPixels(radius),
            PhysicalPixels(borderWidth),
            Pack(fill),
            Pack(border));

        if (caps.TryGetValue(key, out var existing))
            return existing;

        var created = CreateCap(key);
        caps.Add(key, created);
        return created;
    }

    private Texture2D CreateCap(BlendControlCapKey key)
    {
        var pixels = new Vec4u8[key.RadiusPixels * key.HeightPixels];
        var fill = Unpack(key.Fill);
        var border = Unpack(key.Border);

        for (var y = 0; y < key.HeightPixels; y++)
        {
            for (var x = 0; x < key.RadiusPixels; x++)
            {
                var outerCoverage = Coverage(x, y, key.HeightPixels, 0f, key.RadiusPixels, key.RadiusPixels);
                var innerCoverage = Coverage(
                    x,
                    y,
                    key.HeightPixels,
                    key.BorderPixels,
                    key.RadiusPixels - key.BorderPixels,
                    key.RadiusPixels);
                var borderCoverage = MathF.Max(0f, outerCoverage - innerCoverage);
                pixels[y * key.RadiusPixels + x] = ToCoveragePixel(fill, innerCoverage, border, borderCoverage);
            }
        }

        var texture = new Texture2D(gl, ((uint)key.RadiusPixels, (uint)key.HeightPixels))
        {
            Pixels = pixels,
            MinFilter = GlTextureMinFilter.Nearest,
            MagFilter = GlTextureMagFilter.Nearest,
            WrapS = GlTextureWrapMode.ClampToEdge,
            WrapT = GlTextureWrapMode.ClampToEdge,
        };
        return texture;
    }

    private static float Coverage(int pixelX, int pixelY, int height, float inset, float radius, float outerRadius)
    {
        if (radius <= 0f)
            return pixelX + 1f > inset && pixelY + 1f > inset && pixelY < height - inset ? 1f : 0f;

        var covered = 0;
        var top = inset;
        var bottom = height - inset;
        var centerX = inset + radius;
        var topCenterY = top + radius;
        var bottomCenterY = bottom - radius;

        for (var sy = 0; sy < CoverageSamples; sy++)
        {
            var y = pixelY + (sy + 0.5f) / CoverageSamples;
            for (var sx = 0; sx < CoverageSamples; sx++)
            {
                var x = pixelX + (sx + 0.5f) / CoverageSamples;
                if (ContainsLeftRoundedRect(x, y, inset, top, bottom, centerX, topCenterY, bottomCenterY, radius, outerRadius))
                    covered++;
            }
        }

        return covered / (float)(CoverageSamples * CoverageSamples);
    }

    private static bool ContainsLeftRoundedRect(
        float x,
        float y,
        float left,
        float top,
        float bottom,
        float centerX,
        float topCenterY,
        float bottomCenterY,
        float radius,
        float outerRadius)
    {
        if (x < left || y < top || y >= bottom || x >= outerRadius)
            return false;

        if (x < centerX && y < topCenterY)
            return DistanceSquared(x, y, centerX, topCenterY) <= radius * radius;

        if (x < centerX && y >= bottomCenterY)
            return DistanceSquared(x, y, centerX, bottomCenterY) <= radius * radius;

        return true;
    }

    private static float DistanceSquared(float x, float y, float centerX, float centerY)
    {
        var dx = x - centerX;
        var dy = y - centerY;
        return (dx * dx) + (dy * dy);
    }

    private static uint Pack(Vec4 color)
    {
        var r = ToByte(color.X);
        var g = ToByte(color.Y);
        var b = ToByte(color.Z);
        var a = ToByte(color.W);
        return ((uint)r << 24) | ((uint)g << 16) | ((uint)b << 8) | a;
    }

    private static Vec4 Unpack(uint color) => (
        ((color >> 24) & 0xFF) / 255f,
        ((color >> 16) & 0xFF) / 255f,
        ((color >> 8) & 0xFF) / 255f,
        (color & 0xFF) / 255f);

    private static Vec4u8 ToPixel(Vec4 color) => (
        ToByte(color.X),
        ToByte(color.Y),
        ToByte(color.Z),
        ToByte(color.W));

    private static Vec4u8 ToCoveragePixel(Vec4 fill, float fillCoverage, Vec4 border, float borderCoverage)
    {
        var alpha = fill.W * fillCoverage + border.W * borderCoverage;
        if (alpha <= 0f)
            return (0, 0, 0, 0);

        var red = ((fill.X * fill.W * fillCoverage) + (border.X * border.W * borderCoverage)) / alpha;
        var green = ((fill.Y * fill.W * fillCoverage) + (border.Y * border.W * borderCoverage)) / alpha;
        var blue = ((fill.Z * fill.W * fillCoverage) + (border.Z * border.W * borderCoverage)) / alpha;
        return ToPixel((red, green, blue, alpha));
    }

    private static byte ToByte(float value) =>
        (byte)Math.Clamp((int)MathF.Round(value * 255f), 0, 255);

    private readonly record struct BlendControlCapKey(
        int HeightPixels,
        int RadiusPixels,
        int BorderPixels,
        uint Fill,
        uint Border);
}
