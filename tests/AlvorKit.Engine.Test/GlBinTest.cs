namespace AlvorKit.Engine.Test;

[TestClass]
public sealed class GlBinTest
{
    /// <summary>Bins drain deferred OpenGL deletes through the root GL layer and detach disposed children.</summary>
    [TestMethod]
    public void Empty_DeletesQueuedResourcesAndTracksChildren()
    {
        var backend = new RecordingGl();
        using var gl = new RootGl(backend);
        var root = new RootBin(gl);
        var child = new GlBin(gl, root);
        var texture = gl.GenTexture();
        var buffer = gl.GenBuffer();
        var renderbuffer = gl.GenRenderbuffer();
        var vertexArray = gl.GenVertexArray();
        var framebuffer = gl.GenFramebuffer();

        root.DeleteTexture(texture);
        root.DeleteBuffer(buffer);
        root.DeleteRenderbuffer(renderbuffer);
        root.DeleteVertexArray(vertexArray);
        root.DeleteFramebuffer(framebuffer);
        root.Empty();
        root.Empty();
        child.Dispose();
        root.Dispose();

        CollectionAssert.AreEqual(new[] { texture.Handle }, backend.DeletedTextures);
        CollectionAssert.AreEqual(new[] { buffer.Handle }, backend.DeletedBuffers);
        CollectionAssert.AreEqual(new[] { renderbuffer.Handle }, backend.DeletedRenderbuffers);
        CollectionAssert.AreEqual(new[] { vertexArray.Handle }, backend.DeletedVertexArrays);
        CollectionAssert.AreEqual(new[] { framebuffer.Handle }, backend.DeletedFramebuffers);
        Assert.AreEqual(0, root.Children.Length);
    }

    private unsafe sealed class RecordingGl : GlNoop
    {
        private uint next = 1;

        public List<uint> DeletedTextures { get; } = [];

        public List<uint> DeletedBuffers { get; } = [];

        public List<uint> DeletedRenderbuffers { get; } = [];

        public List<uint> DeletedVertexArrays { get; } = [];

        public List<uint> DeletedFramebuffers { get; } = [];

        public override void GenTextures(int n, nint textures)
        {
            var span = new Span<GlTextureHandle>((void*)textures, n);
            for (var i = 0; i < span.Length; i++)
                span[i] = new(next++);
        }

        public override void GenBuffers(int n, nint buffers)
        {
            var span = new Span<GlBufferHandle>((void*)buffers, n);
            for (var i = 0; i < span.Length; i++)
                span[i] = new(next++);
        }

        public override void GenRenderbuffers(int n, nint renderbuffers)
        {
            var span = new Span<GlRenderbufferHandle>((void*)renderbuffers, n);
            for (var i = 0; i < span.Length; i++)
                span[i] = new(next++);
        }

        public override void GenVertexArrays(int n, nint arrays)
        {
            var span = new Span<GlVertexArrayHandle>((void*)arrays, n);
            for (var i = 0; i < span.Length; i++)
                span[i] = new(next++);
        }

        public override void GenFramebuffers(int n, nint framebuffers)
        {
            var span = new Span<GlFramebufferHandle>((void*)framebuffers, n);
            for (var i = 0; i < span.Length; i++)
                span[i] = new(next++);
        }

        public override void DeleteTextures(int n, nint textures)
        {
            var span = new Span<GlTextureHandle>((void*)textures, n);
            foreach (var texture in span)
                DeletedTextures.Add(texture.Handle);
        }

        public override void DeleteBuffers(int n, nint buffers)
        {
            var span = new Span<GlBufferHandle>((void*)buffers, n);
            foreach (var buffer in span)
                DeletedBuffers.Add(buffer.Handle);
        }

        public override void DeleteRenderbuffers(int n, nint renderbuffers)
        {
            var span = new Span<GlRenderbufferHandle>((void*)renderbuffers, n);
            foreach (var renderbuffer in span)
                DeletedRenderbuffers.Add(renderbuffer.Handle);
        }

        public override void DeleteVertexArrays(int n, nint arrays)
        {
            var span = new Span<GlVertexArrayHandle>((void*)arrays, n);
            foreach (var array in span)
                DeletedVertexArrays.Add(array.Handle);
        }

        public override void DeleteFramebuffers(int n, nint framebuffers)
        {
            var span = new Span<GlFramebufferHandle>((void*)framebuffers, n);
            foreach (var framebuffer in span)
                DeletedFramebuffers.Add(framebuffer.Handle);
        }
    }
}
