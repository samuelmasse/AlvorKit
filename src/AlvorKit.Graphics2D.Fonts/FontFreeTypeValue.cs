namespace AlvorKit.Graphics2D.Fonts;

/// <summary>Converts FreeType fixed-point and native integer wrappers into font-space values.</summary>
internal static class FontFreeTypeValue
{
    /// <summary>The number of fractional units in one 26.6 pixel.</summary>
    internal const float PixelOne = 64f;

    /// <summary>Converts a FreeType 26.6 fixed-point value to pixels.</summary>
    internal static float Pixel26Dot6(CLong value) => value.Value.ToInt64() / PixelOne;
}
