namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetDepthFunc()"/>.</remarks>
    public override void DepthFunc(GlDepthFunction func) { depthFunc.Set(nameof(DepthFunc), func); base.DepthFunc(func); }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetDepthMask()"/>.</remarks>
    public override void DepthMask(bool flag) { depthMask.Set(nameof(DepthMask), flag); base.DepthMask(flag); }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetClearDepth()"/>.</remarks>
    public override void ClearDepth(double depth) { clearDepth.Set(nameof(ClearDepth), depth); base.ClearDepth(depth); }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetClearDepth()"/>.</remarks>
    public override void ClearDepthf(float depth) { clearDepth.Set(nameof(ClearDepthf), depth); base.ClearDepthf(depth); }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetClearStencil()"/>.</remarks>
    public override void ClearStencil(int s) { clearStencil.Set(nameof(ClearStencil), s); base.ClearStencil(s); }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetDepthRange()"/>.
    /// Cannot be combined with <c>glDepthRangeIndexed</c> or <c>glDepthRangeArrayv</c>.
    /// </remarks>
    public override void DepthRange(double n, double f)
    {
        if (depthRangeMap.HasAny) throw new GlConflictException(nameof(DepthRange), nameof(DepthRangeIndexed));
        depthRange.Set(nameof(DepthRange), (n, f));
        base.DepthRange(n, f);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetDepthRange()"/>.
    /// Cannot be combined with <c>glDepthRangeIndexed</c> or <c>glDepthRangeArrayv</c>.
    /// </remarks>
    public override void DepthRangef(float n, float f)
    {
        if (depthRangeMap.HasAny) throw new GlConflictException(nameof(DepthRangef), nameof(DepthRangeIndexed));
        depthRange.Set(nameof(DepthRangef), (n, f));
        base.DepthRangef(n, f);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetDepthRangeIndexed(uint)"/> for the same index.
    /// Cannot be combined with <c>glDepthRange</c>.
    /// </remarks>
    public override void DepthRangeIndexed(uint index, double n, double f)
    {
        if (depthRange.IsSet) throw new GlConflictException(nameof(DepthRangeIndexed), nameof(DepthRange));
        depthRangeMap.Set(nameof(DepthRangeIndexed), index, (n, f));
        base.DepthRangeIndexed(index, n, f);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetDepthRangeArray(uint, int)"/> for the same range.
    /// Cannot be combined with <c>glDepthRange</c>.
    /// </remarks>
    public override unsafe void DepthRangeArrayv(uint first, int count, nint v)
    {
        if (depthRange.IsSet) throw new GlConflictException(nameof(DepthRangeArrayv), nameof(DepthRange));
        var values = (double*)v;
        for (var i = 0; i < count; i++)
            depthRangeMap.RequireCanSet(nameof(DepthRangeArrayv), first + (uint)i, (values[i * 2], values[i * 2 + 1]));
        base.DepthRangeArrayv(first, count, v);
        for (var i = 0; i < count; i++)
            depthRangeMap.SetKnownUnset(first + (uint)i, (values[i * 2], values[i * 2 + 1]));
    }
}
