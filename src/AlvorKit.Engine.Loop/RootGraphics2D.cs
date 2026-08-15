namespace AlvorKit;

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
        End(null);

        foreach (var script in scripts.Span)
        {
            var viewport = script.DrawViewport;
            sprites.Begin(script.DrawArea ?? viewport?.Size ?? canvas.Size);
            script.Draw();
            End(viewport);
        }
    }

    private void End(Box2? viewport)
    {
        if (viewport.HasValue)
        {
            var (origin, size) = ResolveViewport(
                canvas.Size,
                viewport.GetValueOrDefault());
            gl.Viewport(origin, size);
        }
        else gl.Viewport(canvas.Size);

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

    internal static (Vec2i Origin, Vec2u Size) ResolveViewport(
        Vec2 canvasSize,
        Box2 viewport) =>
        (
            (
                (int)MathF.Round(viewport.Min.X),
                (int)MathF.Round(canvasSize.Y - viewport.Max.Y)),
            (
                (uint)Math.Max(0, (int)MathF.Round(viewport.Size.X)),
                (uint)Math.Max(0, (int)MathF.Round(viewport.Size.Y))));
}
