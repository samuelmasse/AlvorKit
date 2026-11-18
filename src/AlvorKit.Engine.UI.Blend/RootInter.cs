namespace AlvorKit;

/// <summary>Shares the embedded Inter faces for the lifetime of the engine font collection.</summary>
[Root]
public class RootInter(RootFonts fonts) : BlendFonts(fonts.Open);
