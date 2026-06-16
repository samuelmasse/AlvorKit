namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetDrawBuffers"/>.
    /// Set-once; cannot be combined with <see cref="DrawBuffer"/>.
    /// </remarks>
    public override void DrawBuffers(ReadOnlySpan<GlDrawBufferMode> bufs) => base.DrawBuffers(bufs);
}
