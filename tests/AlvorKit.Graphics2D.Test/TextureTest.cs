namespace AlvorKit.Graphics2D.Test;

/// <summary>Tests texture ownership and strict bind behavior.</summary>
[TestClass]
public sealed class TextureTest
{
    /// <summary>Texture construction stores the generated handle, size, label, and target.</summary>
    [TestMethod]
    public void Constructor_SetsProperties()
    {
        var (_, gl) = Graphics2DTestHarness.CreateLayer();
        using var texture = new Texture(gl, "diffuse", (12u, 24u), GlTextureTarget.Texture2D);

        Assert.AreNotEqual(default, texture.Id);
        Assert.AreEqual("diffuse", texture.Label);
        Assert.AreEqual(new Vec2u(12u, 24u), texture.Size);
        Assert.AreEqual(GlTextureTarget.Texture2D, texture.Target);
    }

    /// <summary>Disposing an unbound texture deletes the tracked handle.</summary>
    [TestMethod]
    public void Dispose_WhenUnbound_DeletesTexture()
    {
        var (backend, gl) = Graphics2DTestHarness.CreateLayer();
        var texture = new Texture(gl, (1u, 1u), GlTextureTarget.Texture2D);
        var id = (uint)texture.Id;

        texture.Dispose();

        CollectionAssert.Contains(backend.Deleted, id);
    }

    /// <summary>Unbinding a texture that was never bound surfaces the layer's strict state error.</summary>
    [TestMethod]
    public void Unbind_WhenNotBound_Throws()
    {
        var (_, gl) = Graphics2DTestHarness.CreateLayer();
        using var texture = new Texture(gl, (1u, 1u), GlTextureTarget.Texture2D);

        Assert.Throws<GlNotBoundException>(() => texture.Unbind(GlTextureUnit.Texture0));
    }

    /// <summary>Binding over an existing live binding surfaces the layer's strict state error.</summary>
    [TestMethod]
    public void Bind_WhenAlreadyBound_Throws()
    {
        var (_, gl) = Graphics2DTestHarness.CreateLayer();
        var texture = new Texture(gl, (1u, 1u), GlTextureTarget.Texture2D);

        texture.Bind(GlTextureUnit.Texture0);

        Assert.Throws<GlAlreadyBoundException>(() => texture.Bind(GlTextureUnit.Texture0));
    }

    /// <summary>A matched bind and unbind pair leaves strict layer state clean.</summary>
    [TestMethod]
    public void BindAndUnbind_Succeeds()
    {
        var (_, gl) = Graphics2DTestHarness.CreateLayer();
        using var texture = new Texture(gl, (1u, 1u), GlTextureTarget.Texture2D);

        texture.Bind(GlTextureUnit.Texture0);
        texture.Unbind(GlTextureUnit.Texture0);
    }

    /// <summary>Texture2D supports repeated level-zero pixel uploads.</summary>
    [TestMethod]
    public void SetPixels_MultipleTimes_UploadsBothSpans()
    {
        var (backend, gl) = Graphics2DTestHarness.CreateLayer();
        using var texture = new Texture2D(gl, (1u, 1u), GlTextureTarget.Texture2D);
        Vec4u8[] first = [(1, 2, 3, 4)];
        Vec4u8[] second = [(5, 6, 7, 8)];

        texture.Pixels = first;
        texture.Pixels = second;

        Assert.AreEqual(2, backend.TexImage2DCalls);
        Assert.AreEqual(GlInternalFormat.Rgba8, backend.LastTexImage2DInternalFormat);
    }

    /// <summary>Generic byte uploads succeed when the span contains exactly one RGBA8 value per texture pixel.</summary>
    [TestMethod]
    public void TexImage2D_WithExactByteCount_UploadsPixels()
    {
        var (backend, gl) = Graphics2DTestHarness.CreateLayer();
        using var texture = new Texture2D(gl, (2u, 1u), GlTextureTarget.Texture2D);
        byte[] pixels = [1, 2, 3, 4, 5, 6, 7, 8];

        texture.TexImage2D(pixels);

        Assert.AreEqual(1, backend.TexImage2DCalls);
        Assert.AreEqual(GlInternalFormat.Rgba8, backend.LastTexImage2DInternalFormat);
    }

    /// <summary>Pixel uploads reject spans whose byte count does not match the texture size.</summary>
    [TestMethod]
    public void SetPixels_WithWrongByteCount_ThrowsBeforeUpload()
    {
        var (backend, gl) = Graphics2DTestHarness.CreateLayer();
        using var texture = new Texture2D(gl, (2u, 1u), GlTextureTarget.Texture2D);
        Vec4u8[] pixels = [(1, 2, 3, 4)];

        var exception = Assert.ThrowsException<ArgumentException>(() =>
        {
            texture.Pixels = pixels;
        });

        Assert.AreEqual("pixels", exception.ParamName);
        Assert.AreEqual(0, backend.TexImage2DCalls);
    }

    /// <summary>PixelsMipmap uploads pixels and then regenerates texture mipmaps.</summary>
    [TestMethod]
    public void SetPixelsMipmap_UploadsAndGeneratesMipmap()
    {
        var (backend, gl) = Graphics2DTestHarness.CreateLayer();
        using var texture = new Texture2D(gl, (1u, 1u), GlTextureTarget.Texture2D);
        Vec4u8[] pixels = [(0xFF, 0xFF, 0xFF, 0xFF)];

        texture.PixelsMipmap = pixels;

        Assert.AreEqual(1, backend.TexImage2DCalls);
        Assert.AreEqual(GlInternalFormat.Rgba8, backend.LastTexImage2DInternalFormat);
        Assert.AreEqual(1, backend.GenerateMipmapCalls);
    }

    /// <summary>Texture parameter convenience properties forward to integer texture parameters.</summary>
    [TestMethod]
    public void FilterAndWrapProperties_SetTextureParameters()
    {
        var (backend, gl) = Graphics2DTestHarness.CreateLayer();
        using var texture = new Texture(gl, (1u, 1u), GlTextureTarget.Texture2D)
        {
            MinFilter = GlTextureMinFilter.LinearMipmapLinear,
            MagFilter = GlTextureMagFilter.Linear,
            WrapS = GlTextureWrapMode.Repeat,
            WrapT = GlTextureWrapMode.ClampToEdge
        };

        Assert.AreEqual(4, backend.TexParameterCalls);
    }

    /// <summary>Labeled Texture2D constructors preserve labels and targets.</summary>
    [TestMethod]
    public void Texture2D_LabelConstructors_SetProperties()
    {
        var (_, gl) = Graphics2DTestHarness.CreateLayer();
        using var defaultTarget = new Texture2D(gl, "default", (2u, 2u));
        using var explicitTarget = new Texture2D(gl, "explicit", (3u, 3u), GlTextureTarget.Texture2D);

        Assert.AreEqual("default", defaultTarget.Label);
        Assert.AreEqual(GlTextureTarget.Texture2D, defaultTarget.Target);
        Assert.AreEqual("explicit", explicitTarget.Label);
        Assert.AreEqual(new Vec2u(3u, 3u), explicitTarget.Size);
    }
}
