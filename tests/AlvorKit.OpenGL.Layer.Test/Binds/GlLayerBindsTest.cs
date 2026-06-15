namespace AlvorKit.OpenGL.Layer.Test;

/// <summary>
/// Tests strict bind and unbind rules enforced by <see cref="GlLayer"/>.
/// </summary>
[TestClass]
public partial class GlLayerBindsTest
{
    private GlLayer gl = null!;

    [TestInitialize]
    public void Setup() => gl = new GlLayer(new GlNoop());

    private static GlBufferHandle Buffer(uint id) => (GlBufferHandle)id;

    private static GlFramebufferHandle Framebuffer(uint id) => (GlFramebufferHandle)id;

    private static GlProgramHandle Program(uint id) => (GlProgramHandle)id;

    private static GlQueryHandle Query(uint id) => (GlQueryHandle)id;

    private static GlSamplerHandle Sampler(uint id) => (GlSamplerHandle)id;

    private static GlTextureHandle Texture(uint id) => (GlTextureHandle)id;

    private static GlVertexArrayHandle VertexArray(uint id) => (GlVertexArrayHandle)id;

    private void TrackTextureTarget(GlTextureHandle texture, GlTextureTarget target)
    {
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(target, texture);
        gl.UnbindTexture(target);
        gl.ResetActiveTexture();
    }
}
