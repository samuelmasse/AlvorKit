namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindRenderbuffer"/>.</remarks>
    public override void BindRenderbuffer(GlRenderbufferTarget target, GlRenderbufferHandle renderbuffer)
    {
        this.renderbuffer.Bind(nameof(BindRenderbuffer), (uint)renderbuffer);
        base.BindRenderbuffer(target, renderbuffer);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindFramebuffer"/> for the same target.</remarks>
    public override void BindFramebuffer(GlFramebufferTarget target, GlFramebufferHandle framebuffer)
    {
        var id = (uint)framebuffer;
        ValidateFramebufferChange(nameof(BindFramebuffer), target, id, false);
        if (target is GlFramebufferTarget.Framebuffer or GlFramebufferTarget.ReadFramebuffer)
            readFramebuffer.Bind(nameof(BindFramebuffer), id);
        if (target is GlFramebufferTarget.Framebuffer or GlFramebufferTarget.DrawFramebuffer)
            drawFramebuffer.Bind(nameof(BindFramebuffer), id);
        base.BindFramebuffer(target, framebuffer);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetDrawBuffers"/>.
    /// Set-once; cannot be combined with <see cref="DrawBuffers"/>.
    /// </remarks>
    public override void DrawBuffer(GlDrawBufferMode buf)
    {
        drawBuffersCount.Set(nameof(DrawBuffer), 1);
        base.DrawBuffer(buf);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetDrawBuffers"/>.
    /// Set-once; cannot be combined with <see cref="DrawBuffer"/>.
    /// </remarks>
    public override void DrawBuffers(int n, nint bufs)
    {
        drawBuffersCount.Set(nameof(DrawBuffers), n);
        base.DrawBuffers(n, bufs);
    }

    /// <summary>
    /// Layer: Unbinds <c>glBindRenderbuffer</c> for <paramref name="target"/>.
    /// Must be paired with exactly one earlier call to <c>glBindRenderbuffer</c>.
    /// </summary>
    public void UnbindRenderbuffer(GlRenderbufferTarget target)
    {
        renderbuffer.Unbind(nameof(BindRenderbuffer));
        base.BindRenderbuffer(target, (GlRenderbufferHandle)0u);
    }

    /// <summary>
    /// Layer: Returns <paramref name="target"/> to the default framebuffer.
    /// Must be paired with exactly one earlier call to <c>glBindFramebuffer</c> for the same target.
    /// </summary>
    public void UnbindFramebuffer(GlFramebufferTarget target)
    {
        ValidateFramebufferChange(nameof(BindFramebuffer), target, 0, true);
        if (target is GlFramebufferTarget.Framebuffer or GlFramebufferTarget.ReadFramebuffer)
            readFramebuffer.Unbind(nameof(BindFramebuffer));
        if (target is GlFramebufferTarget.Framebuffer or GlFramebufferTarget.DrawFramebuffer)
            drawFramebuffer.Unbind(nameof(BindFramebuffer));
        base.BindFramebuffer(target, (GlFramebufferHandle)0u);
    }

    /// <summary>
    /// Layer: Restores the default draw buffer (<c>glDrawBuffer(ColorAttachment0)</c>).
    /// Must be paired with one earlier call to <see cref="DrawBuffer"/> or <see cref="DrawBuffers"/>.
    /// </summary>
    public void ResetDrawBuffers()
    {
        drawBuffersCount.Reset(nameof(DrawBuffer));
        base.DrawBuffer(GlDrawBufferMode.ColorAttachment0);
    }

    /// <summary>
    /// Ensures framebuffer binding changes do not cross live framebuffer-specific state.
    /// </summary>
    /// <param name="function">The GL function that requested the framebuffer change.</param>
    /// <param name="target">The framebuffer target being changed.</param>
    /// <param name="framebuffer">The framebuffer id being bound, or zero for an unbind.</param>
    /// <param name="unbind">Whether the action is an unbind.</param>
    private void ValidateFramebufferChange(string function, GlFramebufferTarget target, uint framebuffer, bool unbind)
    {
        if (target != GlFramebufferTarget.ReadFramebuffer && drawBuffersCount.IsSet)
            throw new GlBindConflictException(function, $"attempted to {FramebufferActionName(framebuffer, unbind)}, but draw buffers are still set.");
        if (target != GlFramebufferTarget.ReadFramebuffer && viewport.IsSet)
            throw new GlBindConflictException(function, $"attempted to {FramebufferActionName(framebuffer, unbind)}, but viewport is still set.");
        if (target != GlFramebufferTarget.DrawFramebuffer && readBuffer.IsSet)
            throw new GlBindConflictException(function, $"attempted to {FramebufferActionName(framebuffer, unbind)}, but read buffer is still set.");
    }

    /// <summary>
    /// Formats a framebuffer action name for strict-bind diagnostics.
    /// </summary>
    /// <param name="framebuffer">The framebuffer id involved in the action.</param>
    /// <param name="unbind">Whether the action is an unbind.</param>
    /// <returns>The user-facing action name.</returns>
    private static string FramebufferActionName(uint framebuffer, bool unbind) => unbind ? "unbind framebuffer" : $"bind framebuffer {framebuffer}";
}
