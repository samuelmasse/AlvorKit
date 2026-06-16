namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetViewportArray(uint, int)"/> for the same range.
    /// Cannot be combined with <c>glViewport</c>.
    /// </remarks>
    public override void ViewportArrayv(uint first, int count, ReadOnlySpan<float> v) => base.ViewportArrayv(first, count, v);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetScissorArray(uint, int)"/> for the same range.
    /// Cannot be combined with <c>glScissor</c>.
    /// </remarks>
    public override void ScissorArrayv(uint first, int count, ReadOnlySpan<int> v) => base.ScissorArrayv(first, count, v);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetDepthRangeArray(uint, int)"/> for the same range.
    /// Cannot be combined with <c>glDepthRange</c>.
    /// </remarks>
    public override void DepthRangeArrayv(uint first, int count, ReadOnlySpan<double> v) => base.DepthRangeArrayv(first, count, v);
}
