namespace AlvorKit.Graphics2D.Fonts;

/// <summary>Font-wide metrics for one selected pixel size.</summary>
/// <param name="Ascender">The ascender in pixels.</param>
/// <param name="Descender">The descender in pixels.</param>
/// <param name="Height">The recommended line height in pixels.</param>
public readonly record struct FontSizeMetrics(float Ascender, float Descender, float Height);
