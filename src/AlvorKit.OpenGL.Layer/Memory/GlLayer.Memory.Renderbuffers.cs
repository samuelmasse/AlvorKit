namespace AlvorKit.OpenGL.Layer;

public unsafe partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the renderbuffer bound to <paramref name="target"/>.</remarks>
    public override void RenderbufferStorage(
        GlRenderbufferTarget target,
        GlInternalFormat internalformat,
        int width,
        int height)
    {
        TrackBoundRenderbufferSize(nameof(RenderbufferStorage), new(internalformat, width, height, 1));
        base.RenderbufferStorage(target, internalformat, width, height);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the renderbuffer bound to <paramref name="target"/>.</remarks>
    public override void RenderbufferStorageMultisample(
        GlRenderbufferTarget target,
        int samples,
        GlInternalFormat internalformat,
        int width,
        int height)
    {
        TrackBoundRenderbufferSize(nameof(RenderbufferStorageMultisample), new(internalformat, width, height, samples));
        base.RenderbufferStorageMultisample(target, samples, internalformat, width, height);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of renderbuffer <paramref name="renderbuffer"/>.</remarks>
    public override void NamedRenderbufferStorage(
        GlRenderbufferHandle renderbuffer,
        GlInternalFormat internalformat,
        int width,
        int height)
    {
        TrackRenderbufferSize(nameof(NamedRenderbufferStorage), renderbuffer, new(internalformat, width, height, 1));
        base.NamedRenderbufferStorage(renderbuffer, internalformat, width, height);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of renderbuffer <paramref name="renderbuffer"/>.</remarks>
    public override void NamedRenderbufferStorageMultisample(
        GlRenderbufferHandle renderbuffer,
        int samples,
        GlInternalFormat internalformat,
        int width,
        int height)
    {
        TrackRenderbufferSize(
            nameof(NamedRenderbufferStorageMultisample),
            renderbuffer,
            new(internalformat, width, height, samples));
        base.NamedRenderbufferStorageMultisample(renderbuffer, samples, internalformat, width, height);
    }

    /// <summary>
    /// Tracks storage bytes for the currently bound renderbuffer.
    /// </summary>
    /// <param name="function">The GL function that requested the accounting update.</param>
    /// <param name="info">The renderbuffer shape used for accounting.</param>
    private void TrackBoundRenderbufferSize(string function, GlRenderbufferInfo info)
    {
        var bound = renderbuffer.Current;
        if (bound == 0)
            throw new GlException(function, "cannot track renderbuffer size: no renderbuffer is bound.");
        TrackRenderbufferSize(function, (GlRenderbufferHandle)bound, info);
    }

    /// <summary>
    /// Tracks storage bytes for a specific live renderbuffer handle.
    /// </summary>
    /// <param name="function">The GL function that requested the accounting update.</param>
    /// <param name="id">The renderbuffer handle receiving the shape.</param>
    /// <param name="info">The renderbuffer shape used for accounting.</param>
    private void TrackRenderbufferSize(string function, GlRenderbufferHandle id, GlRenderbufferInfo info)
    {
        if (!renderbuffers.Contains(id))
            throw new GlException(function, $"cannot track renderbuffer size: renderbuffer {id} is not tracked.");
        renderbufferUsage += info.MemoryUsage - renderbufferSizes.GetValueOrDefault(id).MemoryUsage;
        renderbufferSizes[id] = info;
    }

    /// <summary>
    /// Releases any tracked storage bytes for a renderbuffer that is being deleted.
    /// </summary>
    /// <param name="id">The renderbuffer handle whose memory accounting should be released.</param>
    private void ReleaseRenderbufferMemory(GlRenderbufferHandle id)
    {
        if (renderbufferSizes.Remove(id, out var info))
            renderbufferUsage -= info.MemoryUsage;
    }
}
