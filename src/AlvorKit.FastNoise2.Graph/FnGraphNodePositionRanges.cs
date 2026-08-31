namespace AlvorKit;

/// <summary>Provides position-array sampling overloads that also report each batch's minimum and maximum.</summary>
/// <remarks>
/// These extension methods complement <see cref="FnGraphNodeSampling"/>. They allocate no managed memory. Immutable
/// trees may be sampled concurrently into independent buffers; input, output, and min/max spans must not overlap.
/// </remarks>
public static class FnGraphNodePositionRanges
{
    /// <summary>Samples two-dimensional positions and reports the generated range.</summary>
    /// <param name="node">A live, complete graph root.</param>
    /// <param name="output">Destination whose length is the sample count; every element is written.</param>
    /// <param name="x">X coordinates with at least <paramref name="output"/> length elements.</param>
    /// <param name="y">Y coordinates with at least <paramref name="output"/> length elements.</param>
    /// <param name="offset">Offset added to every caller-supplied position.</param>
    /// <param name="seed">The seed supplied to the entire graph.</param>
    /// <param name="outputMinMax">Destination for minimum at index 0 and maximum at index 1.</param>
    /// <exception cref="ArgumentException">
    /// Output is empty; a position or range is too short; or any input and output storage overlaps.
    /// </exception>
    /// <exception cref="InvalidOperationException">The node is invalid, stale, foreign, or incomplete.</exception>
    /// <exception cref="ObjectDisposedException">The owning graph has been disposed.</exception>
    /// <remarks>
    /// Extra position and range elements are untouched. The range is not meaningful for packed RGBA8 output.
    /// </remarks>
    public static void GenPositionArray2D(
        this FnGraphNode node,
        Span<float> output,
        ReadOnlySpan<float> x,
        ReadOnlySpan<float> y,
        Vec2 offset,
        int seed,
        Span<float> outputMinMax)
    {
        var fn = node.Use();
        FnSampleValidation.Positions2(output, x, y);
        FnSampleValidation.MinMax(output, outputMinMax);
        FnSampleValidation.RequireNoOverlap(outputMinMax, x, nameof(x));
        FnSampleValidation.RequireNoOverlap(outputMinMax, y, nameof(y));
        fn.GenPositionArray2D(
            node.Native, output, output.Length, x, y, offset.X, offset.Y, seed, outputMinMax);
    }

    /// <summary>Samples three-dimensional positions and reports the generated range.</summary>
    /// <param name="node">A live, complete graph root.</param>
    /// <param name="output">Destination whose length is the sample count; every element is written.</param>
    /// <param name="x">X coordinates with at least <paramref name="output"/> length elements.</param>
    /// <param name="y">Y coordinates with at least <paramref name="output"/> length elements.</param>
    /// <param name="z">Z coordinates with at least <paramref name="output"/> length elements.</param>
    /// <param name="offset">Offset added to every caller-supplied position.</param>
    /// <param name="seed">The seed supplied to the entire graph.</param>
    /// <param name="outputMinMax">Destination for minimum at index 0 and maximum at index 1.</param>
    /// <exception cref="ArgumentException">
    /// Output is empty; a position or range is too short; or any input and output storage overlaps.
    /// </exception>
    /// <exception cref="InvalidOperationException">The node is invalid, stale, foreign, or incomplete.</exception>
    /// <exception cref="ObjectDisposedException">The owning graph has been disposed.</exception>
    /// <remarks>
    /// Extra position and range elements are untouched. The range is not meaningful for packed RGBA8 output.
    /// </remarks>
    public static void GenPositionArray3D(
        this FnGraphNode node,
        Span<float> output,
        ReadOnlySpan<float> x,
        ReadOnlySpan<float> y,
        ReadOnlySpan<float> z,
        Vec3 offset,
        int seed,
        Span<float> outputMinMax)
    {
        var fn = node.Use();
        FnSampleValidation.Positions3(output, x, y, z);
        FnSampleValidation.MinMax(output, outputMinMax);
        FnSampleValidation.RequireNoOverlap(outputMinMax, x, nameof(x));
        FnSampleValidation.RequireNoOverlap(outputMinMax, y, nameof(y));
        FnSampleValidation.RequireNoOverlap(outputMinMax, z, nameof(z));
        fn.GenPositionArray3D(
            node.Native, output, output.Length, x, y, z, offset.X, offset.Y, offset.Z, seed, outputMinMax);
    }

    /// <summary>Samples four-dimensional positions and reports the generated range.</summary>
    /// <param name="node">A live, complete graph root.</param>
    /// <param name="output">Destination whose length is the sample count; every element is written.</param>
    /// <param name="x">X coordinates with at least <paramref name="output"/> length elements.</param>
    /// <param name="y">Y coordinates with at least <paramref name="output"/> length elements.</param>
    /// <param name="z">Z coordinates with at least <paramref name="output"/> length elements.</param>
    /// <param name="w">W coordinates with at least <paramref name="output"/> length elements.</param>
    /// <param name="offset">Offset added to every caller-supplied position.</param>
    /// <param name="seed">The seed supplied to the entire graph.</param>
    /// <param name="outputMinMax">Destination for minimum at index 0 and maximum at index 1.</param>
    /// <exception cref="ArgumentException">
    /// Output is empty; a position or range is too short; or any input and output storage overlaps.
    /// </exception>
    /// <exception cref="InvalidOperationException">The node is invalid, stale, foreign, or incomplete.</exception>
    /// <exception cref="ObjectDisposedException">The owning graph has been disposed.</exception>
    /// <remarks>
    /// Extra position and range elements are untouched. The range is not meaningful for packed RGBA8 output.
    /// </remarks>
    public static void GenPositionArray4D(
        this FnGraphNode node,
        Span<float> output,
        ReadOnlySpan<float> x,
        ReadOnlySpan<float> y,
        ReadOnlySpan<float> z,
        ReadOnlySpan<float> w,
        Vec4 offset,
        int seed,
        Span<float> outputMinMax)
    {
        var fn = node.Use();
        FnSampleValidation.Positions4(output, x, y, z, w);
        FnSampleValidation.MinMax(output, outputMinMax);
        FnSampleValidation.RequireNoOverlap(outputMinMax, x, nameof(x));
        FnSampleValidation.RequireNoOverlap(outputMinMax, y, nameof(y));
        FnSampleValidation.RequireNoOverlap(outputMinMax, z, nameof(z));
        FnSampleValidation.RequireNoOverlap(outputMinMax, w, nameof(w));
        fn.GenPositionArray4D(
            node.Native, output, output.Length, x, y, z, w,
            offset.X, offset.Y, offset.Z, offset.W, seed, outputMinMax);
    }
}
