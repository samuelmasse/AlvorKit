namespace AlvorKit;

/// <summary>Provides span- and vector-shaped sampling operations for managed FastNoise2 node handles.</summary>
/// <remarks>
/// Batch operations allocate no managed memory. Immutable node trees may be sampled concurrently into independent
/// buffers. Do not configure a graph during sampling, and do not overlap input and output spans.
/// Uniform-grid output is row-major with X as the innermost axis, followed by Y, Z, and W.
/// </remarks>
public static class FnGraphNodeSampling
{
    /// <summary>Generates a regular two-dimensional grid through native <c>fnGenUniformGrid2D</c>.</summary>
    /// <param name="node">A configured graph root.</param>
    /// <param name="output">Destination with at least <c>count.X * count.Y</c> elements.</param>
    /// <param name="offset">World position sampled at grid index (0, 0).</param>
    /// <param name="count">Positive sample counts. X is the innermost output axis.</param>
    /// <param name="step">World-space increment per sample; zero and negative values are allowed.</param>
    /// <param name="seed">The seed supplied to the entire graph.</param>
    /// <exception cref="ArgumentOutOfRangeException">A count is nonpositive or the count product exceeds Int32.</exception>
    /// <exception cref="ArgumentException"><paramref name="output"/> is too short.</exception>
    /// <exception cref="InvalidOperationException">The node is the default value.</exception>
    /// <remarks>Writes <c>output[y * count.X + x]</c>; extra destination elements are untouched.</remarks>
    public static void GenUniformGrid2D(
        this FnGraphNode node,
        Span<float> output,
        Vec2 offset,
        Vec2i count,
        Vec2 step,
        int seed)
    {
        var native = node.Borrow(out var fn);
        FnSampleValidation.Grid2(output, count, nameof(count));
        fn.GenUniformGrid2D(native, output, offset.X, offset.Y, count.X, count.Y, step.X, step.Y, seed);
        node.KeepAlive();
    }

    /// <summary>Generates a regular two-dimensional grid and reports its range.</summary>
    /// <param name="node">A configured graph root.</param>
    /// <param name="output">Destination with at least <c>count.X * count.Y</c> elements.</param>
    /// <param name="offset">World position sampled at grid index (0, 0).</param>
    /// <param name="count">Positive sample counts. X is the innermost output axis.</param>
    /// <param name="step">World-space increment per sample; zero and negative values are allowed.</param>
    /// <param name="seed">The seed supplied to the entire graph.</param>
    /// <param name="outputMinMax">
    /// Destination whose first two elements receive the minimum and maximum; later elements are untouched.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">A count is nonpositive or the count product exceeds Int32.</exception>
    /// <exception cref="ArgumentException">A destination is too short or the destinations overlap.</exception>
    /// <exception cref="InvalidOperationException">The node is the default value.</exception>
    /// <remarks>
    /// Range values cover exactly the written prefix and are not meaningful for <see cref="FnNodeType.ConvertRgba8"/>.
    /// </remarks>
    public static void GenUniformGrid2D(
        this FnGraphNode node,
        Span<float> output,
        Vec2 offset,
        Vec2i count,
        Vec2 step,
        int seed,
        Span<float> outputMinMax)
    {
        var native = node.Borrow(out var fn);
        FnSampleValidation.Grid2(output, count, nameof(count));
        FnSampleValidation.MinMax(output, outputMinMax);
        fn.GenUniformGrid2D(
            native, output, offset.X, offset.Y, count.X, count.Y, step.X, step.Y, seed, outputMinMax);
        node.KeepAlive();
    }

    /// <summary>Generates a regular three-dimensional grid through native <c>fnGenUniformGrid3D</c>.</summary>
    /// <param name="node">A configured graph root.</param>
    /// <param name="output">Destination with at least <c>count.X * count.Y * count.Z</c> elements.</param>
    /// <param name="offset">World position sampled at grid index (0, 0, 0).</param>
    /// <param name="count">Positive sample counts in X, Y, and Z.</param>
    /// <param name="step">World-space increment per sample; zero and negative values are allowed.</param>
    /// <param name="seed">The seed supplied to the entire graph.</param>
    /// <exception cref="ArgumentOutOfRangeException">A count is nonpositive or the count product exceeds Int32.</exception>
    /// <exception cref="ArgumentException"><paramref name="output"/> is too short.</exception>
    /// <exception cref="InvalidOperationException">The node is the default value.</exception>
    /// <remarks>
    /// Writes <c>output[(z * count.Y + y) * count.X + x]</c>. For a slice, prefer a singleton Y or Z axis over X.
    /// </remarks>
    public static void GenUniformGrid3D(
        this FnGraphNode node,
        Span<float> output,
        Vec3 offset,
        Vec3i count,
        Vec3 step,
        int seed)
    {
        var native = node.Borrow(out var fn);
        FnSampleValidation.Grid3(output, count);
        fn.GenUniformGrid3D(
            native, output, offset.X, offset.Y, offset.Z, count.X, count.Y, count.Z,
            step.X, step.Y, step.Z, seed);
        node.KeepAlive();
    }

    /// <summary>Generates a regular three-dimensional grid and reports its range.</summary>
    /// <param name="node">A configured graph root.</param>
    /// <param name="output">Destination with at least <c>count.X * count.Y * count.Z</c> elements.</param>
    /// <param name="offset">World position sampled at grid index (0, 0, 0).</param>
    /// <param name="count">Positive sample counts in X, Y, and Z.</param>
    /// <param name="step">World-space increment per sample; zero and negative values are allowed.</param>
    /// <param name="seed">The seed supplied to the entire graph.</param>
    /// <param name="outputMinMax">Destination for minimum at index 0 and maximum at index 1.</param>
    /// <exception cref="ArgumentOutOfRangeException">A count is nonpositive or the count product exceeds Int32.</exception>
    /// <exception cref="ArgumentException">A destination is too short or the destinations overlap.</exception>
    /// <exception cref="InvalidOperationException">The node is the default value.</exception>
    /// <remarks>Additional min/max elements are untouched; packed RGBA8 output has no meaningful numeric range.</remarks>
    public static void GenUniformGrid3D(
        this FnGraphNode node,
        Span<float> output,
        Vec3 offset,
        Vec3i count,
        Vec3 step,
        int seed,
        Span<float> outputMinMax)
    {
        var native = node.Borrow(out var fn);
        FnSampleValidation.Grid3(output, count);
        FnSampleValidation.MinMax(output, outputMinMax);
        fn.GenUniformGrid3D(
            native, output, offset.X, offset.Y, offset.Z, count.X, count.Y, count.Z,
            step.X, step.Y, step.Z, seed, outputMinMax);
        node.KeepAlive();
    }

    /// <summary>Generates a regular four-dimensional grid through native <c>fnGenUniformGrid4D</c>.</summary>
    /// <param name="node">A configured graph root.</param>
    /// <param name="output">Destination with at least <c>count.X * count.Y * count.Z * count.W</c> elements.</param>
    /// <param name="offset">World position sampled at grid index (0, 0, 0, 0).</param>
    /// <param name="count">Positive sample counts in X, Y, Z, and W.</param>
    /// <param name="step">World-space increment per sample; zero and negative values are allowed.</param>
    /// <param name="seed">The seed supplied to the entire graph.</param>
    /// <exception cref="ArgumentOutOfRangeException">A count is nonpositive or the count product exceeds Int32.</exception>
    /// <exception cref="ArgumentException"><paramref name="output"/> is too short.</exception>
    /// <exception cref="InvalidOperationException">The node is the default value.</exception>
    /// <remarks>
    /// Writes <c>output[((w * count.Z + z) * count.Y + y) * count.X + x]</c>. Avoid singleton X for slices.
    /// </remarks>
    public static void GenUniformGrid4D(
        this FnGraphNode node,
        Span<float> output,
        Vec4 offset,
        Vec4i count,
        Vec4 step,
        int seed)
    {
        var native = node.Borrow(out var fn);
        FnSampleValidation.Grid4(output, count);
        fn.GenUniformGrid4D(
            native, output, offset.X, offset.Y, offset.Z, offset.W, count.X, count.Y, count.Z, count.W,
            step.X, step.Y, step.Z, step.W, seed);
        node.KeepAlive();
    }

    /// <summary>Generates a regular four-dimensional grid and reports its range.</summary>
    /// <param name="node">A configured graph root.</param>
    /// <param name="output">Destination with at least <c>count.X * count.Y * count.Z * count.W</c> elements.</param>
    /// <param name="offset">World position sampled at grid index (0, 0, 0, 0).</param>
    /// <param name="count">Positive sample counts in X, Y, Z, and W.</param>
    /// <param name="step">World-space increment per sample; zero and negative values are allowed.</param>
    /// <param name="seed">The seed supplied to the entire graph.</param>
    /// <param name="outputMinMax">Destination for minimum at index 0 and maximum at index 1.</param>
    /// <exception cref="ArgumentOutOfRangeException">A count is nonpositive or the count product exceeds Int32.</exception>
    /// <exception cref="ArgumentException">A destination is too short or the destinations overlap.</exception>
    /// <exception cref="InvalidOperationException">The node is the default value.</exception>
    /// <remarks>Additional min/max elements are untouched; packed RGBA8 output has no meaningful numeric range.</remarks>
    public static void GenUniformGrid4D(
        this FnGraphNode node,
        Span<float> output,
        Vec4 offset,
        Vec4i count,
        Vec4 step,
        int seed,
        Span<float> outputMinMax)
    {
        var native = node.Borrow(out var fn);
        FnSampleValidation.Grid4(output, count);
        FnSampleValidation.MinMax(output, outputMinMax);
        fn.GenUniformGrid4D(
            native, output, offset.X, offset.Y, offset.Z, offset.W, count.X, count.Y, count.Z, count.W,
            step.X, step.Y, step.Z, step.W, seed, outputMinMax);
        node.KeepAlive();
    }

    /// <summary>Generates a seamless two-dimensional tile through native <c>fnGenTileable2D</c>.</summary>
    /// <param name="node">A configured root that supports meaningful four-dimensional evaluation.</param>
    /// <param name="output">Destination with at least <c>size.X * size.Y</c> elements.</param>
    /// <param name="size">Positive tile dimensions and period in samples.</param>
    /// <param name="step">World-space circumference scale for each tile axis.</param>
    /// <param name="seed">The seed supplied to the entire graph.</param>
    /// <exception cref="ArgumentOutOfRangeException">A size is nonpositive or the size product exceeds Int32.</exception>
    /// <exception cref="ArgumentException"><paramref name="output"/> is too short.</exception>
    /// <exception cref="InvalidOperationException">The node is the default value.</exception>
    /// <remarks>
    /// FastNoise2 maps the grid onto a four-dimensional hypertorus. Opposite edge samples are adjacent, not duplicates.
    /// Output is row-major with X innermost.
    /// </remarks>
    public static void GenTileable2D(
        this FnGraphNode node,
        Span<float> output,
        Vec2i size,
        Vec2 step,
        int seed)
    {
        var native = node.Borrow(out var fn);
        FnSampleValidation.Grid2(output, size, nameof(size));
        fn.GenTileable2D(native, output, size.X, size.Y, step.X, step.Y, seed);
        node.KeepAlive();
    }

    /// <summary>Generates a seamless two-dimensional tile and reports its range.</summary>
    /// <param name="node">A configured root that supports meaningful four-dimensional evaluation.</param>
    /// <param name="output">Destination with at least <c>size.X * size.Y</c> elements.</param>
    /// <param name="size">Positive tile dimensions and period in samples.</param>
    /// <param name="step">World-space circumference scale for each tile axis.</param>
    /// <param name="seed">The seed supplied to the entire graph.</param>
    /// <param name="outputMinMax">Destination for minimum at index 0 and maximum at index 1.</param>
    /// <exception cref="ArgumentOutOfRangeException">A size is nonpositive or the size product exceeds Int32.</exception>
    /// <exception cref="ArgumentException">A destination is too short or the destinations overlap.</exception>
    /// <exception cref="InvalidOperationException">The node is the default value.</exception>
    /// <remarks>Additional min/max elements are untouched; packed RGBA8 output has no meaningful numeric range.</remarks>
    public static void GenTileable2D(
        this FnGraphNode node,
        Span<float> output,
        Vec2i size,
        Vec2 step,
        int seed,
        Span<float> outputMinMax)
    {
        var native = node.Borrow(out var fn);
        FnSampleValidation.Grid2(output, size, nameof(size));
        FnSampleValidation.MinMax(output, outputMinMax);
        fn.GenTileable2D(native, output, size.X, size.Y, step.X, step.Y, seed, outputMinMax);
        node.KeepAlive();
    }

    /// <summary>Samples caller-owned two-dimensional position arrays through native <c>fnGenPositionArray2D</c>.</summary>
    /// <param name="node">A configured graph root.</param>
    /// <param name="output">Destination whose length is the sample count; every element is written.</param>
    /// <param name="x">X coordinates with at least <paramref name="output"/> length elements.</param>
    /// <param name="y">Y coordinates with at least <paramref name="output"/> length elements.</param>
    /// <param name="offset">Offset added to every caller-supplied position.</param>
    /// <param name="seed">The seed supplied to the entire graph.</param>
    /// <exception cref="ArgumentException">Output is empty; a position is too short; or input and output overlap.</exception>
    /// <exception cref="InvalidOperationException">The node is the default value.</exception>
    /// <remarks>Extra position elements are ignored. Reusing position arrays can avoid repeated coordinate construction.</remarks>
    public static void GenPositionArray2D(
        this FnGraphNode node,
        Span<float> output,
        ReadOnlySpan<float> x,
        ReadOnlySpan<float> y,
        Vec2 offset,
        int seed)
    {
        var native = node.Borrow(out var fn);
        FnSampleValidation.Positions2(output, x, y);
        fn.GenPositionArray2D(native, output, output.Length, x, y, offset.X, offset.Y, seed);
        node.KeepAlive();
    }

    /// <summary>Samples caller-owned three-dimensional position arrays through native <c>fnGenPositionArray3D</c>.</summary>
    /// <param name="node">A configured graph root.</param>
    /// <param name="output">Destination whose length is the sample count; every element is written.</param>
    /// <param name="x">X coordinates with at least <paramref name="output"/> length elements.</param>
    /// <param name="y">Y coordinates with at least <paramref name="output"/> length elements.</param>
    /// <param name="z">Z coordinates with at least <paramref name="output"/> length elements.</param>
    /// <param name="offset">Offset added to every caller-supplied position.</param>
    /// <param name="seed">The seed supplied to the entire graph.</param>
    /// <exception cref="ArgumentException">Output is empty; a position is too short; or input and output overlap.</exception>
    /// <exception cref="InvalidOperationException">The node is the default value.</exception>
    /// <remarks>Extra position elements are ignored. Reusing position arrays can avoid repeated coordinate construction.</remarks>
    public static void GenPositionArray3D(
        this FnGraphNode node,
        Span<float> output,
        ReadOnlySpan<float> x,
        ReadOnlySpan<float> y,
        ReadOnlySpan<float> z,
        Vec3 offset,
        int seed)
    {
        var native = node.Borrow(out var fn);
        FnSampleValidation.Positions3(output, x, y, z);
        fn.GenPositionArray3D(native, output, output.Length, x, y, z, offset.X, offset.Y, offset.Z, seed);
        node.KeepAlive();
    }

    /// <summary>Samples caller-owned four-dimensional position arrays through native <c>fnGenPositionArray4D</c>.</summary>
    /// <param name="node">A configured graph root.</param>
    /// <param name="output">Destination whose length is the sample count; every element is written.</param>
    /// <param name="x">X coordinates with at least <paramref name="output"/> length elements.</param>
    /// <param name="y">Y coordinates with at least <paramref name="output"/> length elements.</param>
    /// <param name="z">Z coordinates with at least <paramref name="output"/> length elements.</param>
    /// <param name="w">W coordinates with at least <paramref name="output"/> length elements.</param>
    /// <param name="offset">Offset added to every caller-supplied position.</param>
    /// <param name="seed">The seed supplied to the entire graph.</param>
    /// <exception cref="ArgumentException">Output is empty; a position is too short; or input and output overlap.</exception>
    /// <exception cref="InvalidOperationException">The node is the default value.</exception>
    /// <remarks>Extra position elements are ignored. Reusing position arrays can avoid repeated coordinate construction.</remarks>
    public static void GenPositionArray4D(
        this FnGraphNode node,
        Span<float> output,
        ReadOnlySpan<float> x,
        ReadOnlySpan<float> y,
        ReadOnlySpan<float> z,
        ReadOnlySpan<float> w,
        Vec4 offset,
        int seed)
    {
        var native = node.Borrow(out var fn);
        FnSampleValidation.Positions4(output, x, y, z, w);
        fn.GenPositionArray4D(
            native, output, output.Length, x, y, z, w, offset.X, offset.Y, offset.Z, offset.W, seed);
        node.KeepAlive();
    }

    /// <summary>Evaluates one two-dimensional position through native <c>fnGenSingle2D</c>.</summary>
    /// <param name="node">A configured graph root.</param>
    /// <param name="position">The world-space coordinate to evaluate.</param>
    /// <param name="seed">The seed supplied to the entire graph.</param>
    /// <returns>The generated scalar value.</returns>
    /// <exception cref="InvalidOperationException">The node is the default value.</exception>
    /// <remarks>Use a batch API for multiple samples; isolated calls substantially underutilize SIMD lanes.</remarks>
    public static float GenSingle2D(this FnGraphNode node, Vec2 position, int seed)
    {
        var native = node.Borrow(out var fn);
        var result = fn.GenSingle2D(native, position.X, position.Y, seed);
        node.KeepAlive();
        return result;
    }

    /// <summary>Evaluates one three-dimensional position through native <c>fnGenSingle3D</c>.</summary>
    /// <param name="node">A configured graph root.</param>
    /// <param name="position">The world-space coordinate to evaluate.</param>
    /// <param name="seed">The seed supplied to the entire graph.</param>
    /// <returns>The generated scalar value.</returns>
    /// <exception cref="InvalidOperationException">The node is the default value.</exception>
    /// <remarks>Use a batch API for multiple samples; isolated calls substantially underutilize SIMD lanes.</remarks>
    public static float GenSingle3D(this FnGraphNode node, Vec3 position, int seed)
    {
        var native = node.Borrow(out var fn);
        var result = fn.GenSingle3D(native, position.X, position.Y, position.Z, seed);
        node.KeepAlive();
        return result;
    }

    /// <summary>Evaluates one four-dimensional position through native <c>fnGenSingle4D</c>.</summary>
    /// <param name="node">A configured graph root.</param>
    /// <param name="position">The world-space coordinate to evaluate.</param>
    /// <param name="seed">The seed supplied to the entire graph.</param>
    /// <returns>The generated scalar value.</returns>
    /// <exception cref="InvalidOperationException">The node is the default value.</exception>
    /// <remarks>Use a batch API for multiple samples; isolated calls substantially underutilize SIMD lanes.</remarks>
    public static float GenSingle4D(this FnGraphNode node, Vec4 position, int seed)
    {
        var native = node.Borrow(out var fn);
        var result = fn.GenSingle4D(native, position.X, position.Y, position.Z, position.W, seed);
        node.KeepAlive();
        return result;
    }
}
