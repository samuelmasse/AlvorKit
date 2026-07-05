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
        if (state.viewportMap.HasAny) throw new GlConflictException(nameof(Viewport), nameof(ViewportIndexedf));
        state.viewport.Set(nameof(Viewport), (x, y, width, height));
        base.Viewport(x, y, width, height);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetViewportIndexed(uint)"/> for the same index.
    /// Cannot be combined with <c>glViewport</c>.
    /// </remarks>
    public override void ViewportIndexedf(uint index, float x, float y, float w, float h)
    {
        if (state.viewport.IsSet) throw new GlConflictException(nameof(ViewportIndexedf), nameof(Viewport));
        state.viewportMap.Set(nameof(ViewportIndexedf), index, (x, y, w, h));
        base.ViewportIndexedf(index, x, y, w, h);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetViewportArray(uint, int)"/> for the same range.
    /// Cannot be combined with <c>glViewport</c>.
    /// </remarks>
    public override unsafe void ViewportArrayv(uint first, int count, nint v)
    {
        if (state.viewport.IsSet) throw new GlConflictException(nameof(ViewportArrayv), nameof(Viewport));
        var values = (float*)v;
        for (var i = 0; i < count; i++)
        {
            state.viewportMap.RequireCanSet(
                nameof(ViewportArrayv),
                first + (uint)i,
                (values[i * 4], values[i * 4 + 1], values[i * 4 + 2], values[i * 4 + 3]));
        }
        base.ViewportArrayv(first, count, v);
        for (var i = 0; i < count; i++)
            state.viewportMap.SetKnownUnset(first + (uint)i, (values[i * 4], values[i * 4 + 1], values[i * 4 + 2], values[i * 4 + 3]));
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetScissor()"/>.
    /// Cannot be combined with <c>glScissorIndexed</c> or <c>glScissorArrayv</c>.
    /// </remarks>
    public override void Scissor(int x, int y, int width, int height)
    {
        if (state.scissorMap.HasAny) throw new GlConflictException(nameof(Scissor), nameof(ScissorIndexed));
        state.scissor.Set(nameof(Scissor), (x, y, width, height));
        base.Scissor(x, y, width, height);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetScissorIndexed(uint)"/> for the same index.
    /// Cannot be combined with <c>glScissor</c>.
    /// </remarks>
    public override void ScissorIndexed(uint index, int left, int bottom, int width, int height)
    {
        if (state.scissor.IsSet) throw new GlConflictException(nameof(ScissorIndexed), nameof(Scissor));
        state.scissorMap.Set(nameof(ScissorIndexed), index, (left, bottom, width, height));
        base.ScissorIndexed(index, left, bottom, width, height);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetScissorArray(uint, int)"/> for the same range.
    /// Cannot be combined with <c>glScissor</c>.
    /// </remarks>
    public override unsafe void ScissorArrayv(uint first, int count, nint v)
    {
        if (state.scissor.IsSet) throw new GlConflictException(nameof(ScissorArrayv), nameof(Scissor));
        var values = (int*)v;
        for (var i = 0; i < count; i++)
        {
            state.scissorMap.RequireCanSet(
                nameof(ScissorArrayv),
                first + (uint)i,
                (values[i * 4], values[i * 4 + 1], values[i * 4 + 2], values[i * 4 + 3]));
        }
        base.ScissorArrayv(first, count, v);
        for (var i = 0; i < count; i++)
            state.scissorMap.SetKnownUnset(first + (uint)i, (values[i * 4], values[i * 4 + 1], values[i * 4 + 2], values[i * 4 + 3]));
    }
}
