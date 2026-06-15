namespace AlvorKit.OpenGL.Layer.Test;

[TestClass]
public class GlLayerMemoryTest
{
    private GlLayer gl = null!;

    [TestInitialize]
    public void Setup() => gl = new GlLayer(new RecordingGl());

    [TestMethod]
    public void BufferData_TracksBoundBufferSize()
    {
        var buffer = gl.GenBuffer();
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, buffer);
        gl.BufferData(GlBufferTarget.ArrayBuffer, 1024, 0, GlBufferUsage.StaticDraw);
        Assert.AreEqual(1024L, gl.BufferUsage);
        Assert.AreEqual(1024L, gl.BufferSizes[buffer]);
    }

    [TestMethod]
    public void BufferData_Reallocate_UpdatesByDelta()
    {
        var buffer = gl.GenBuffer();
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, buffer);
        gl.BufferData(GlBufferTarget.ArrayBuffer, 1024, 0, GlBufferUsage.StaticDraw);
        gl.BufferData(GlBufferTarget.ArrayBuffer, 256, 0, GlBufferUsage.StaticDraw);
        Assert.AreEqual(256L, gl.BufferUsage);
    }

    [TestMethod]
    public void BufferData_WhenNoBufferBound_Throws()
    {
        Assert.Throws<GlException>(() => gl.BufferData(GlBufferTarget.ArrayBuffer, 1024, 0, GlBufferUsage.StaticDraw));
    }

    [TestMethod]
    public void NamedBufferData_TracksByHandle()
    {
        var buffer = gl.GenBuffer();
        gl.NamedBufferData(buffer, 2048, 0, GlBufferUsage.StaticDraw);
        Assert.AreEqual(2048L, gl.BufferUsage);
    }

    [TestMethod]
    public void BufferData_AfterBindBufferBase_TracksGenericBinding()
    {
        var buffer = gl.GenBuffer();
        gl.BindBufferBase(GlBufferTarget.UniformBuffer, 0, buffer);
        gl.BufferData(GlBufferTarget.UniformBuffer, 1024, 0, GlBufferUsage.StaticDraw);
        Assert.AreEqual(1024L, gl.BufferSizes[buffer]);
    }

    [TestMethod]
    public void DeleteBuffer_ReleasesMemory()
    {
        var buffer = gl.GenBuffer();
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, buffer);
        gl.BufferData(GlBufferTarget.ArrayBuffer, 1024, 0, GlBufferUsage.StaticDraw);
        gl.UnbindBuffer(GlBufferTarget.ArrayBuffer);
        gl.DeleteBuffer(buffer);
        Assert.AreEqual(0L, gl.BufferUsage);
    }

    [TestMethod]
    public void TexImage2D_TracksBoundTextureSize()
    {
        var texture = gl.GenTexture();
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, texture);
        gl.TexImage2D(GlTextureTarget.Texture2D, 0, GlInternalFormat.Rgba8, 16, 16, 0, GlPixelFormat.Rgba, GlPixelType.UnsignedByte, 0);
        Assert.AreEqual(16L * 16 * 4, gl.TextureUsage);
        Assert.AreEqual(16L * 16 * 4, gl.TextureSizes[texture].MemoryUsage);
    }

    [TestMethod]
    public void CopyTexImage2D_TracksBoundTextureSize()
    {
        var texture = gl.GenTexture();
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, texture);
        gl.CopyTexImage2D(GlTextureTarget.Texture2D, 0, GlInternalFormat.Rgba8, 0, 0, 8, 8, 0);
        Assert.AreEqual(8L * 8 * 4, gl.TextureUsage);
        Assert.AreEqual(8L * 8 * 4, gl.TextureSizes[texture].MemoryUsage);
    }

    [TestMethod]
    public void TexImage2D_WhenNoTextureBound_Throws()
    {
        gl.ActiveTexture(GlTextureUnit.Texture0);
        Assert.Throws<GlException>(() =>
            gl.TexImage2D(GlTextureTarget.Texture2D, 0, GlInternalFormat.Rgba8, 16, 16, 0, GlPixelFormat.Rgba, GlPixelType.UnsignedByte, 0));
    }

    [TestMethod]
    public void TexImage2D_WhenNoActiveTexture_Throws()
    {
        Assert.Throws<GlMissingPrerequisiteException>(() =>
            gl.TexImage2D(GlTextureTarget.Texture2D, 0, GlInternalFormat.Rgba8, 16, 16, 0, GlPixelFormat.Rgba, GlPixelType.UnsignedByte, 0));
    }

    [TestMethod]
    public void DeleteTexture_ReleasesMemory()
    {
        var texture = gl.GenTexture();
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, texture);
        gl.TexImage2D(GlTextureTarget.Texture2D, 0, GlInternalFormat.Rgba8, 16, 16, 0, GlPixelFormat.Rgba, GlPixelType.UnsignedByte, 0);
        gl.UnbindTexture(GlTextureTarget.Texture2D);
        gl.DeleteTexture(texture);
        Assert.AreEqual(0L, gl.TextureUsage);
    }

    [TestMethod]
    public void RenderbufferStorage_TracksBoundRenderbufferSize()
    {
        var rb = gl.GenRenderbuffer();
        gl.BindRenderbuffer(GlRenderbufferTarget.Renderbuffer, rb);
        gl.RenderbufferStorage(GlRenderbufferTarget.Renderbuffer, GlInternalFormat.DepthComponent24, 16, 16);
        Assert.AreEqual(16L * 16 * 4, gl.RenderbufferUsage);
    }

    [TestMethod]
    public void TextureStorage2D_TracksByHandle()
    {
        var texture = gl.GenTexture();
        gl.TextureStorage2D(texture, 1, GlSizedInternalFormat.Rgba8, 8, 8);
        Assert.AreEqual(8L * 8 * 4, gl.TextureUsage);
    }

}
