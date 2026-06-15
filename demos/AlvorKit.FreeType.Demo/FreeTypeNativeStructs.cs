namespace AlvorKit.FreeType.Demo;

/// <summary>Native FT_Open_Args layout used by FT_Open_Face and FT_Attach_Stream in this demo.</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct FtOpenArgs
{
    /// <summary>FT_OPEN_* flags.</summary>
    public uint Flags;

    /// <summary>FT_OPEN_MEMORY base pointer, unused here.</summary>
    public nint MemoryBase;

    /// <summary>FT_OPEN_MEMORY byte size, unused here.</summary>
    public CLong MemorySize;

    /// <summary>FT_OPEN_PATHNAME UTF-8 path pointer.</summary>
    public nint Pathname;

    /// <summary>FT_OPEN_STREAM stream pointer, unused here.</summary>
    public nint Stream;

    /// <summary>FT_OPEN_DRIVER module pointer, unused here.</summary>
    public nint Driver;

    /// <summary>The number of optional FT_Parameter entries.</summary>
    public int NumParams;

    /// <summary>Pointer to optional FT_Parameter entries.</summary>
    public nint Params;
}

/// <summary>Native FT_Size_RequestRec layout used by FT_Request_Size in this demo.</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct FtSizeRequestRec
{
    /// <summary>The FT_Size_Request_Type enum value.</summary>
    public FtSizeRequestType Type;

    /// <summary>The requested width in 26.6 fractional points or pixels, depending on <see cref="Type"/>.</summary>
    public CLong Width;

    /// <summary>The requested height in 26.6 fractional points or pixels, depending on <see cref="Type"/>.</summary>
    public CLong Height;

    /// <summary>The horizontal resolution in dpi.</summary>
    public uint HoriResolution;

    /// <summary>The vertical resolution in dpi.</summary>
    public uint VertResolution;
}

/// <summary>Native FT_Matrix layout used by FT_Set_Transform and FT_Vector_Transform.</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct FtMatrix
{
    /// <summary>The x-to-x 16.16 fixed-point coefficient.</summary>
    public CLong Xx;

    /// <summary>The x-to-y 16.16 fixed-point coefficient.</summary>
    public CLong Xy;

    /// <summary>The y-to-x 16.16 fixed-point coefficient.</summary>
    public CLong Yx;

    /// <summary>The y-to-y 16.16 fixed-point coefficient.</summary>
    public CLong Yy;

    /// <summary>Creates a 16.16 fixed-point rotation matrix.</summary>
    /// <param name="degrees">Clockwise degrees in the demo's image coordinate space.</param>
    /// <returns>A native FreeType matrix.</returns>
    public static FtMatrix RotateDegrees(double degrees)
    {
        var radians = degrees * Math.PI / 180.0;
        var cos = Math.Cos(radians);
        var sin = Math.Sin(radians);
        return new FtMatrix
        {
            Xx = FreeTypeValues.Fixed(cos),
            Xy = FreeTypeValues.Fixed(-sin),
            Yx = FreeTypeValues.Fixed(sin),
            Yy = FreeTypeValues.Fixed(cos),
        };
    }
}
