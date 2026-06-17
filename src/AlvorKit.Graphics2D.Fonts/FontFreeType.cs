namespace AlvorKit.Graphics2D.Fonts;

/// <summary>Contains FreeType-specific helpers used by the font runtime.</summary>
internal static class FontFreeType
{
    /// <summary>The number of fractional units in one FreeType 26.6 pixel value.</summary>
    internal const float PixelOne = 64f;

    /// <summary>Converts a FreeType 26.6 fixed-point value to pixels.</summary>
    internal static float Pixel26Dot6(CLong value) => value.Value.ToInt64() / PixelOne;

    /// <summary>Throws a font exception when a FreeType call reports an error.</summary>
    internal static void Require(Ft ft, string method, int error)
    {
        if (error == 0)
            return;

        ft.ErrorString(error, out var description);
        throw new FontException($"FreeType {method} failed with error {error}: {description ?? "unknown error"}");
    }
}
