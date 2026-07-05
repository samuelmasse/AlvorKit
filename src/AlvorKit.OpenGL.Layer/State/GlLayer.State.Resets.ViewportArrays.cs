namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <summary>
    /// Layer: Restores <c>glViewportArrayv</c> for <paramref name="count"/> viewports from
    /// <paramref name="first"/>. Must be paired with exactly one earlier call to
    /// <c>glViewportArrayv</c> for the same range.
    /// </summary>
    public unsafe void ResetViewportArray(uint first, int count)
    {
        for (var i = 0; i < count; i++)
            state.viewportMap.RequireCanReset(nameof(ViewportArrayv), first + (uint)i);
        Span<float> values = stackalloc float[count * 4];
        for (var i = 0; i < count; i++)
        {
            values[i * 4] = DefaultViewport.X;
            values[i * 4 + 1] = DefaultViewport.Y;
            values[i * 4 + 2] = DefaultViewport.Width;
            values[i * 4 + 3] = DefaultViewport.Height;
        }
        fixed (float* pointer = values)
            base.ViewportArrayv(first, count, (nint)pointer);
        for (var i = 0; i < count; i++)
            state.viewportMap.ResetKnownSet(first + (uint)i);
    }

    /// <summary>
    /// Layer: Restores <c>glScissorArrayv</c> for <paramref name="count"/> viewports from
    /// <paramref name="first"/>. Must be paired with exactly one earlier call to
    /// <c>glScissorArrayv</c> for the same range.
    /// </summary>
    public unsafe void ResetScissorArray(uint first, int count)
    {
        for (var i = 0; i < count; i++)
            state.scissorMap.RequireCanReset(nameof(ScissorArrayv), first + (uint)i);
        Span<int> values = stackalloc int[count * 4];
        for (var i = 0; i < count; i++)
        {
            values[i * 4] = DefaultScissor.X;
            values[i * 4 + 1] = DefaultScissor.Y;
            values[i * 4 + 2] = DefaultScissor.Width;
            values[i * 4 + 3] = DefaultScissor.Height;
        }
        fixed (int* pointer = values)
            base.ScissorArrayv(first, count, (nint)pointer);
        for (var i = 0; i < count; i++)
            state.scissorMap.ResetKnownSet(first + (uint)i);
    }
}
