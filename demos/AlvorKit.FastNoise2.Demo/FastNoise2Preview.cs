namespace AlvorKit;

/// <summary>Generation shape visualized by the interactive feature gallery.</summary>
internal enum FastNoise2PreviewMode
{
    Uniform2D,
    Uniform3DSlice,
    Uniform4DSlice,
    Tileable2D,
}

/// <summary>Owns reusable sample and pixel buffers plus the root-scoped texture receiving each preview.</summary>
internal class FastNoise2Preview
{
    private readonly float[] values;
    private readonly float[] minMax = new float[2];
    private readonly Vec4u8[] pixels;
    private readonly int width;
    private readonly int height;
    /// <summary>Reusable preview texture owned by the root GL scope.</summary>
    private readonly Texture2D texture;

    /// <summary>Gets the current generated preview for sprite rendering.</summary>
    public Texture2D Texture => texture;

    /// <summary>Allocates reusable buffers and a texture owned by the injected root GL layer.</summary>
    public FastNoise2Preview(RootGl gl, Vec2u size)
    {
        width = (int)size.X;
        height = (int)size.Y;
        values = new float[width * height];
        pixels = new Vec4u8[values.Length];
        texture = new(gl, size)
        {
            MinFilter = GlTextureMinFilter.Linear,
            MagFilter = GlTextureMagFilter.Linear,
            WrapS = GlTextureWrapMode.ClampToEdge,
            WrapT = GlTextureWrapMode.ClampToEdge,
        };
    }

    /// <summary>Generates the selected shape, converts its output contract to RGBA8, and uploads the preview.</summary>
    public void Generate(FnGraphNode root, FastNoise2PreviewMode mode, int seed, bool packedRgba8)
    {
        switch (mode)
        {
            case FastNoise2PreviewMode.Uniform2D:
                root.GenUniformGrid2D(values, (-width * 0.5f, -height * 0.5f), (width, height), (1f, 1f), seed, minMax);
                break;
            case FastNoise2PreviewMode.Uniform3DSlice:
                root.GenUniformGrid3D(
                    values, (-width * 0.5f, -height * 0.5f, 37f), (width, height, 1), Vec3.One, seed, minMax);
                break;
            case FastNoise2PreviewMode.Uniform4DSlice:
                root.GenUniformGrid4D(
                    values, (-width * 0.5f, -height * 0.5f, 37f, 19f), (width, height, 1, 1), Vec4.One, seed, minMax);
                break;
            case FastNoise2PreviewMode.Tileable2D:
                root.GenTileable2D(values, (width, height), (1f, 1f), seed, minMax);
                break;
        }

        if (packedRgba8)
            WritePackedPixels();
        else WriteNumericPixels();

        Texture.Pixels = pixels;
    }

    private void WriteNumericPixels()
    {
        var low = minMax[0];
        var high = minMax[1];
        if (!float.IsFinite(low) || !float.IsFinite(high))
            throw new InvalidOperationException("FastNoise2 returned a non-finite preview range.");

        var scale = high > low ? 1f / (high - low) : 0f;

        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index];
            if (!float.IsFinite(value))
                throw new InvalidOperationException("FastNoise2 returned a non-finite preview sample.");

            var normalized = high > low ? Math.Clamp((value - low) * scale, 0f, 1f) : 0.5f;
            var gray = Byte(normalized * 255f);
            pixels[index] = (gray, gray, gray, 255);
        }
    }

    private void WritePackedPixels()
    {
        for (var index = 0; index < values.Length; index++)
        {
            var packed = BitConverter.SingleToUInt32Bits(values[index]);
            pixels[index] = ((byte)packed, (byte)(packed >> 8), (byte)(packed >> 16), (byte)(packed >> 24));
        }
    }

    private static byte Byte(float value) => (byte)Math.Clamp(MathF.Round(value), 0f, 255f);
}
