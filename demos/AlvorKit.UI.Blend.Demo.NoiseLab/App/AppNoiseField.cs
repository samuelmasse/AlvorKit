namespace AlvorKit;

/// <summary>Samples the selected typed graph into reusable buffers and a root-owned preview texture.</summary>
[App]
public class AppNoiseField(RootGl gl, AppRamps ramps, AppNoiseNodes nodes)
{
    /// <summary>Reusable output range storage passed to native generation.</summary>
    private readonly float[] minMax = new float[2];

    /// <summary>Current preview texture; the root GL scope owns its final disposal.</summary>
    private Texture2D texture = ConfigureTexture(new(gl, (1, 1)));
    /// <summary>Reusable row-major samples for the current viewport.</summary>
    private float[] values = [];
    /// <summary>Reusable ramp-colored pixels uploaded to the preview.</summary>
    private Vec4u8[] pixels = [];
    /// <summary>Elapsed time of the most recent graph sampling call.</summary>
    private double generateMs;

    /// <summary>Gets the typed graph selections and editable parameters.</summary>
    public AppNoiseNodes Nodes => nodes;
    /// <summary>Gets the texture displayed by the viewport.</summary>
    public Texture2D Texture => texture;
    /// <summary>Gets the sample width, or zero before the first resize.</summary>
    public int Width => values.Length == 0 ? 0 : (int)texture.Size.X;
    /// <summary>Gets the sample height, or zero before the first resize.</summary>
    public int Height => values.Length == 0 ? 0 : (int)texture.Size.Y;
    /// <summary>Gets the minimum generated sample used for optional display normalization.</summary>
    public float SampleMin => minMax[0];
    /// <summary>Gets the maximum generated sample used for optional display normalization.</summary>
    public float SampleMax => minMax[1];
    /// <summary>Gets graph sampling time in milliseconds, excluding coloring and upload.</summary>
    public double GenerateMs => generateMs;

    /// <summary>Replaces the texture when the viewport changes and reuses sample storage when its count is unchanged.</summary>
    public bool Resize(int width, int height)
    {
        width = Math.Max(1, width);
        height = Math.Max(1, height);

        if (width == Width && height == Height)
            return false;

        var count = width * height;

        if (values.Length != count)
        {
            values = new float[count];
            pixels = new Vec4u8[count];
        }

        texture.Dispose();
        texture = ConfigureTexture(new(gl, ((uint)width, (uint)height)));
        return true;
    }

    /// <summary>Reads the nearest in-bounds sample for the viewport inspector.</summary>
    public float Sample(int x, int y)
    {
        if (Width == 0)
            return 0f;

        return values[(Math.Clamp(y, 0, Height - 1) * Width) + Math.Clamp(x, 0, Width - 1)];
    }

    /// <summary>Samples a 3D slice, applies the selected display ramp, and uploads its pixels.</summary>
    public void Generate(int seed, Vec2 offset, float step, float z, bool normalize, bool invert, int ramp)
    {
        if (Width == 0)
            return;

        var start = Stopwatch.GetTimestamp();
        nodes.Root.GenUniformGrid3D(values, (offset.X * step, offset.Y * step, z),
            (Width, Height, 1), new Vec3(step), seed, minMax);
        generateMs = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        var low = normalize ? SampleMin : -1f;
        var high = normalize ? SampleMax : 1f;
        var scale = high > low ? 1f / (high - low) : 0f;

        for (var index = 0; index < values.Length; index++)
        {
            var value = Math.Clamp((values[index] - low) * scale, 0f, 1f);

            if (invert)
                value = 1f - value;

            pixels[index] = ramps.Color(ramp, value);
        }

        texture.Pixels = pixels;
    }

    /// <summary>Configures smooth preview sampling without wrapping at the viewport edges.</summary>
    private static Texture2D ConfigureTexture(Texture2D texture)
    {
        texture.MinFilter = GlTextureMinFilter.Linear;
        texture.MagFilter = GlTextureMagFilter.Linear;
        texture.WrapS = GlTextureWrapMode.ClampToEdge;
        texture.WrapT = GlTextureWrapMode.ClampToEdge;
        return texture;
    }
}
