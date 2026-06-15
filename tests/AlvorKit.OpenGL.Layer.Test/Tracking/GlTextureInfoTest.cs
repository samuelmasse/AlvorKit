namespace AlvorKit.OpenGL.Layer.Test;

/// <summary>
/// Tests byte-size estimates for texture and renderbuffer shapes.
/// </summary>
[TestClass]
public class GlTextureInfoTest
{
    /// <summary>
    /// Provides sized internal format cases and their expected byte estimates.
    /// </summary>
    public static IEnumerable<object[]> SizedInternalFormatCases =>
    [
        [GlInternalFormat.R8, 1],
        [GlInternalFormat.R8i, 1],
        [GlInternalFormat.R8ui, 1],
        [GlInternalFormat.R8Snorm, 1],
        [GlInternalFormat.Rg8, 2],
        [GlInternalFormat.Rg8i, 2],
        [GlInternalFormat.Rg8ui, 2],
        [GlInternalFormat.Rg8Snorm, 2],
        [GlInternalFormat.Rgb8, 3],
        [GlInternalFormat.Rgb8i, 3],
        [GlInternalFormat.Rgb8ui, 3],
        [GlInternalFormat.Rgb8Snorm, 3],
        [GlInternalFormat.SRgb8, 3],
        [GlInternalFormat.Rgba8, 4],
        [GlInternalFormat.Rgba8i, 4],
        [GlInternalFormat.Rgba8ui, 4],
        [GlInternalFormat.Rgba8Snorm, 4],
        [GlInternalFormat.SRgb8Alpha8, 4],
        [GlInternalFormat.R16, 2],
        [GlInternalFormat.R16f, 2],
        [GlInternalFormat.R16i, 2],
        [GlInternalFormat.R16ui, 2],
        [GlInternalFormat.R16Snorm, 2],
        [GlInternalFormat.Rg16, 4],
        [GlInternalFormat.Rg16f, 4],
        [GlInternalFormat.Rg16i, 4],
        [GlInternalFormat.Rg16ui, 4],
        [GlInternalFormat.Rg16Snorm, 4],
        [GlInternalFormat.Rgb16, 6],
        [GlInternalFormat.Rgb16f, 6],
        [GlInternalFormat.Rgb16i, 6],
        [GlInternalFormat.Rgb16ui, 6],
        [GlInternalFormat.Rgb16Snorm, 6],
        [GlInternalFormat.Rgba16, 8],
        [GlInternalFormat.Rgba16f, 8],
        [GlInternalFormat.Rgba16i, 8],
        [GlInternalFormat.Rgba16ui, 8],
        [GlInternalFormat.Rgba16Snorm, 8],
        [GlInternalFormat.R32f, 4],
        [GlInternalFormat.R32i, 4],
        [GlInternalFormat.R32ui, 4],
        [GlInternalFormat.Rg32f, 8],
        [GlInternalFormat.Rg32i, 8],
        [GlInternalFormat.Rg32ui, 8],
        [GlInternalFormat.Rgb32f, 12],
        [GlInternalFormat.Rgb32i, 12],
        [GlInternalFormat.Rgb32ui, 12],
        [GlInternalFormat.Rgba32f, 16],
        [GlInternalFormat.Rgba32i, 16],
        [GlInternalFormat.Rgba32ui, 16],
        [GlInternalFormat.R3G3B2, 1],
        [GlInternalFormat.Rgba2, 1],
        [GlInternalFormat.Rgb4, 2],
        [GlInternalFormat.Rgb5, 2],
        [GlInternalFormat.Rgb565, 2],
        [GlInternalFormat.Rgba4, 2],
        [GlInternalFormat.Rgb5A1, 2],
        [GlInternalFormat.Rgb10, 4],
        [GlInternalFormat.Rgb10A2, 4],
        [GlInternalFormat.Rgb10A2ui, 4],
        [GlInternalFormat.R11fG11fB10f, 4],
        [GlInternalFormat.Rgb9E5, 4],
        [GlInternalFormat.Rgb12, 6],
        [GlInternalFormat.Rgba12, 6],
        [GlInternalFormat.DepthComponent16, 2],
        [GlInternalFormat.DepthComponent24, 4],
        [GlInternalFormat.DepthComponent32, 4],
        [GlInternalFormat.DepthComponent32f, 4],
        [GlInternalFormat.Depth24Stencil8, 4],
        [GlInternalFormat.Depth32fStencil8, 8],
        [GlInternalFormat.StencilIndex1, 1],
        [GlInternalFormat.StencilIndex4, 1],
        [GlInternalFormat.StencilIndex8, 1],
        [GlInternalFormat.StencilIndex16, 2],
        [GlInternalFormat.CompressedRedRgtc1, 1],
        [GlInternalFormat.CompressedSignedRedRgtc1, 1],
        [GlInternalFormat.CompressedRgRgtc2, 1],
        [GlInternalFormat.CompressedSignedRgRgtc2, 1],
        [GlInternalFormat.CompressedRgbaBptcUnorm, 1],
        [GlInternalFormat.CompressedSRgbAlphaBptcUnorm, 1],
        [GlInternalFormat.CompressedRgbBptcSignedFloat, 1],
        [GlInternalFormat.CompressedRgbBptcUnsignedFloat, 1],
    ];

    /// <summary>
    /// Verifies sized internal formats use the format-specific byte estimate.
    /// </summary>
    [DataTestMethod]
    [DynamicData(nameof(SizedInternalFormatCases), DynamicDataSourceType.Property)]
    public void MemoryUsage_SizedInternalFormats_UseSizedEstimate(GlInternalFormat format, int bytes)
    {
        var info = new GlTextureInfo(format, (2, 3, 4), default, default, 2);

        Assert.AreEqual(2L * 3 * 4 * 2 * bytes, info.MemoryUsage);
    }

    /// <summary>
    /// Verifies unsized internal formats use the transfer format and type byte estimate.
    /// </summary>
    [DataTestMethod]
    [DataRow(GlPixelFormat.Red, GlPixelType.UnsignedByte, 1)]
    [DataRow(GlPixelFormat.Rg, GlPixelType.UnsignedShort, 4)]
    [DataRow(GlPixelFormat.Rgb, GlPixelType.UnsignedInt, 12)]
    [DataRow(GlPixelFormat.Rgba, GlPixelType.Float, 16)]
    [DataRow(GlPixelFormat.Rgb, GlPixelType.UnsignedByte3_3_2, 1)]
    [DataRow(GlPixelFormat.Rgb, GlPixelType.UnsignedShort5_6_5, 2)]
    [DataRow(GlPixelFormat.DepthStencil, GlPixelType.UnsignedInt24_8, 4)]
    [DataRow(GlPixelFormat.Rgba, GlPixelType.Float32UnsignedInt24_8Rev, 8)]
    [DataRow((GlPixelFormat)0, (GlPixelType)0, 4)]
    public void MemoryUsage_UnsizedInternalFormats_UseTransferEstimate(GlPixelFormat format, GlPixelType type, int bytes)
    {
        var info = new GlTextureInfo(default, (2, 3, 1), format, type);

        Assert.AreEqual(2L * 3 * bytes, info.MemoryUsage);
    }
}
