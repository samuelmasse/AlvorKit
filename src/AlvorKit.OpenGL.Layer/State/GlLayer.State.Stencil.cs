namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetStencilFunc()"/>. Cannot be combined with <c>glStencilFuncSeparate</c>.</remarks>
    public override void StencilFunc(GlStencilFunction func, int @ref, uint mask)
    {
        if (state.stencilFuncSeparateMap.HasAny) throw new GlConflictException(nameof(StencilFunc), nameof(StencilFuncSeparate));
        state.stencilFunc.Set(nameof(StencilFunc), (func, @ref, mask));
        base.StencilFunc(func, @ref, mask);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetStencilFuncSeparate(GlTriangleFace)"/>
    /// for the same face. Cannot be combined with <c>glStencilFunc</c>.
    /// </remarks>
    public override void StencilFuncSeparate(GlTriangleFace face, GlStencilFunction func, int @ref, uint mask)
    {
        if (state.stencilFunc.IsSet) throw new GlConflictException(nameof(StencilFuncSeparate), nameof(StencilFunc));
        state.stencilFuncSeparateMap.Set(nameof(StencilFuncSeparate), face, (func, @ref, mask));
        base.StencilFuncSeparate(face, func, @ref, mask);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetStencilMask()"/>. Cannot be combined with <c>glStencilMaskSeparate</c>.</remarks>
    public override void StencilMask(uint mask)
    {
        if (state.stencilMaskSeparateMap.HasAny) throw new GlConflictException(nameof(StencilMask), nameof(StencilMaskSeparate));
        state.stencilMask.Set(nameof(StencilMask), mask);
        base.StencilMask(mask);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetStencilMaskSeparate(GlTriangleFace)"/>
    /// for the same face. Cannot be combined with <c>glStencilMask</c>.
    /// </remarks>
    public override void StencilMaskSeparate(GlTriangleFace face, uint mask)
    {
        if (state.stencilMask.IsSet) throw new GlConflictException(nameof(StencilMaskSeparate), nameof(StencilMask));
        state.stencilMaskSeparateMap.Set(nameof(StencilMaskSeparate), face, mask);
        base.StencilMaskSeparate(face, mask);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetStencilOp()"/>. Cannot be combined with <c>glStencilOpSeparate</c>.</remarks>
    public override void StencilOp(GlStencilOp fail, GlStencilOp zfail, GlStencilOp zpass)
    {
        if (state.stencilOpSeparateMap.HasAny) throw new GlConflictException(nameof(StencilOp), nameof(StencilOpSeparate));
        state.stencilOp.Set(nameof(StencilOp), (fail, zfail, zpass));
        base.StencilOp(fail, zfail, zpass);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetStencilOpSeparate(GlTriangleFace)"/>
    /// for the same face. Cannot be combined with <c>glStencilOp</c>.
    /// </remarks>
    public override void StencilOpSeparate(GlTriangleFace face, GlStencilOp sfail, GlStencilOp dpfail, GlStencilOp dppass)
    {
        if (state.stencilOp.IsSet) throw new GlConflictException(nameof(StencilOpSeparate), nameof(StencilOp));
        state.stencilOpSeparateMap.Set(nameof(StencilOpSeparate), face, (sfail, dpfail, dppass));
        base.StencilOpSeparate(face, sfail, dpfail, dppass);
    }
}
