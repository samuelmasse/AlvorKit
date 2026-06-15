namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetViewport()"/>.
    /// Cannot be combined with <c>glViewportIndexedf</c> or <c>glViewportArrayv</c>.
    /// </remarks>
    public override void Viewport(int x, int y, int width, int height)
    {
        if (viewportMap.HasAny) throw new GlConflictException(nameof(Viewport), nameof(ViewportIndexedf));
        viewport.Set(nameof(Viewport), (x, y, width, height));
        base.Viewport(x, y, width, height);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetViewportIndexed(uint)"/> for the same index.
    /// Cannot be combined with <c>glViewport</c>.
    /// </remarks>
    public override void ViewportIndexedf(uint index, float x, float y, float w, float h)
    {
        if (viewport.IsSet) throw new GlConflictException(nameof(ViewportIndexedf), nameof(Viewport));
        viewportMap.Set(nameof(ViewportIndexedf), index, (x, y, w, h));
        base.ViewportIndexedf(index, x, y, w, h);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetViewportArray(uint, int)"/> for the same range.
    /// Cannot be combined with <c>glViewport</c>.
    /// </remarks>
    public override unsafe void ViewportArrayv(uint first, int count, nint v)
    {
        if (viewport.IsSet) throw new GlConflictException(nameof(ViewportArrayv), nameof(Viewport));
        var values = (float*)v;
        for (var i = 0; i < count; i++)
            viewportMap.Set(nameof(ViewportArrayv), first + (uint)i, (values[i * 4], values[i * 4 + 1], values[i * 4 + 2], values[i * 4 + 3]));
        base.ViewportArrayv(first, count, v);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetScissor()"/>.
    /// Cannot be combined with <c>glScissorIndexed</c> or <c>glScissorArrayv</c>.
    /// </remarks>
    public override void Scissor(int x, int y, int width, int height)
    {
        if (scissorMap.HasAny) throw new GlConflictException(nameof(Scissor), nameof(ScissorIndexed));
        scissor.Set(nameof(Scissor), (x, y, width, height));
        base.Scissor(x, y, width, height);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetScissorIndexed(uint)"/> for the same index.
    /// Cannot be combined with <c>glScissor</c>.
    /// </remarks>
    public override void ScissorIndexed(uint index, int left, int bottom, int width, int height)
    {
        if (scissor.IsSet) throw new GlConflictException(nameof(ScissorIndexed), nameof(Scissor));
        scissorMap.Set(nameof(ScissorIndexed), index, (left, bottom, width, height));
        base.ScissorIndexed(index, left, bottom, width, height);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetScissorArray(uint, int)"/> for the same range.
    /// Cannot be combined with <c>glScissor</c>.
    /// </remarks>
    public override unsafe void ScissorArrayv(uint first, int count, nint v)
    {
        if (scissor.IsSet) throw new GlConflictException(nameof(ScissorArrayv), nameof(Scissor));
        var values = (int*)v;
        for (var i = 0; i < count; i++)
            scissorMap.Set(nameof(ScissorArrayv), first + (uint)i, (values[i * 4], values[i * 4 + 1], values[i * 4 + 2], values[i * 4 + 3]));
        base.ScissorArrayv(first, count, v);
    }
}
