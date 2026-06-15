using System.Globalization;

namespace AlvorKit.Script.Bindgen;

/// <summary>Parses OpenGL registry token values.</summary>
internal static class GlTokenValueParser
{
    /// <summary>Parses decimal, negative, or hexadecimal registry token values into unsigned storage.</summary>
    public static ulong Parse(string text) =>
        text.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? ulong.Parse(text[2..], NumberStyles.HexNumber)
            : unchecked((ulong)long.Parse(text));
}
