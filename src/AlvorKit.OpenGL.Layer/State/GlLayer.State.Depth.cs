namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetDepthFunc()"/>.</remarks>
    public override void DepthFunc(GlDepthFunction func) { state.depthFunc.Set(nameof(DepthFunc), func); base.DepthFunc(func); }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetDepthMask()"/>.</remarks>
    public override void DepthMask(bool flag) { state.depthMask.Set(nameof(DepthMask), flag); base.DepthMask(flag); }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetClearDepth()"/>.</remarks>
    public override void ClearDepth(double depth) { state.clearDepth.Set(nameof(ClearDepth), depth); base.ClearDepth(depth); }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetClearDepth()"/>.</remarks>
    public override void ClearDepthf(float depth) { state.clearDepth.Set(nameof(ClearDepthf), depth); base.ClearDepthf(depth); }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetClearStencil()"/>.</remarks>
    public override void ClearStencil(int s) { state.clearStencil.Set(nameof(ClearStencil), s); base.ClearStencil(s); }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetDepthRange()"/>.
    /// Cannot be combined with <c>glDepthRangeIndexed</c> or <c>glDepthRangeArrayv</c>.
    /// </remarks>
    public override void DepthRange(double n, double f)
    {
        if (state.depthRangeMap.HasAny) throw new GlConflictException(nameof(DepthRange), nameof(DepthRangeIndexed));
        state.depthRange.Set(nameof(DepthRange), (n, f));
        base.DepthRange(n, f);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetDepthRange()"/>.
    /// Cannot be combined with <c>glDepthRangeIndexed</c> or <c>glDepthRangeArrayv</c>.
    /// </remarks>
    public override void DepthRangef(float n, float f)
    {
        if (state.depthRangeMap.HasAny) throw new GlConflictException(nameof(DepthRangef), nameof(DepthRangeIndexed));
        state.depthRange.Set(nameof(DepthRangef), (n, f));
        base.DepthRangef(n, f);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetDepthRangeIndexed(uint)"/> for the same index.
    /// Cannot be combined with <c>glDepthRange</c>.
    /// </remarks>
    public override void DepthRangeIndexed(uint index, double n, double f)
    {
        if (state.depthRange.IsSet) throw new GlConflictException(nameof(DepthRangeIndexed), nameof(DepthRange));
        state.depthRangeMap.Set(nameof(DepthRangeIndexed), index, (n, f));
        base.DepthRangeIndexed(index, n, f);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetDepthRangeArray(uint, int)"/> for the same range.
    /// Cannot be combined with <c>glDepthRange</c>.
    /// </remarks>
    public override unsafe void DepthRangeArrayv(uint first, int count, nint v)
    {
        if (state.depthRange.IsSet) throw new GlConflictException(nameof(DepthRangeArrayv), nameof(DepthRange));
        var values = (double*)v;
        for (var i = 0; i < count; i++)
            state.depthRangeMap.RequireCanSet(nameof(DepthRangeArrayv), first + (uint)i, (values[i * 2], values[i * 2 + 1]));
        base.DepthRangeArrayv(first, count, v);
        for (var i = 0; i < count; i++)
            state.depthRangeMap.SetKnownUnset(first + (uint)i, (values[i * 2], values[i * 2 + 1]));
    }
}
