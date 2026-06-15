namespace AlvorKit.OpenGL.Layer.Test;

/// <summary>
/// Exercises buffer and renderbuffer memory accounting APIs.
/// </summary>
[TestClass]
public class GlLayerMemoryCoverageTestBuffersRenderbuffers
{
    private GlLayer gl = null!;

    [TestInitialize]
    public void Setup() => gl = new GlLayer(new RecordingGl());

    [TestMethod]
    public void BufferStorage_TracksBoundAndNamedBuffers()
    {
        var buffer = gl.GenBuffer();
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, buffer);
        gl.BufferStorage((GlBufferStorageTarget)(uint)GlBufferTarget.ArrayBuffer, 512, 0, 0);
        Assert.AreEqual(512L, gl.BufferSizes[buffer]);

        var named = gl.GenBuffer();
        gl.NamedBufferStorage(named, 256, 0, 0);
        Assert.AreEqual(768L, gl.BufferUsage);
        Assert.Throws<GlException>(() => gl.NamedBufferData((GlBufferHandle)999u, 1, 0, GlBufferUsage.StaticDraw));
    }

    [TestMethod]
    public void RenderbufferStorage_TracksBoundAndNamedRenderbuffers()
    {
        var renderbuffer = gl.GenRenderbuffer();
        gl.BindRenderbuffer(GlRenderbufferTarget.Renderbuffer, renderbuffer);
        gl.RenderbufferStorageMultisample(GlRenderbufferTarget.Renderbuffer, 4, GlInternalFormat.Rgba8, 4, 4);
        Assert.AreEqual(4L * 4 * 4 * 4, gl.RenderbufferSizes[renderbuffer].MemoryUsage);

        var named = gl.GenRenderbuffer();
        gl.NamedRenderbufferStorage(named, GlInternalFormat.DepthComponent16, 4, 4);
        gl.NamedRenderbufferStorageMultisample(named, 2, GlInternalFormat.DepthComponent16, 4, 4);
        Assert.IsTrue(gl.RenderbufferUsage > 0);

        gl.UnbindRenderbuffer(GlRenderbufferTarget.Renderbuffer);
        gl.DeleteRenderbuffer(renderbuffer);
        gl.DeleteRenderbuffer(named);
        Assert.AreEqual(0L, gl.RenderbufferUsage);
    }

    [TestMethod]
    public void RenderbufferStorage_WhenNoRenderbufferBoundOrTracked_Throws()
    {
        Assert.Throws<GlException>(() =>
            gl.RenderbufferStorage(GlRenderbufferTarget.Renderbuffer, GlInternalFormat.Rgba8, 4, 4));
        Assert.Throws<GlException>(() =>
            gl.NamedRenderbufferStorage((GlRenderbufferHandle)999u, GlInternalFormat.Rgba8, 4, 4));
    }
}
