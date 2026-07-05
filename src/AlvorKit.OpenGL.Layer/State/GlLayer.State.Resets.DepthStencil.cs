namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <summary>
    /// Layer: Restores <c>glDepthFunc</c> to <see cref="DefaultDepthFunc"/>.
    /// Must be paired with exactly one earlier call to <c>glDepthFunc</c>.
    /// </summary>
    public void ResetDepthFunc()
    {
        state.depthFunc.Reset(nameof(DepthFunc));
        base.DepthFunc(DefaultDepthFunc);
    }

    /// <summary>
    /// Layer: Restores <c>glDepthMask</c> to <see cref="DefaultDepthMask"/>.
    /// Must be paired with exactly one earlier call to <c>glDepthMask</c>.
    /// </summary>
    public void ResetDepthMask()
    {
        state.depthMask.Reset(nameof(DepthMask));
        base.DepthMask(DefaultDepthMask);
    }

    /// <summary>
    /// Layer: Restores <c>glClearDepth</c> to <see cref="DefaultClearDepth"/>.
    /// Must be paired with exactly one earlier call to <c>glClearDepth</c> or <c>glClearDepthf</c>.
    /// </summary>
    public void ResetClearDepth()
    {
        state.clearDepth.Reset(nameof(ClearDepth));
        base.ClearDepth(DefaultClearDepth);
    }

    /// <summary>
    /// Layer: Restores <c>glDepthRange</c> to <see cref="DefaultDepthRange"/>.
    /// Must be paired with exactly one earlier call to <c>glDepthRange</c> or <c>glDepthRangef</c>.
    /// </summary>
    public void ResetDepthRange()
    {
        state.depthRange.Reset(nameof(DepthRange));
        base.DepthRange(DefaultDepthRange.Near, DefaultDepthRange.Far);
    }

    /// <summary>
    /// Layer: Restores <c>glDepthRangeIndexed</c> for viewport <paramref name="index"/>.
    /// Must be paired with exactly one earlier call to <c>glDepthRangeIndexed</c> for the same index.
    /// </summary>
    public void ResetDepthRangeIndexed(uint index)
    {
        state.depthRangeMap.Reset(nameof(DepthRangeIndexed), index);
        base.DepthRangeIndexed(index, DefaultDepthRange.Near, DefaultDepthRange.Far);
    }

    /// <summary>
    /// Layer: Restores <c>glDepthRangeArrayv</c> for <paramref name="count"/> viewports
    /// from <paramref name="first"/>. Must be paired with exactly one earlier call for the same range.
    /// </summary>
    public unsafe void ResetDepthRangeArray(uint first, int count)
    {
        for (var i = 0; i < count; i++)
            state.depthRangeMap.RequireCanReset(nameof(DepthRangeArrayv), first + (uint)i);
        Span<double> values = stackalloc double[count * 2];
        for (var i = 0; i < count; i++) { values[i * 2] = DefaultDepthRange.Near; values[i * 2 + 1] = DefaultDepthRange.Far; }
        fixed (double* pointer = values)
            base.DepthRangeArrayv(first, count, (nint)pointer);
        for (var i = 0; i < count; i++)
            state.depthRangeMap.ResetKnownSet(first + (uint)i);
    }

    /// <summary>
    /// Layer: Restores <c>glClearStencil</c> to <see cref="DefaultClearStencil"/>.
    /// Must be paired with exactly one earlier call to <c>glClearStencil</c>.
    /// </summary>
    public void ResetClearStencil()
    {
        state.clearStencil.Reset(nameof(ClearStencil));
        base.ClearStencil(DefaultClearStencil);
    }

    /// <summary>
    /// Layer: Restores <c>glStencilFunc</c> to <see cref="DefaultStencilFunc"/>.
    /// Must be paired with exactly one earlier call to <c>glStencilFunc</c>.
    /// </summary>
    public void ResetStencilFunc()
    {
        state.stencilFunc.Reset(nameof(StencilFunc));
        base.StencilFunc(DefaultStencilFunc.Func, DefaultStencilFunc.Ref, DefaultStencilFunc.Mask);
    }

    /// <summary>
    /// Layer: Restores <c>glStencilFuncSeparate</c> for <paramref name="face"/>.
    /// Must be paired with exactly one earlier call to <c>glStencilFuncSeparate</c> for the same face.
    /// </summary>
    public void ResetStencilFuncSeparate(GlTriangleFace face)
    {
        state.stencilFuncSeparateMap.Reset(nameof(StencilFuncSeparate), face);
        base.StencilFuncSeparate(face, DefaultStencilFunc.Func, DefaultStencilFunc.Ref, DefaultStencilFunc.Mask);
    }

    /// <summary>
    /// Layer: Restores <c>glStencilMask</c> to <see cref="DefaultStencilMask"/>.
    /// Must be paired with exactly one earlier call to <c>glStencilMask</c>.
    /// </summary>
    public void ResetStencilMask()
    {
        state.stencilMask.Reset(nameof(StencilMask));
        base.StencilMask(DefaultStencilMask);
    }

    /// <summary>
    /// Layer: Restores <c>glStencilMaskSeparate</c> for <paramref name="face"/>.
    /// Must be paired with exactly one earlier call to <c>glStencilMaskSeparate</c> for the same face.
    /// </summary>
    public void ResetStencilMaskSeparate(GlTriangleFace face)
    {
        state.stencilMaskSeparateMap.Reset(nameof(StencilMaskSeparate), face);
        base.StencilMaskSeparate(face, DefaultStencilMask);
    }

    /// <summary>
    /// Layer: Restores <c>glStencilOp</c> to <see cref="DefaultStencilOp"/>.
    /// Must be paired with exactly one earlier call to <c>glStencilOp</c>.
    /// </summary>
    public void ResetStencilOp()
    {
        state.stencilOp.Reset(nameof(StencilOp));
        base.StencilOp(DefaultStencilOp.Fail, DefaultStencilOp.ZFail, DefaultStencilOp.ZPass);
    }

    /// <summary>
    /// Layer: Restores <c>glStencilOpSeparate</c> for <paramref name="face"/>.
    /// Must be paired with exactly one earlier call to <c>glStencilOpSeparate</c> for the same face.
    /// </summary>
    public void ResetStencilOpSeparate(GlTriangleFace face)
    {
        state.stencilOpSeparateMap.Reset(nameof(StencilOpSeparate), face);
        base.StencilOpSeparate(face, DefaultStencilOp.Fail, DefaultStencilOp.ZFail, DefaultStencilOp.ZPass);
    }
}
