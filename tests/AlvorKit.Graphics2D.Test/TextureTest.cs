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
        using var texture = new Texture(gl, "diffuse", new Vector2(12f, 24f), GlTextureTarget.Texture2D);

        Assert.AreNotEqual(default, texture.Id);
        Assert.AreEqual("diffuse", texture.Label);
        Assert.AreEqual(new Vector2(12f, 24f), texture.Size);
        Assert.AreEqual(GlTextureTarget.Texture2D, texture.Target);
    }

    /// <summary>Disposing an unbound texture deletes the tracked handle.</summary>
    [TestMethod]
    public void Dispose_WhenUnbound_DeletesTexture()
    {
        var (backend, gl) = Graphics2DTestHarness.CreateLayer();
        var texture = new Texture(gl, new Vector2(1f, 1f), GlTextureTarget.Texture2D);
        var id = (uint)texture.Id;

        texture.Dispose();

        CollectionAssert.Contains(backend.Deleted, id);
    }

    /// <summary>Unbinding a texture that was never bound surfaces the layer's strict state error.</summary>
    [TestMethod]
    public void Unbind_WhenNotBound_Throws()
    {
        var (_, gl) = Graphics2DTestHarness.CreateLayer();
        using var texture = new Texture(gl, new Vector2(1f, 1f), GlTextureTarget.Texture2D);

        Assert.Throws<GlNotBoundException>(() => texture.Unbind(GlTextureUnit.Texture0));
    }

    /// <summary>Binding over an existing live binding surfaces the layer's strict state error.</summary>
    [TestMethod]
    public void Bind_WhenAlreadyBound_Throws()
    {
        var (_, gl) = Graphics2DTestHarness.CreateLayer();
        var texture = new Texture(gl, new Vector2(1f, 1f), GlTextureTarget.Texture2D);

        texture.Bind(GlTextureUnit.Texture0);

        Assert.Throws<GlAlreadyBoundException>(() => texture.Bind(GlTextureUnit.Texture0));
    }

    /// <summary>A matched bind and unbind pair leaves strict layer state clean.</summary>
    [TestMethod]
    public void BindAndUnbind_Succeeds()
    {
        var (_, gl) = Graphics2DTestHarness.CreateLayer();
        using var texture = new Texture(gl, new Vector2(1f, 1f), GlTextureTarget.Texture2D);

        texture.Bind(GlTextureUnit.Texture0);
        texture.Unbind(GlTextureUnit.Texture0);
    }

    /// <summary>Texture2D supports repeated level-zero pixel uploads.</summary>
    [TestMethod]
    public void SetPixels_MultipleTimes_UploadsBothSpans()
    {
        var (backend, gl) = Graphics2DTestHarness.CreateLayer();
        using var texture = new Texture2D(gl, new Vector2(1f, 1f), GlTextureTarget.Texture2D);
        (byte, byte, byte, byte)[] first = [(1, 2, 3, 4)];
        (byte, byte, byte, byte)[] second = [(1, 2, 3, 4), (5, 6, 7, 8)];

        texture.Pixels = first;
        texture.Pixels = second;

        Assert.AreEqual(2, backend.TexImage2DCalls);
    }

    /// <summary>PixelsMipmap uploads pixels and then regenerates texture mipmaps.</summary>
    [TestMethod]
    public void SetPixelsMipmap_UploadsAndGeneratesMipmap()
    {
        var (backend, gl) = Graphics2DTestHarness.CreateLayer();
        using var texture = new Texture2D(gl, new Vector2(1f, 1f), GlTextureTarget.Texture2D);
        (byte, byte, byte, byte)[] pixels = [(0xFF, 0xFF, 0xFF, 0xFF)];

        texture.PixelsMipmap = pixels;

        Assert.AreEqual(1, backend.TexImage2DCalls);
        Assert.AreEqual(1, backend.GenerateMipmapCalls);
    }

    /// <summary>Texture parameter convenience properties forward to integer texture parameters.</summary>
    [TestMethod]
    public void FilterAndWrapProperties_SetTextureParameters()
    {
        var (backend, gl) = Graphics2DTestHarness.CreateLayer();
        using var texture = new Texture(gl, new Vector2(1f, 1f), GlTextureTarget.Texture2D)
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
        using var defaultTarget = new Texture2D(gl, "default", new Vector2(2f, 2f));
        using var explicitTarget = new Texture2D(gl, "explicit", new Vector2(3f, 3f), GlTextureTarget.Texture2D);

        Assert.AreEqual("default", defaultTarget.Label);
        Assert.AreEqual(GlTextureTarget.Texture2D, defaultTarget.Target);
        Assert.AreEqual("explicit", explicitTarget.Label);
        Assert.AreEqual(new Vector2(3f, 3f), explicitTarget.Size);
    }
}
