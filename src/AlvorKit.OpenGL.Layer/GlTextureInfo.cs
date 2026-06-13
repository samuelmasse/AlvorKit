namespace AlvorKit.OpenGL.Layer;

/// <summary>
/// The shape of a texture (or renderbuffer) level, used to estimate its GPU memory: volume times
/// samples times an estimated bytes-per-pixel derived from the sized internal format, or from the
/// transfer format and type when the internal format is unsized.
/// </summary>
public readonly record struct GlTextureInfo(InternalFormat InternalFormat, (int Width, int Height, int Depth) Size, PixelFormat PixelFormat, PixelType PixelType, int Samples = 1)
{
    /// <summary>The estimated byte size of one mip level with this shape.</summary>
    public long MemoryUsage
    {
        get
        {
            long volume = (long)Size.Width * Size.Height * Size.Depth;
            return volume * Math.Max(Samples, 1) * EstimateBytesPerPixel();
        }
    }

    private int EstimateBytesPerPixel()
    {
        int sized = EstimateFromSizedFormat(InternalFormat);
        return sized > 0 ? sized : EstimateFromFormatAndType(PixelFormat, PixelType);
    }

    private static int EstimateFromSizedFormat(InternalFormat format) => format switch
    {
        InternalFormat.R8 or InternalFormat.R8i or InternalFormat.R8ui or InternalFormat.R8Snorm => 1,
        InternalFormat.Rg8 or InternalFormat.Rg8i or InternalFormat.Rg8ui or InternalFormat.Rg8Snorm => 2,
        InternalFormat.Rgb8 or InternalFormat.Rgb8i or InternalFormat.Rgb8ui or InternalFormat.Rgb8Snorm or InternalFormat.Srgb8 => 3,
        InternalFormat.Rgba8 or InternalFormat.Rgba8i or InternalFormat.Rgba8ui or InternalFormat.Rgba8Snorm or InternalFormat.Srgb8Alpha8 => 4,

        InternalFormat.R16 or InternalFormat.R16f or InternalFormat.R16i or InternalFormat.R16ui or InternalFormat.R16Snorm => 2,
        InternalFormat.Rg16 or InternalFormat.Rg16f or InternalFormat.Rg16i or InternalFormat.Rg16ui or InternalFormat.Rg16Snorm => 4,
        InternalFormat.Rgb16 or InternalFormat.Rgb16f or InternalFormat.Rgb16i or InternalFormat.Rgb16ui or InternalFormat.Rgb16Snorm => 6,
        InternalFormat.Rgba16 or InternalFormat.Rgba16f or InternalFormat.Rgba16i or InternalFormat.Rgba16ui or InternalFormat.Rgba16Snorm => 8,

        InternalFormat.R32f or InternalFormat.R32i or InternalFormat.R32ui => 4,
        InternalFormat.Rg32f or InternalFormat.Rg32i or InternalFormat.Rg32ui => 8,
        InternalFormat.Rgb32f or InternalFormat.Rgb32i or InternalFormat.Rgb32ui => 12,
        InternalFormat.Rgba32f or InternalFormat.Rgba32i or InternalFormat.Rgba32ui => 16,

        InternalFormat.R3G3B2 or InternalFormat.Rgba2 => 1,
        InternalFormat.Rgb4 or InternalFormat.Rgb5 or InternalFormat.Rgb565 or InternalFormat.Rgba4 or InternalFormat.Rgb5A1 => 2,
        InternalFormat.Rgb10 or InternalFormat.Rgb10A2 or InternalFormat.Rgb10A2ui or InternalFormat.R11fG11fB10f or InternalFormat.Rgb9E5 => 4,
        InternalFormat.Rgb12 or InternalFormat.Rgba12 => 6,

        InternalFormat.DepthComponent16 => 2,
        InternalFormat.DepthComponent24 or InternalFormat.DepthComponent32 or InternalFormat.DepthComponent32f or InternalFormat.Depth24Stencil8 => 4,
        InternalFormat.Depth32fStencil8 => 8,

        InternalFormat.StencilIndex1 or InternalFormat.StencilIndex4 or InternalFormat.StencilIndex8 => 1,
        InternalFormat.StencilIndex16 => 2,

        InternalFormat.CompressedRedRgtc1 or InternalFormat.CompressedSignedRedRgtc1 or InternalFormat.CompressedRgRgtc2 or InternalFormat.CompressedSignedRgRgtc2 => 1,
        InternalFormat.CompressedRgbaBptcUnorm or InternalFormat.CompressedSrgbAlphaBptcUnorm or InternalFormat.CompressedRgbBptcSignedFloat or InternalFormat.CompressedRgbBptcUnsignedFloat => 1,

        _ => 0,
    };

    private static int EstimateFromFormatAndType(PixelFormat format, PixelType type)
    {
        int packed = GetPackedTypeBytes(type);
        return packed > 0 ? packed : GetComponentCount(format) * GetComponentSize(type);
    }

    private static int GetPackedTypeBytes(PixelType type) => type switch
    {
        PixelType.UnsignedByte3_3_2 or PixelType.UnsignedByte2_3_3Rev => 1,
        PixelType.UnsignedShort5_6_5 or PixelType.UnsignedShort5_6_5Rev or PixelType.UnsignedShort4_4_4_4 or PixelType.UnsignedShort4_4_4_4Rev or PixelType.UnsignedShort5_5_5_1 or PixelType.UnsignedShort1_5_5_5Rev => 2,
        PixelType.UnsignedInt8_8_8_8 or PixelType.UnsignedInt8_8_8_8Rev or PixelType.UnsignedInt10_10_10_2 or PixelType.UnsignedInt2_10_10_10Rev or PixelType.UnsignedInt24_8 or PixelType.UnsignedInt10F11F11FRev or PixelType.UnsignedInt5_9_9_9Rev => 4,
        PixelType.Float32UnsignedInt24_8Rev => 8,
        _ => 0,
    };

    private static int GetComponentCount(PixelFormat format) => format switch
    {
        PixelFormat.Red or PixelFormat.Green or PixelFormat.Blue or PixelFormat.RedInteger or PixelFormat.GreenInteger or PixelFormat.BlueInteger or PixelFormat.StencilIndex or PixelFormat.DepthComponent => 1,
        PixelFormat.Rg or PixelFormat.RgInteger or PixelFormat.DepthStencil => 2,
        PixelFormat.Rgb or PixelFormat.Bgr or PixelFormat.RgbInteger or PixelFormat.BgrInteger => 3,
        PixelFormat.Rgba or PixelFormat.Bgra or PixelFormat.RgbaInteger or PixelFormat.BgraInteger => 4,
        _ => 4,
    };

    private static int GetComponentSize(PixelType type) => type switch
    {
        PixelType.UnsignedByte or PixelType.Byte => 1,
        PixelType.UnsignedShort or PixelType.Short or PixelType.HalfFloat => 2,
        PixelType.UnsignedInt or PixelType.Int or PixelType.Float => 4,
        _ => 1,
    };
}
