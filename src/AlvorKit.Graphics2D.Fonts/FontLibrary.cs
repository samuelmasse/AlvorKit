namespace AlvorKit.Graphics2D.Fonts;

/// <summary>Owns an initialized FreeType library handle.</summary>
internal sealed class FontLibrary(Ft ft) : IDisposable
{
    /// <summary>The initialized FreeType library handle.</summary>
    private readonly nint pointer = Init(ft);

    /// <summary>Gets the initialized FreeType library handle.</summary>
    internal nint Pointer => pointer;

    /// <summary>Releases the FreeType library handle.</summary>
    public void Dispose() => FontFreeType.Require(ft, nameof(Ft.DoneFreeType), ft.DoneFreeType(pointer));

    /// <summary>Initializes FreeType and returns the library handle.</summary>
    private static nint Init(Ft ft)
    {
        FontFreeType.Require(ft, nameof(Ft.InitFreeType), ft.InitFreeType(out var library));
        return library;
    }
}
