namespace AlvorKit.Script.AlvorEye;

/// <summary>Runs lightweight image checks on captured frames.</summary>
[SupportedOSPlatform("windows6.1")]
internal static class BasicImageAnalysis
{
    /// <summary>Analyzes one frame and optionally compares it with another frame or target color.</summary>
    public static BasicImageAnalysisResult Analyze(string framePath, string? comparePath = null, string? color = null)
    {
        using var frame = new Bitmap(framePath);
        var first = frame.GetPixel(0, 0);
        var changedPixels = 0;
        var colorHits = 0;
        var minX = frame.Width;
        var minY = frame.Height;
        var maxX = -1;
        var maxY = -1;
        Color? target = color is null ? null : ParseColor(color);

        using var compare = comparePath is null ? null : new Bitmap(comparePath);
        for (var y = 0; y < frame.Height; y++)
        {
            for (var x = 0; x < frame.Width; x++)
            {
                var pixel = frame.GetPixel(x, y);
                var changed = ColorDistance(pixel, first) > 8;
                if (compare is not null && x < compare.Width && y < compare.Height && ColorDistance(pixel, compare.GetPixel(x, y)) > 8)
                    changed = true;
                if (changed)
                {
                    changedPixels++;
                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x);
                    maxY = Math.Max(maxY, y);
                }

                if (target is { } targetColor && ColorDistance(pixel, targetColor) < 12)
                    colorHits++;
            }
        }

        var total = frame.Width * frame.Height;
        return new(frame.Width, frame.Height, changedPixels > total / 100, changedPixels, colorHits, minX, minY, maxX, maxY);
    }

    /// <summary>Parses a six-digit RGB hex color.</summary>
    private static Color ParseColor(string color)
    {
        var hex = color.TrimStart('#');
        if (hex.Length != 6)
            throw new ArgumentException("Color must be a six-digit RGB hex value.");
        return Color.FromArgb(
            Convert.ToInt32(hex[..2], 16),
            Convert.ToInt32(hex.Substring(2, 2), 16),
            Convert.ToInt32(hex.Substring(4, 2), 16));
    }

    /// <summary>Computes a simple RGB Manhattan distance.</summary>
    private static int ColorDistance(Color left, Color right) =>
        Math.Abs(left.R - right.R) + Math.Abs(left.G - right.G) + Math.Abs(left.B - right.B);
}
