namespace AlvorKit.OpenGL.Layer.Test;

[TestClass]
public class GlLayerMemoryTest
{
    private GlLayer gl = null!;

    [TestInitialize]
    public void Setup() => gl = new GlLayer(new RecordingGl());

    /// <summary>Buffer storage assigned through a bound target updates total and per-buffer usage.</summary>
    [TestMethod]
    public void BufferData_TracksBoundBufferSize()
    {
        var buffer = gl.GenBuffer();
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, buffer);
        gl.BufferData(GlBufferTarget.ArrayBuffer, 1024, 0, GlBufferUsage.StaticDraw);
        Assert.AreEqual(1024L, gl.BufferUsage);
        Assert.AreEqual(1024L, gl.BufferSizes[buffer]);
    }

    /// <summary>Reallocating a buffer replaces the previous byte size instead of accumulating it.</summary>
    [TestMethod]
    public void BufferData_Reallocate_UpdatesByDelta()
    {
        var buffer = gl.GenBuffer();
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, buffer);
        gl.BufferData(GlBufferTarget.ArrayBuffer, 1024, 0, GlBufferUsage.StaticDraw);
        gl.BufferData(GlBufferTarget.ArrayBuffer, 256, 0, GlBufferUsage.StaticDraw);
        Assert.AreEqual(256L, gl.BufferUsage);
    }

    /// <summary>Buffer storage calls require a live buffer bound to the requested target.</summary>
    [TestMethod]
    public void BufferData_WhenNoBufferBound_Throws()
    {
        Assert.Throws<GlException>(() => gl.BufferData(GlBufferTarget.ArrayBuffer, 1024, 0, GlBufferUsage.StaticDraw));
    }

    /// <summary>Direct-state buffer storage updates memory accounting by handle.</summary>
    [TestMethod]
    public void NamedBufferData_TracksByHandle()
    {
        var buffer = gl.GenBuffer();
        gl.NamedBufferData(buffer, 2048, 0, GlBufferUsage.StaticDraw);
        Assert.AreEqual(2048L, gl.BufferUsage);
    }

    /// <summary>Indexed buffer binds also populate the generic target binding used by storage calls.</summary>
    [TestMethod]
    public void BufferData_AfterBindBufferBase_TracksGenericBinding()
    {
        var buffer = gl.GenBuffer();
        gl.BindBufferBase(GlBufferTarget.UniformBuffer, 0, buffer);
        gl.BufferData(GlBufferTarget.UniformBuffer, 1024, 0, GlBufferUsage.StaticDraw);
        Assert.AreEqual(1024L, gl.BufferSizes[buffer]);
    }

    /// <summary>Deleting an unbound buffer releases its tracked storage bytes.</summary>
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

    /// <summary>Texture image storage assigned through a bound target updates total and per-texture usage.</summary>
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

    /// <summary>Texture image levels accumulate into the texture's aggregate memory usage.</summary>
    [TestMethod]
    public void TexImage2D_MultipleLevels_AccumulatesUsage()
    {
        var texture = gl.GenTexture();
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, texture);

        gl.TexImage2D(GlTextureTarget.Texture2D, 0, GlInternalFormat.Rgba8, 16, 16, 0, GlPixelFormat.Rgba, GlPixelType.UnsignedByte, 0);
        gl.TexImage2D(GlTextureTarget.Texture2D, 1, GlInternalFormat.Rgba8, 8, 8, 0, GlPixelFormat.Rgba, GlPixelType.UnsignedByte, 0);

        Assert.AreEqual(16L * 16 * 4 + 8L * 8 * 4, gl.TextureUsage);
        Assert.AreEqual(gl.TextureUsage, gl.TextureSizes[texture].MemoryUsage);
        Assert.AreEqual(2, gl.TextureLevelSizes.Count);
    }

    /// <summary>Reallocating one texture image level updates usage by that level's size delta.</summary>
    [TestMethod]
    public void TexImage2D_ReallocateLevel_UpdatesByDelta()
    {
        var texture = gl.GenTexture();
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, texture);

        gl.TexImage2D(GlTextureTarget.Texture2D, 0, GlInternalFormat.Rgba8, 16, 16, 0, GlPixelFormat.Rgba, GlPixelType.UnsignedByte, 0);
        gl.TexImage2D(GlTextureTarget.Texture2D, 0, GlInternalFormat.Rgba8, 4, 4, 0, GlPixelFormat.Rgba, GlPixelType.UnsignedByte, 0);

        Assert.AreEqual(4L * 4 * 4, gl.TextureUsage);
        Assert.AreEqual(1, gl.TextureLevelSizes.Count);
    }

    /// <summary>Immutable texture storage accounts for the full mip chain requested by levels.</summary>
    [TestMethod]
    public void TextureStorage2D_TracksMipChainLevels()
    {
        var texture = gl.GenTexture();

        gl.TextureStorage2D(texture, 3, GlSizedInternalFormat.Rgba8, 8, 8);

        Assert.AreEqual(8L * 8 * 4 + 4L * 4 * 4 + 2L * 2 * 4, gl.TextureUsage);
        Assert.AreEqual(3, gl.TextureSizes[texture].Levels);
    }

    /// <summary>Bound storage for a 1D array mip chain only shrinks the width dimension.</summary>
    [TestMethod]
    public void TexStorage2D_Texture1DArray_ReducesOneMipDimension()
    {
        var texture = gl.GenTexture();
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture1DArray, texture);

        gl.TexStorage2D(GlTextureTarget.Texture1DArray, 3, GlSizedInternalFormat.R8, 8, 4);

        Assert.AreEqual(8L * 4 + 4L * 4 + 2L * 4, gl.TextureUsage);
    }

    /// <summary>Bound cube-map storage accounts for all six faces.</summary>
    [TestMethod]
    public void TexStorage2D_CubeMap_TracksSixFaces()
    {
        var texture = gl.GenTexture();
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.TextureCubeMap, texture);

        gl.TexStorage2D(GlTextureTarget.TextureCubeMap, 1, GlSizedInternalFormat.Rgba8, 4, 4);

        Assert.AreEqual(4L * 4 * 6 * 4, gl.TextureUsage);
    }

    /// <summary>Bound texture storage calls require a live texture bound to the requested target.</summary>
    [TestMethod]
    public void TexStorage2D_WhenNoTextureBound_Throws()
    {
        gl.ActiveTexture(GlTextureUnit.Texture0);
        Assert.Throws<GlException>(() =>
            gl.TexStorage2D(GlTextureTarget.Texture2D, 1, GlSizedInternalFormat.Rgba8, 4, 4));
    }

    /// <summary>Compressed image calls use the exact compressed image size supplied by the caller.</summary>
    [TestMethod]
    public void CompressedTexImage2D_UsesImageSize()
    {
        var texture = gl.GenTexture();
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, texture);

        gl.CompressedTexImage2D(GlTextureTarget.Texture2D, 0, GlInternalFormat.CompressedRedRgtc1, 8, 8, 0, 37, 0);

        Assert.AreEqual(37L, gl.TextureUsage);
        Assert.AreEqual(37L, gl.TextureSizes[texture].MemoryUsage);
    }

    /// <summary>Copied texture image storage updates memory accounting for the copied shape.</summary>
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

    /// <summary>Texture image calls require a live texture bound to the requested target.</summary>
    [TestMethod]
    public void TexImage2D_WhenNoTextureBound_Throws()
    {
        gl.ActiveTexture(GlTextureUnit.Texture0);
        Assert.Throws<GlException>(() =>
            gl.TexImage2D(GlTextureTarget.Texture2D, 0, GlInternalFormat.Rgba8, 16, 16, 0, GlPixelFormat.Rgba, GlPixelType.UnsignedByte, 0));
    }

    /// <summary>Texture image calls reject bound handles that were not generated or created through the layer.</summary>
    [TestMethod]
    public void TexImage2D_WhenTextureUntracked_Throws()
    {
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, (GlTextureHandle)999u);

        Assert.Throws<GlException>(() =>
            gl.TexImage2D(GlTextureTarget.Texture2D, 0, GlInternalFormat.Rgba8, 16, 16, 0, GlPixelFormat.Rgba, GlPixelType.UnsignedByte, 0));
        Assert.AreEqual(0L, gl.TextureUsage);
    }

    /// <summary>Texture image calls reject negative mip levels before recording storage.</summary>
    [TestMethod]
    public void TexImage2D_WhenLevelNegative_Throws()
    {
        var texture = gl.GenTexture();
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, texture);

        Assert.Throws<GlException>(() =>
            gl.TexImage2D(GlTextureTarget.Texture2D, -1, GlInternalFormat.Rgba8, 16, 16, 0, GlPixelFormat.Rgba, GlPixelType.UnsignedByte, 0));
        Assert.AreEqual(0L, gl.TextureUsage);
    }

    /// <summary>Texture image calls require the active texture unit to be explicitly set first.</summary>
    [TestMethod]
    public void TexImage2D_WhenNoActiveTexture_Throws()
    {
        Assert.Throws<GlMissingPrerequisiteException>(() =>
            gl.TexImage2D(GlTextureTarget.Texture2D, 0, GlInternalFormat.Rgba8, 16, 16, 0, GlPixelFormat.Rgba, GlPixelType.UnsignedByte, 0));
    }

    /// <summary>Deleting an unbound texture releases all tracked texture level storage.</summary>
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

    /// <summary>Renderbuffer storage assigned through a bound target updates renderbuffer usage.</summary>
    [TestMethod]
    public void RenderbufferStorage_TracksBoundRenderbufferSize()
    {
        var rb = gl.GenRenderbuffer();
        gl.BindRenderbuffer(GlRenderbufferTarget.Renderbuffer, rb);
        gl.RenderbufferStorage(GlRenderbufferTarget.Renderbuffer, GlInternalFormat.DepthComponent24, 16, 16);
        Assert.AreEqual(16L * 16 * 4, gl.RenderbufferUsage);
    }

    /// <summary>Direct-state texture storage updates memory accounting by handle.</summary>
    [TestMethod]
    public void TextureStorage2D_TracksByHandle()
    {
        var texture = gl.GenTexture();
        gl.TextureStorage2D(texture, 1, GlSizedInternalFormat.Rgba8, 8, 8);
        Assert.AreEqual(8L * 8 * 4, gl.TextureUsage);
    }

}
