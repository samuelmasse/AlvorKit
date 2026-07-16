namespace AlvorKit.Engine.Loop;

/// <summary>Runs the root two-dimensional drawing pass for state and scripts.</summary>
[Root]
[ExcludeFromCodeCoverage]
public class RootGraphics2D(
    RootGl gl,
    RootCanvas canvas,
    RootState state,
    RootScripts scripts,
    RootFonts fonts,
    RootSprites sprites)
{
    public void Unload() => fonts.Unload();

    public void Render()
    {
        fonts.Pack();

        sprites.Begin(canvas.Size);
        state.Current.Draw();
        End();

        foreach (var script in scripts.Span)
        {
            sprites.Begin(script.DrawArea ?? canvas.Size);
            script.Draw();
            End();
        }
    }

    private void End()
    {
        gl.Viewport(canvas.Size);

        gl.Enable(GlEnableCap.Blend);
        gl.Enable(GlEnableCap.CullFace);

        gl.BlendFunc(GlBlendingFactor.SrcAlpha, GlBlendingFactor.OneMinusSrcAlpha);
        gl.CullFace(GlTriangleFace.Back);

        sprites.End();

        gl.ResetBlendFunc();
        gl.ResetCullFace();

        gl.Disable(GlEnableCap.Blend);
        gl.Disable(GlEnableCap.CullFace);

        gl.ResetViewport();
    }
}
