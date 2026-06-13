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
        gl.BindBuffer(BufferTarget.ArrayBuffer, buffer);
        gl.BufferData(BufferTarget.ArrayBuffer, 1024, 0, BufferUsage.StaticDraw);
        Assert.AreEqual(1024L, gl.BufferUsage);
        Assert.AreEqual(1024L, gl.BufferSizes[buffer]);
    }

    [TestMethod]
    public void BufferData_Reallocate_UpdatesByDelta()
    {
        var buffer = gl.GenBuffer();
        gl.BindBuffer(BufferTarget.ArrayBuffer, buffer);
        gl.BufferData(BufferTarget.ArrayBuffer, 1024, 0, BufferUsage.StaticDraw);
        gl.BufferData(BufferTarget.ArrayBuffer, 256, 0, BufferUsage.StaticDraw);
        Assert.AreEqual(256L, gl.BufferUsage);
    }

    [TestMethod]
    public void BufferData_WhenNoBufferBound_Throws()
    {
        Assert.Throws<GlException>(() => gl.BufferData(BufferTarget.ArrayBuffer, 1024, 0, BufferUsage.StaticDraw));
    }

    [TestMethod]
    public void NamedBufferData_TracksByHandle()
    {
        var buffer = gl.GenBuffer();
        gl.NamedBufferData(buffer, 2048, 0, BufferUsage.StaticDraw);
        Assert.AreEqual(2048L, gl.BufferUsage);
    }

    [TestMethod]
    public void DeleteBuffer_ReleasesMemory()
    {
        var buffer = gl.GenBuffer();
        gl.BindBuffer(BufferTarget.ArrayBuffer, buffer);
        gl.BufferData(BufferTarget.ArrayBuffer, 1024, 0, BufferUsage.StaticDraw);
        gl.UnbindBuffer(BufferTarget.ArrayBuffer);
        gl.DeleteBuffer(buffer);
        Assert.AreEqual(0L, gl.BufferUsage);
    }

    [TestMethod]
    public void TexImage2D_TracksBoundTextureSize()
    {
        var texture = gl.GenTexture();
        gl.ActiveTexture(TextureUnit.Texture0);
        gl.BindTexture(TextureTarget.Texture2D, texture);
        gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, 16, 16, 0, PixelFormat.Rgba, PixelType.UnsignedByte, 0);
        Assert.AreEqual(16L * 16 * 4, gl.TextureUsage);
        Assert.AreEqual(16L * 16 * 4, gl.TextureSizes[texture].MemoryUsage);
    }

    [TestMethod]
    public void CopyTexImage2D_TracksBoundTextureSize()
    {
        var texture = gl.GenTexture();
        gl.ActiveTexture(TextureUnit.Texture0);
        gl.BindTexture(TextureTarget.Texture2D, texture);
        gl.CopyTexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, 0, 0, 8, 8, 0);
        Assert.AreEqual(8L * 8 * 4, gl.TextureUsage);
        Assert.AreEqual(8L * 8 * 4, gl.TextureSizes[texture].MemoryUsage);
    }

    [TestMethod]
    public void TexImage2D_WhenNoTextureBound_Throws()
    {
        gl.ActiveTexture(TextureUnit.Texture0);
        Assert.Throws<GlException>(() =>
            gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, 16, 16, 0, PixelFormat.Rgba, PixelType.UnsignedByte, 0));
    }

    [TestMethod]
    public void TexImage2D_WhenNoActiveTexture_Throws()
    {
        Assert.Throws<GlMissingPrerequisiteException>(() =>
            gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, 16, 16, 0, PixelFormat.Rgba, PixelType.UnsignedByte, 0));
    }

    [TestMethod]
    public void DeleteTexture_ReleasesMemory()
    {
        var texture = gl.GenTexture();
        gl.ActiveTexture(TextureUnit.Texture0);
        gl.BindTexture(TextureTarget.Texture2D, texture);
        gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, 16, 16, 0, PixelFormat.Rgba, PixelType.UnsignedByte, 0);
        gl.UnbindTexture(TextureTarget.Texture2D);
        gl.DeleteTexture(texture);
        Assert.AreEqual(0L, gl.TextureUsage);
    }

    [TestMethod]
    public void RenderbufferStorage_TracksBoundRenderbufferSize()
    {
        var rb = gl.GenRenderbuffer();
        gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rb);
        gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.DepthComponent24, 16, 16);
        Assert.AreEqual(16L * 16 * 4, gl.RenderbufferUsage);
    }

    [TestMethod]
    public void TextureStorage2D_TracksByHandle()
    {
        var texture = gl.GenTexture();
        gl.TextureStorage2D(texture, 1, SizedInternalFormat.Rgba8, 8, 8);
        Assert.AreEqual(8L * 8 * 4, gl.TextureUsage);
    }
}
