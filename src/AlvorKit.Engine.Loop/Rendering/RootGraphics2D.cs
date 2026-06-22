namespace AlvorKit.Engine.Loop;

/// <summary>Runs the root two-dimensional drawing pass for state and scripts.</summary>
[Root]
[ExcludeFromCodeCoverage]
public sealed class RootGraphics2D(
    RootGl gl,
    RootCanvas canvas,
    RootState state,
    RootScripts scripts,
    RootFonts fonts,
    RootSprites sprites)
{
    /// <summary>Renders the active state's and scripts' two-dimensional draw commands.</summary>
    public void Render()
    {
        fonts.Pack();
        Draw(state.Current.DrawArea, state.Current.Draw);
        foreach (var script in scripts.Span)
            Draw(script.DrawArea, script.Draw);
    }

    /// <summary>Releases two-dimensional root rendering resources.</summary>
    public void Unload()
    {
        fonts.Dispose();
        sprites.Dispose();
    }

    private void Draw(Vec2? area, Action draw)
    {
        Begin(area ?? canvas.Size);
        draw();
        End();
    }

    private void Begin(Vec2 size) => sprites.Begin(size);

    private void End()
    {
        var size = canvas.Size;
        gl.Viewport(0, 0, checked((int)size.X), checked((int)size.Y));
        gl.Enable(GlEnableCap.Blend);
        gl.Enable(GlEnableCap.CullFace);
        gl.BlendFunc(GlBlendingFactor.SrcAlpha, GlBlendingFactor.OneMinusSrcAlpha);
        gl.CullFace(GlTriangleFace.Back);
        sprites.End();
        gl.ResetCullFace();
        gl.ResetBlendFunc();
        gl.Disable(GlEnableCap.CullFace);
        gl.Disable(GlEnableCap.Blend);
        gl.ResetViewport();
    }
}
