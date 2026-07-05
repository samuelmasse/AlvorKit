namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>Layer: validates the clear prerequisites set by this layer before forwarding to <c>glClear</c>.</remarks>
    public override void Clear(GlClearBufferMask mask)
    {
        ValidateClear(mask);
        base.Clear(mask);
    }

    /// <summary>
    /// Validates the strict state required before clearing the requested buffers.
    /// </summary>
    /// <param name="mask">The clear buffer mask requested by the caller.</param>
    private void ValidateClear(GlClearBufferMask mask)
    {
        if ((mask & GlClearBufferMask.ColorBufferBit) != 0)
            ValidateClearColorBuffer();
        if ((mask & GlClearBufferMask.DepthBufferBit) != 0 && !state.clearDepth.IsSet)
            throw new GlMissingPrerequisiteException(nameof(Clear), "cannot clear depth buffer because clear depth is not set.");
        if ((mask & GlClearBufferMask.StencilBufferBit) != 0 && !state.clearStencil.IsSet)
            throw new GlMissingPrerequisiteException(nameof(Clear), "cannot clear stencil buffer because clear stencil is not set.");
        if (state.enableMap.IsSet(GlEnableCap.ScissorTest) && !state.scissor.IsSet)
            throw new GlMissingPrerequisiteException(nameof(Clear), "cannot clear while scissor test is enabled because no scissor box is set.");
        if (state.indexedEnableMap.IsSet((GlEnableCap.ScissorTest, 0)) && !state.scissorMap.HasAny)
            throw new GlMissingPrerequisiteException(nameof(Clear), "cannot clear while indexed scissor test is enabled because no indexed scissor box is set.");
    }

    /// <summary>
    /// Validates the strict state required before clearing the color buffer.
    /// </summary>
    private void ValidateClearColorBuffer()
    {
        if (!state.clearColor.IsSet)
            throw new GlMissingPrerequisiteException(nameof(Clear), "cannot clear color buffer because clear color is not set.");
        if (state.drawFramebuffer.Current != 0 && !state.drawBuffersCount.IsSet)
            throw new GlMissingPrerequisiteException(nameof(Clear), "cannot clear color buffer with a draw framebuffer bound because draw buffers are not set.");
    }
}
