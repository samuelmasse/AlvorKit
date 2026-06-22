namespace AlvorKit.Engine;

/// <summary>Clears the root window backbuffer with strict OpenGL state pairing.</summary>
[Root]
[ExcludeFromCodeCoverage]
public sealed class RootBackbuffer(RootGl gl, RootCanvas canvas)
{
    /// <summary>Clears color and depth for the current canvas size.</summary>
    public void Clear(Vec4 color = default)
    {
        var size = canvas.Size;
        gl.Viewport(0, 0, checked((int)size.X), checked((int)size.Y));
        gl.ClearColor(color.X, color.Y, color.Z, color.W);
        gl.ClearDepth(1);
        gl.Clear(GlClearBufferMask.ColorBufferBit | GlClearBufferMask.DepthBufferBit);
        gl.ResetClearDepth();
        gl.ResetClearColor();
        gl.ResetViewport();
    }
}
