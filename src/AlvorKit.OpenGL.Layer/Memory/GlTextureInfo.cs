namespace AlvorKit.OpenGL.Layer;

/// <summary>
/// The shape of a texture (or renderbuffer) level, used to estimate its GPU memory: volume times
/// samples times an estimated bytes-per-pixel derived from the sized internal format, or from the
/// transfer format and type when the internal format is unsized.
/// </summary>
/// <param name="InternalFormat">The internal storage format requested for the texture level.</param>
/// <param name="Size">The texture level dimensions as width, height, and depth.</param>
/// <param name="PixelFormat">The transfer pixel format used when the internal format is unsized.</param>
/// <param name="PixelType">The transfer pixel type used when the internal format is unsized.</param>
/// <param name="Samples">The multisample count for the level, or one for non-multisampled storage.</param>
public readonly partial record struct GlTextureInfo(
    GlInternalFormat InternalFormat,
    (int Width, int Height, int Depth) Size,
    GlPixelFormat PixelFormat,
    GlPixelType PixelType,
    int Samples = 1)
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

    /// <summary>
    /// Estimates bytes per pixel from the strongest available format information.
    /// </summary>
    /// <returns>The estimated number of bytes used by one pixel or texel.</returns>
    private int EstimateBytesPerPixel()
    {
        int sized = EstimateFromSizedFormat(InternalFormat);
        return sized > 0 ? sized : EstimateFromFormatAndType(PixelFormat, PixelType);
    }

    /// <summary>
    /// Estimates bytes per pixel for sized internal formats known to the layer.
    /// </summary>
    /// <param name="format">The internal storage format to inspect.</param>
    /// <returns>The bytes per pixel, or zero when the format is not recognized as sized.</returns>
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

        GlInternalFormat.CompressedRedRgtc1
            or GlInternalFormat.CompressedSignedRedRgtc1
            or GlInternalFormat.CompressedRgRgtc2
            or GlInternalFormat.CompressedSignedRgRgtc2 => 1,
        GlInternalFormat.CompressedRgbaBptcUnorm
            or GlInternalFormat.CompressedSrgbAlphaBptcUnorm
            or GlInternalFormat.CompressedRgbBptcSignedFloat
            or GlInternalFormat.CompressedRgbBptcUnsignedFloat => 1,

        _ => 0,
    };

    /// <summary>
    /// Estimates bytes per pixel from the transfer format and transfer type.
    /// </summary>
    /// <param name="format">The transfer pixel format.</param>
    /// <param name="type">The transfer pixel type.</param>
    /// <returns>The estimated number of bytes used by one pixel or texel.</returns>
    private static int EstimateFromFormatAndType(GlPixelFormat format, GlPixelType type)
    {
        int packed = GetPackedTypeBytes(type);
        return packed > 0 ? packed : GetComponentCount(format) * GetComponentSize(type);
    }

}
