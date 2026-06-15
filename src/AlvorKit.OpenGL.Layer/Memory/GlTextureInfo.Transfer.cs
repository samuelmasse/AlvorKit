namespace AlvorKit.OpenGL.Layer;

public readonly partial record struct GlTextureInfo
{
    /// <summary>
    /// Returns the byte width for packed transfer types.
    /// </summary>
    /// <param name="type">The transfer pixel type.</param>
    /// <returns>The packed byte width, or zero when the type is not packed.</returns>
    private static int GetPackedTypeBytes(GlPixelType type) => type switch
    {
        GlPixelType.UnsignedByte3_3_2 or GlPixelType.UnsignedByte2_3_3Rev => 1,
        GlPixelType.UnsignedShort5_6_5
            or GlPixelType.UnsignedShort5_6_5Rev
            or GlPixelType.UnsignedShort4_4_4_4
            or GlPixelType.UnsignedShort4_4_4_4Rev
            or GlPixelType.UnsignedShort5_5_5_1
            or GlPixelType.UnsignedShort1_5_5_5Rev => 2,
        GlPixelType.UnsignedInt8_8_8_8
            or GlPixelType.UnsignedInt8_8_8_8Rev
            or GlPixelType.UnsignedInt10_10_10_2
            or GlPixelType.UnsignedInt2_10_10_10Rev
            or GlPixelType.UnsignedInt24_8
            or GlPixelType.UnsignedInt10F11F11FRev
            or GlPixelType.UnsignedInt5_9_9_9Rev => 4,
        GlPixelType.Float32UnsignedInt24_8Rev => 8,
        _ => 0,
    };

    /// <summary>
    /// Returns the component count implied by a transfer pixel format.
    /// </summary>
    /// <param name="format">The transfer pixel format.</param>
    /// <returns>The number of components per pixel.</returns>
    private static int GetComponentCount(GlPixelFormat format) => format switch
    {
        GlPixelFormat.Red
            or GlPixelFormat.Green
            or GlPixelFormat.Blue
            or GlPixelFormat.RedInteger
            or GlPixelFormat.GreenInteger
            or GlPixelFormat.BlueInteger
            or GlPixelFormat.StencilIndex
            or GlPixelFormat.DepthComponent => 1,
        GlPixelFormat.Rg or GlPixelFormat.RgInteger or GlPixelFormat.DepthStencil => 2,
        GlPixelFormat.Rgb or GlPixelFormat.Bgr or GlPixelFormat.RgbInteger or GlPixelFormat.BgrInteger => 3,
        GlPixelFormat.Rgba or GlPixelFormat.Bgra or GlPixelFormat.RgbaInteger or GlPixelFormat.BgraInteger => 4,
        _ => 4,
    };

    /// <summary>
    /// Returns the byte width of one component for scalar transfer types.
    /// </summary>
    /// <param name="type">The transfer pixel type.</param>
    /// <returns>The byte width of one component.</returns>
    private static int GetComponentSize(GlPixelType type) => type switch
    {
        GlPixelType.UnsignedByte or GlPixelType.Byte => 1,
        GlPixelType.UnsignedShort or GlPixelType.Short or GlPixelType.HalfFloat => 2,
        GlPixelType.UnsignedInt or GlPixelType.Int or GlPixelType.Float => 4,
        _ => 1,
    };
}
