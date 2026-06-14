namespace AlvorKit.OpenGL.Layer;

/// <summary>
/// The shape of a texture (or renderbuffer) level, used to estimate its GPU memory: volume times
/// samples times an estimated bytes-per-pixel derived from the sized internal format, or from the
/// transfer format and type when the internal format is unsized.
/// </summary>
public readonly record struct GlTextureInfo(GlInternalFormat InternalFormat, (int Width, int Height, int Depth) Size, GlPixelFormat PixelFormat, GlPixelType PixelType, int Samples = 1)
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

    private static int EstimateFromSizedFormat(GlInternalFormat format) => format switch
    {
        GlInternalFormat.R8 or GlInternalFormat.R8i or GlInternalFormat.R8ui or GlInternalFormat.R8Snorm => 1,
        GlInternalFormat.Rg8 or GlInternalFormat.Rg8i or GlInternalFormat.Rg8ui or GlInternalFormat.Rg8Snorm => 2,
        GlInternalFormat.Rgb8 or GlInternalFormat.Rgb8i or GlInternalFormat.Rgb8ui or GlInternalFormat.Rgb8Snorm or GlInternalFormat.Srgb8 => 3,
        GlInternalFormat.Rgba8 or GlInternalFormat.Rgba8i or GlInternalFormat.Rgba8ui or GlInternalFormat.Rgba8Snorm or GlInternalFormat.Srgb8Alpha8 => 4,

        GlInternalFormat.R16 or GlInternalFormat.R16f or GlInternalFormat.R16i or GlInternalFormat.R16ui or GlInternalFormat.R16Snorm => 2,
        GlInternalFormat.Rg16 or GlInternalFormat.Rg16f or GlInternalFormat.Rg16i or GlInternalFormat.Rg16ui or GlInternalFormat.Rg16Snorm => 4,
        GlInternalFormat.Rgb16 or GlInternalFormat.Rgb16f or GlInternalFormat.Rgb16i or GlInternalFormat.Rgb16ui or GlInternalFormat.Rgb16Snorm => 6,
        GlInternalFormat.Rgba16 or GlInternalFormat.Rgba16f or GlInternalFormat.Rgba16i or GlInternalFormat.Rgba16ui or GlInternalFormat.Rgba16Snorm => 8,

        GlInternalFormat.R32f or GlInternalFormat.R32i or GlInternalFormat.R32ui => 4,
        GlInternalFormat.Rg32f or GlInternalFormat.Rg32i or GlInternalFormat.Rg32ui => 8,
        GlInternalFormat.Rgb32f or GlInternalFormat.Rgb32i or GlInternalFormat.Rgb32ui => 12,
        GlInternalFormat.Rgba32f or GlInternalFormat.Rgba32i or GlInternalFormat.Rgba32ui => 16,

        GlInternalFormat.R3G3B2 or GlInternalFormat.Rgba2 => 1,
        GlInternalFormat.Rgb4 or GlInternalFormat.Rgb5 or GlInternalFormat.Rgb565 or GlInternalFormat.Rgba4 or GlInternalFormat.Rgb5A1 => 2,
        GlInternalFormat.Rgb10 or GlInternalFormat.Rgb10A2 or GlInternalFormat.Rgb10A2ui or GlInternalFormat.R11fG11fB10f or GlInternalFormat.Rgb9E5 => 4,
        GlInternalFormat.Rgb12 or GlInternalFormat.Rgba12 => 6,

        GlInternalFormat.DepthComponent16 => 2,
        GlInternalFormat.DepthComponent24 or GlInternalFormat.DepthComponent32 or GlInternalFormat.DepthComponent32f or GlInternalFormat.Depth24Stencil8 => 4,
        GlInternalFormat.Depth32fStencil8 => 8,

        GlInternalFormat.StencilIndex1 or GlInternalFormat.StencilIndex4 or GlInternalFormat.StencilIndex8 => 1,
        GlInternalFormat.StencilIndex16 => 2,

        GlInternalFormat.CompressedRedRgtc1 or GlInternalFormat.CompressedSignedRedRgtc1 or GlInternalFormat.CompressedRgRgtc2 or GlInternalFormat.CompressedSignedRgRgtc2 => 1,
        GlInternalFormat.CompressedRgbaBptcUnorm or GlInternalFormat.CompressedSrgbAlphaBptcUnorm or GlInternalFormat.CompressedRgbBptcSignedFloat or GlInternalFormat.CompressedRgbBptcUnsignedFloat => 1,

        _ => 0,
    };

    private static int EstimateFromFormatAndType(GlPixelFormat format, GlPixelType type)
    {
        int packed = GetPackedTypeBytes(type);
        return packed > 0 ? packed : GetComponentCount(format) * GetComponentSize(type);
    }

    private static int GetPackedTypeBytes(GlPixelType type) => type switch
    {
        GlPixelType.UnsignedByte3_3_2 or GlPixelType.UnsignedByte2_3_3Rev => 1,
        GlPixelType.UnsignedShort5_6_5 or GlPixelType.UnsignedShort5_6_5Rev or GlPixelType.UnsignedShort4_4_4_4 or GlPixelType.UnsignedShort4_4_4_4Rev or GlPixelType.UnsignedShort5_5_5_1 or GlPixelType.UnsignedShort1_5_5_5Rev => 2,
        GlPixelType.UnsignedInt8_8_8_8 or GlPixelType.UnsignedInt8_8_8_8Rev or GlPixelType.UnsignedInt10_10_10_2 or GlPixelType.UnsignedInt2_10_10_10Rev or GlPixelType.UnsignedInt24_8 or GlPixelType.UnsignedInt10F11F11FRev or GlPixelType.UnsignedInt5_9_9_9Rev => 4,
        GlPixelType.Float32UnsignedInt24_8Rev => 8,
        _ => 0,
    };

    private static int GetComponentCount(GlPixelFormat format) => format switch
    {
        GlPixelFormat.Red or GlPixelFormat.Green or GlPixelFormat.Blue or GlPixelFormat.RedInteger or GlPixelFormat.GreenInteger or GlPixelFormat.BlueInteger or GlPixelFormat.StencilIndex or GlPixelFormat.DepthComponent => 1,
        GlPixelFormat.Rg or GlPixelFormat.RgInteger or GlPixelFormat.DepthStencil => 2,
        GlPixelFormat.Rgb or GlPixelFormat.Bgr or GlPixelFormat.RgbInteger or GlPixelFormat.BgrInteger => 3,
        GlPixelFormat.Rgba or GlPixelFormat.Bgra or GlPixelFormat.RgbaInteger or GlPixelFormat.BgraInteger => 4,
        _ => 4,
    };

    private static int GetComponentSize(GlPixelType type) => type switch
    {
        GlPixelType.UnsignedByte or GlPixelType.Byte => 1,
        GlPixelType.UnsignedShort or GlPixelType.Short or GlPixelType.HalfFloat => 2,
        GlPixelType.UnsignedInt or GlPixelType.Int or GlPixelType.Float => 4,
        _ => 1,
    };
}
