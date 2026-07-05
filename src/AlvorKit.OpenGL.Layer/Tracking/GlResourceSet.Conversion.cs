namespace AlvorKit.OpenGL.Layer;

internal sealed unsafe partial class GlResourceSet<THandle> where THandle : unmanaged
{
    /// <summary>
    /// Wraps a raw object id in the tracked handle type without allocating a converter delegate.
    /// </summary>
    /// <param name="id">The raw OpenGL object id.</param>
    /// <returns>The typed handle.</returns>
    private static THandle FromId(uint id)
    {
        EnsureHandleSize();
        return *(THandle*)&id;
    }

    /// <summary>
    /// Unwraps a typed handle to its raw object id without allocating a converter delegate.
    /// </summary>
    /// <param name="handle">The typed handle.</param>
    /// <returns>The raw OpenGL object id.</returns>
    private static uint ToId(THandle handle)
    {
        EnsureHandleSize();
        return *(uint*)&handle;
    }

    /// <summary>
    /// Verifies that the tracked handle type matches the generated single-uint OpenGL handle layout.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the handle layout is not the expected single <see cref="uint"/>.</exception>
    private static void EnsureHandleSize()
    {
        if (sizeof(THandle) != sizeof(uint))
            throw new InvalidOperationException($"{typeof(THandle)} must be a single-uint OpenGL handle.");
    }

    /// <summary>
    /// Views a contiguous native array of raw ids as typed handles without copying.
    /// </summary>
    /// <param name="count">The number of handles in the native array.</param>
    /// <param name="handles">The native pointer to the first raw id.</param>
    /// <returns>A typed span over the native handle array.</returns>
    internal static ReadOnlySpan<THandle> NativeHandleSpan(int count, nint handles) => new((void*)handles, count);
}
