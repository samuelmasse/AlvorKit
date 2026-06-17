namespace AlvorKit.Graphics2D.Fonts;

/// <summary>Reports a font or FreeType operation failure.</summary>
/// <param name="message">The diagnostic failure message.</param>
[ExcludeFromCodeCoverage]
public class FontException(string message) : Exception(message);
