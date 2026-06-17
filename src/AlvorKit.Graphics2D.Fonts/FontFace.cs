namespace AlvorKit.Graphics2D.Fonts;

/// <summary>Owns one FreeType face and any pinned managed bytes backing it.</summary>
internal sealed unsafe class FontFace : IDisposable
{
    /// <summary>The FreeType driver that created the face.</summary>
    private readonly FontDriver driver;

    /// <summary>The pinned managed font data for memory-backed faces.</summary>
    private GCHandle pinnedData;

    /// <summary>The opened FreeType face pointer.</summary>
    private readonly FtFaceRec* pointer;

    /// <summary>Opens a face using the supplied options.</summary>
    internal FontFace(FontDriver driver, FontLibrary library, FontOptions options)
    {
        this.driver = driver;
        if (options.File is { } file)
        {
            pointer = driver.NewFace(library.Pointer, file, options.Index);
            return;
        }

        var data = options.Data ?? [];
        if (data.Length > 0)
            pinnedData = GCHandle.Alloc(data, GCHandleType.Pinned);

        var dataPointer = pinnedData.IsAllocated ? pinnedData.AddrOfPinnedObject() : 0;
        pointer = driver.NewMemoryFace(library.Pointer, dataPointer, data.Length, options.Index);
    }

    /// <summary>Gets the opened FreeType face pointer.</summary>
    internal FtFaceRec* Pointer => pointer;

    /// <summary>Releases the face and then unpins memory-backed font bytes.</summary>
    public void Dispose()
    {
        driver.DoneFace(pointer);
        if (pinnedData.IsAllocated)
            pinnedData.Free();
    }
}
