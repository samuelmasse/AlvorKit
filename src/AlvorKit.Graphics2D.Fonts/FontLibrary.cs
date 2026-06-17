namespace AlvorKit.Graphics2D.Fonts;

/// <summary>Owns an initialized FreeType library handle.</summary>
internal sealed class FontLibrary(FontDriver driver) : IDisposable
{
    /// <summary>The initialized FreeType library handle.</summary>
    private readonly nint pointer = driver.InitFreeType();

    /// <summary>Gets the initialized FreeType library handle.</summary>
    internal nint Pointer => pointer;

    /// <summary>Releases the FreeType library handle.</summary>
    public void Dispose() => driver.DoneFreeType(pointer);
}
