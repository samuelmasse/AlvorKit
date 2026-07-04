namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Provides OpenGL texture limits used by dense visualizer rasters.</summary>
[App]
public class AppTextureLimits(RootGl gl)
{
    private int maxTextureWidth;

    /// <summary>Gets the largest one-row texture width the visualizer should request.</summary>
    public int MaxTextureWidth
    {
        get
        {
            if (maxTextureWidth == 0)
            {
                gl.GetIntegerv(GlGetPName.MaxTextureSize, out maxTextureWidth);
                maxTextureWidth = Math.Max(1, maxTextureWidth);
            }

            return maxTextureWidth;
        }
    }
}
