namespace AlvorKit.OpenGL.Layer.Test;

/// <summary>
/// Exercises texture memory accounting APIs.
/// </summary>
[TestClass]
public class GlLayerMemoryCoverageTestTextures
{
    private GlLayer gl = null!;

    [TestInitialize]
    public void Setup()
    {
        gl = new GlLayer(new RecordingGl());
        gl.ActiveTexture(GlTextureUnit.Texture0);
    }

    [TestMethod]
    public void BoundTextureAllocations_TrackEveryAllocationShape()
    {
        WithBoundTexture(GlTextureTarget.Texture1D, target =>
            gl.TexImage1D(target, 0, GlInternalFormat.R8, 4, 0, GlPixelFormat.Red, GlPixelType.UnsignedByte, 0));
        WithBoundTexture(GlTextureTarget.Texture3D, target =>
            gl.TexImage3D(target, 0, GlInternalFormat.Rgba8, 2, 2, 2, 0, GlPixelFormat.Rgba, GlPixelType.UnsignedByte, 0));
        WithBoundTexture(GlTextureTarget.Texture2DMultisample, target =>
            gl.TexImage2DMultisample(target, 2, GlInternalFormat.Rgba8, 2, 2, false));
        WithBoundTexture(GlTextureTarget.Texture2DMultisampleArray, target =>
            gl.TexImage3DMultisample(target, 2, GlInternalFormat.Rgba8, 2, 2, 2, false));
        WithBoundTexture(GlTextureTarget.Texture1D, target =>
            gl.CompressedTexImage1D(target, 0, GlInternalFormat.CompressedRedRgtc1, 4, 0, 4, 0));
        WithBoundTexture(GlTextureTarget.Texture2D, target =>
            gl.CompressedTexImage2D(target, 0, GlInternalFormat.CompressedRedRgtc1, 2, 2, 0, 4, 0));
        WithBoundTexture(GlTextureTarget.Texture3D, target =>
            gl.CompressedTexImage3D(target, 0, GlInternalFormat.CompressedRedRgtc1, 2, 2, 2, 0, 8, 0));
        WithBoundTexture(GlTextureTarget.Texture1D, target =>
            gl.TexStorage1D(target, 1, GlSizedInternalFormat.R8, 4));
        WithBoundTexture(GlTextureTarget.Texture2D, target =>
            gl.TexStorage2D(target, 1, GlSizedInternalFormat.Rgba8, 2, 2));
        WithBoundTexture(GlTextureTarget.Texture3D, target =>
            gl.TexStorage3D(target, 1, GlSizedInternalFormat.Rgba8, 2, 2, 2));
        WithBoundTexture(GlTextureTarget.Texture2DMultisample, target =>
            gl.TexStorage2DMultisample(target, 2, GlSizedInternalFormat.Rgba8, 2, 2, false));
        WithBoundTexture(GlTextureTarget.Texture2DMultisampleArray, target =>
            gl.TexStorage3DMultisample(target, 2, GlSizedInternalFormat.Rgba8, 2, 2, 2, false));
        WithBoundTexture(GlTextureTarget.Texture1D, target =>
            gl.CopyTexImage1D(target, 0, GlInternalFormat.R8, 0, 0, 4, 0));
    }

    [TestMethod]
    public void DirectTextureAllocations_TrackEveryAllocationShape()
    {
        var one = gl.GenTexture();
        var three = gl.GenTexture();
        var multi = gl.GenTexture();

        gl.TextureStorage1D(one, 1, GlSizedInternalFormat.R8, 4);
        gl.TextureStorage3D(three, 1, GlSizedInternalFormat.Rgba8, 2, 2, 2);
        gl.TextureStorage2DMultisample(multi, 2, GlSizedInternalFormat.Rgba8, 2, 2, false);
        gl.TextureStorage3DMultisample(multi, 2, GlSizedInternalFormat.Rgba8, 2, 2, 2, false);

        Assert.Throws<GlException>(() =>
            gl.TextureStorage1D((GlTextureHandle)999u, 1, GlSizedInternalFormat.R8, 4));
    }

    private void WithBoundTexture(GlTextureTarget target, Action<GlTextureTarget> allocate)
    {
        var texture = gl.GenTexture();
        gl.BindTexture(target, texture);
        allocate(target);
        gl.UnbindTexture(target);
    }
}
