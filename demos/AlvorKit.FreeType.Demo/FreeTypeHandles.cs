namespace AlvorKit.FreeType.Demo;

/// <summary>Owns an FT_Library handle and releases it through FT_Done_FreeType.</summary>
/// <param name="ft">The FreeType binding that owns the destroy call.</param>
/// <param name="handle">The FT_Library handle.</param>
internal readonly struct FreeTypeLibrary(Ft ft, nint handle) : IDisposable
{
    /// <summary>The FT_Library handle.</summary>
    public nint Handle { get; } = handle;

    /// <summary>Calls FT_Done_FreeType for the library handle.</summary>
    public void Dispose() => FreeTypeStatus.Require(ft, "FT_Done_FreeType", ft.DoneFreeType(Handle));
}

/// <summary>Owns an FT_Face handle and releases it through FT_Done_Face.</summary>
/// <param name="ft">The FreeType binding that owns the destroy call.</param>
/// <param name="handle">The FT_Face handle.</param>
internal readonly struct FreeTypeFace(Ft ft, nint handle) : IDisposable
{
    /// <summary>The FT_Face handle.</summary>
    public nint Handle { get; } = handle;

    /// <summary>Calls FT_Done_Face for the face handle.</summary>
    public void Dispose() => FreeTypeStatus.Require(ft, "FT_Done_Face", ft.DoneFace(Handle));
}

/// <summary>Pins a managed byte array for FreeType APIs that require stable font memory.</summary>
/// <param name="bytes">The bytes to pin.</param>
internal sealed class PinnedBytes(byte[] bytes) : IDisposable
{
    private readonly GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

    /// <summary>The pinned byte array.</summary>
    public byte[] Bytes { get; } = bytes;

    /// <summary>The pinned native pointer.</summary>
    public nint Pointer => handle.AddrOfPinnedObject();

    /// <summary>The number of pinned bytes.</summary>
    public int Length => Bytes.Length;

    /// <summary>Releases the pinned GC handle.</summary>
    public void Dispose() => handle.Free();
}

/// <summary>Creates FreeType handles in the same order as the C API calls used by the demo.</summary>
internal static class FreeTypeHandles
{
    /// <summary>Calls FT_Init_FreeType and returns an owned library handle.</summary>
    /// <param name="ft">The FreeType binding.</param>
    /// <returns>An owned FT_Library wrapper.</returns>
    public static FreeTypeLibrary InitLibrary(Ft ft)
    {
        FreeTypeStatus.Require(ft, "FT_Init_FreeType", ft.InitFreeType(out var handle));
        return new FreeTypeLibrary(ft, handle);
    }

    /// <summary>Calls FT_New_Face for a pathname and returns an owned face handle.</summary>
    /// <param name="ft">The FreeType binding.</param>
    /// <param name="library">The owning FT_Library handle.</param>
    /// <param name="fontPath">The font pathname.</param>
    /// <returns>An owned FT_Face wrapper.</returns>
    public static FreeTypeFace NewFace(Ft ft, nint library, string fontPath)
    {
        FreeTypeStatus.Require(ft, "FT_New_Face", ft.NewFace(library, fontPath, FreeTypeValues.Long(0), out var face));
        return new FreeTypeFace(ft, face);
    }

    /// <summary>Calls FT_New_Memory_Face for pinned managed font bytes and returns an owned face handle.</summary>
    /// <param name="ft">The FreeType binding.</param>
    /// <param name="library">The owning FT_Library handle.</param>
    /// <param name="font">The pinned font bytes that must outlive the face.</param>
    /// <returns>An owned FT_Face wrapper.</returns>
    public static FreeTypeFace NewMemoryFace(Ft ft, nint library, PinnedBytes font)
    {
        FreeTypeStatus.Require(
            ft,
            "FT_New_Memory_Face",
            ft.NewMemoryFace(library, font.Pointer, FreeTypeValues.Long(font.Length), FreeTypeValues.Long(0), out var face));

        return new FreeTypeFace(ft, face);
    }

    /// <summary>Calls FT_Open_Face with FT_OPEN_PATHNAME and returns an owned face handle.</summary>
    /// <param name="ft">The FreeType binding.</param>
    /// <param name="library">The owning FT_Library handle.</param>
    /// <param name="fontPath">The font pathname.</param>
    /// <returns>An owned FT_Face wrapper.</returns>
    public static unsafe FreeTypeFace OpenPathFace(Ft ft, nint library, string fontPath)
    {
        var pathBytes = Encoding.UTF8.GetBytes(fontPath + '\0');
        fixed (byte* path = pathBytes)
        {
            var args = new FtOpenArgs
            {
                Flags = Ft.OpenPathname,
                Pathname = (nint)path,
            };

            FreeTypeStatus.Require(ft, "FT_Open_Face", ft.OpenFace(library, (nint)(&args), FreeTypeValues.Long(0), out var face));
            return new FreeTypeFace(ft, face);
        }
    }
}

/// <summary>Formats and checks FreeType FT_Error return values.</summary>
internal static class FreeTypeStatus
{
    /// <summary>Throws an exception if <paramref name="error"/> is nonzero.</summary>
    /// <param name="ft">The FreeType binding used to decode errors.</param>
    /// <param name="cName">The C API name to include in the message.</param>
    /// <param name="error">The FT_Error value.</param>
    public static void Require(Ft ft, string cName, int error)
    {
        if (error == 0)
            return;

        throw new InvalidOperationException($"{cName} failed with FT_Error {error}: {Describe(ft, error)}");
    }

    /// <summary>Writes a status line for an optional FreeType feature that might not be supported by the current font.</summary>
    /// <param name="ft">The FreeType binding used to decode errors.</param>
    /// <param name="cName">The C API name to include in the message.</param>
    /// <param name="error">The FT_Error value.</param>
    public static void ReportOptional(Ft ft, string cName, int error)
    {
        if (error == 0)
            Console.WriteLine($"{cName}: supported");
        else
            Console.WriteLine($"{cName}: unavailable for this font ({error}, {Describe(ft, error)})");
    }

    /// <summary>Returns the native error string for an FT_Error value when the build exposes one.</summary>
    /// <param name="ft">The FreeType binding used to decode the error.</param>
    /// <param name="error">The FT_Error value.</param>
    /// <returns>The decoded error string, or a fallback message.</returns>
    public static string Describe(Ft ft, int error)
    {
        ft.ErrorString(error, out var value);
        return value ?? "native build did not expose an error string";
    }
}
