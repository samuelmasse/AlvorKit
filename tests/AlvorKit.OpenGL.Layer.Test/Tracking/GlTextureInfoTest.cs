namespace AlvorKit.OpenGL.Layer.Test;

/// <summary>
/// Tests byte-size estimates for texture and renderbuffer shapes.
/// </summary>
[TestClass]
public class GlTextureInfoTest
{
    [DataTestMethod]
    [DataRow(GlInternalFormat.R8, 1)]
    [DataRow(GlInternalFormat.Rg8, 2)]
    [DataRow(GlInternalFormat.Rgb8, 3)]
    [DataRow(GlInternalFormat.Rgba8, 4)]
    [DataRow(GlInternalFormat.R16, 2)]
    [DataRow(GlInternalFormat.Rg16, 4)]
    [DataRow(GlInternalFormat.Rgb16, 6)]
    [DataRow(GlInternalFormat.Rgba16, 8)]
    [DataRow(GlInternalFormat.R32f, 4)]
    [DataRow(GlInternalFormat.Rg32f, 8)]
    [DataRow(GlInternalFormat.Rgb32f, 12)]
    [DataRow(GlInternalFormat.Rgba32f, 16)]
    [DataRow(GlInternalFormat.R3G3B2, 1)]
    [DataRow(GlInternalFormat.Rgb4, 2)]
    [DataRow(GlInternalFormat.Rgb10, 4)]
    [DataRow(GlInternalFormat.Rgb12, 6)]
    [DataRow(GlInternalFormat.DepthComponent16, 2)]
    [DataRow(GlInternalFormat.DepthComponent24, 4)]
    [DataRow(GlInternalFormat.Depth32fStencil8, 8)]
    [DataRow(GlInternalFormat.StencilIndex1, 1)]
    [DataRow(GlInternalFormat.StencilIndex16, 2)]
    [DataRow(GlInternalFormat.CompressedRedRgtc1, 1)]
    [DataRow(GlInternalFormat.CompressedRgbaBptcUnorm, 1)]
    public void MemoryUsage_SizedInternalFormats_UseSizedEstimate(GlInternalFormat format, int bytes)
    {
        var info = new GlTextureInfo(format, (2, 3, 4), default, default, 2);

        Assert.AreEqual(2L * 3 * 4 * 2 * bytes, info.MemoryUsage);
    }

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
