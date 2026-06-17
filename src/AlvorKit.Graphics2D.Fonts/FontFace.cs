namespace AlvorKit.Graphics2D.Fonts;

/// <summary>Owns one FreeType face and any native-copied bytes backing it.</summary>
internal sealed unsafe class FontFace : IDisposable
{
    /// <summary>The FreeType binding that created the face.</summary>
    private readonly Ft ft;

    /// <summary>The native font data copy backing a memory face.</summary>
    private nint nativeData;

    /// <summary>The opened FreeType face pointer.</summary>
    private readonly FtFaceRec* pointer;

    /// <summary>Opens a face using the supplied options.</summary>
    internal FontFace(Ft ft, FontLibrary library, FontOptions options)
    {
        this.ft = ft;
        if (options.File is { } file)
        {
            pointer = NewFace(library, file, options.Index);
            return;
        }

        pointer = NewMemoryFace(library, options.Data.HasValue ? options.Data.Value.Span : [], options.Index);
    }

    /// <summary>Opens a memory-backed face by copying bytes into native memory.</summary>
    internal FontFace(Ft ft, FontLibrary library, ReadOnlySpan<byte> data, int index)
    {
        this.ft = ft;
        pointer = NewMemoryFace(library, data, index);
    }

    /// <summary>Gets the opened FreeType face pointer.</summary>
    internal FtFaceRec* Pointer => pointer;

    /// <summary>Releases the face and then frees memory-backed native font bytes.</summary>
    public void Dispose()
    {
        try
        {
            FontFreeType.Require(ft, nameof(Ft.DoneFace), ft.DoneFace(pointer));
        }
        finally
        {
            if (nativeData != 0)
            {
                NativeMemory.Free((void*)nativeData);
                nativeData = 0;
            }
        }
    }

    /// <summary>Opens a face from a filesystem path.</summary>
    private FtFaceRec* NewFace(FontLibrary library, string file, nint index)
    {
        FontFreeType.Require(ft, nameof(Ft.NewFace), ft.NewFace(library.Pointer, file, new CLong(index), out var face));
        return face;
    }

    /// <summary>Opens a face from a native copy of caller-supplied bytes.</summary>
    private FtFaceRec* NewMemoryFace(FontLibrary library, ReadOnlySpan<byte> data, nint index)
    {
        if (data.Length > 0)
        {
            nativeData = (nint)NativeMemory.Alloc((nuint)data.Length);
            data.CopyTo(new Span<byte>((void*)nativeData, data.Length));
        }

        try
        {
            FontFreeType.Require(
                ft,
                nameof(Ft.NewMemoryFace),
                ft.NewMemoryFace(library.Pointer, nativeData, new CLong(data.Length), new CLong(index), out var face));
            return face;
        }
        catch
        {
            if (nativeData != 0)
            {
                NativeMemory.Free((void*)nativeData);
                nativeData = 0;
            }

            throw;
        }
    }
}
