namespace AlvorKit.UI.Blend;

/// <summary>One dropdown option: display text plus an optional swatch color shown before it.</summary>
/// <param name="Text">Display text for the option row and the closed field.</param>
/// <param name="Swatch">Optional swatch color; a zero-alpha value hides the swatch.</param>
public readonly record struct BlendDropdownItem(string Text, Vec4 Swatch = default);
