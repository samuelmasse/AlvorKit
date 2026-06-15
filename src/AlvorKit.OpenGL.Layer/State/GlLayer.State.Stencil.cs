namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetStencilFunc()"/>. Cannot be combined with <c>glStencilFuncSeparate</c>.</remarks>
    public override void StencilFunc(GlStencilFunction func, int @ref, uint mask)
    {
        if (stencilFuncSeparateMap.HasAny) throw new GlConflictException(nameof(StencilFunc), nameof(StencilFuncSeparate));
        stencilFunc.Set(nameof(StencilFunc), (func, @ref, mask));
        base.StencilFunc(func, @ref, mask);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetStencilFuncSeparate(GlTriangleFace)"/>
    /// for the same face. Cannot be combined with <c>glStencilFunc</c>.
    /// </remarks>
    public override void StencilFuncSeparate(GlTriangleFace face, GlStencilFunction func, int @ref, uint mask)
    {
        if (stencilFunc.IsSet) throw new GlConflictException(nameof(StencilFuncSeparate), nameof(StencilFunc));
        stencilFuncSeparateMap.Set(nameof(StencilFuncSeparate), face, (func, @ref, mask));
        base.StencilFuncSeparate(face, func, @ref, mask);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetStencilMask()"/>. Cannot be combined with <c>glStencilMaskSeparate</c>.</remarks>
    public override void StencilMask(uint mask)
    {
        if (stencilMaskSeparateMap.HasAny) throw new GlConflictException(nameof(StencilMask), nameof(StencilMaskSeparate));
        stencilMask.Set(nameof(StencilMask), mask);
        base.StencilMask(mask);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetStencilMaskSeparate(GlTriangleFace)"/>
    /// for the same face. Cannot be combined with <c>glStencilMask</c>.
    /// </remarks>
    public override void StencilMaskSeparate(GlTriangleFace face, uint mask)
    {
        if (stencilMask.IsSet) throw new GlConflictException(nameof(StencilMaskSeparate), nameof(StencilMask));
        stencilMaskSeparateMap.Set(nameof(StencilMaskSeparate), face, mask);
        base.StencilMaskSeparate(face, mask);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetStencilOp()"/>. Cannot be combined with <c>glStencilOpSeparate</c>.</remarks>
    public override void StencilOp(GlStencilOp fail, GlStencilOp zfail, GlStencilOp zpass)
    {
        if (stencilOpSeparateMap.HasAny) throw new GlConflictException(nameof(StencilOp), nameof(StencilOpSeparate));
        stencilOp.Set(nameof(StencilOp), (fail, zfail, zpass));
        base.StencilOp(fail, zfail, zpass);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetStencilOpSeparate(GlTriangleFace)"/>
    /// for the same face. Cannot be combined with <c>glStencilOp</c>.
    /// </remarks>
    public override void StencilOpSeparate(GlTriangleFace face, GlStencilOp sfail, GlStencilOp dpfail, GlStencilOp dppass)
    {
        if (stencilOp.IsSet) throw new GlConflictException(nameof(StencilOpSeparate), nameof(StencilOp));
        stencilOpSeparateMap.Set(nameof(StencilOpSeparate), face, (sfail, dpfail, dppass));
        base.StencilOpSeparate(face, sfail, dpfail, dppass);
    }
}
