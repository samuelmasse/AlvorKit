namespace AlvorKit;

/// <summary>Owns fonts with engine-root lifetime, including the embedded UI fonts.</summary>
[Root]
public class RootFonts(RootFontPacking packing, Ft ft, RootGl gl) : FontCollection(packing, ft, gl);
