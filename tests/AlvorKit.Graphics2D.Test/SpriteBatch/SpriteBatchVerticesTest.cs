namespace AlvorKit.Graphics2D.Test;

/// <summary>Tests sectioning and texture-slot assignment for sprite batch vertices.</summary>
[TestClass]
public sealed class SpriteBatchVerticesTest
{
    /// <summary>Adding one vertex creates one section and assigns texture slot zero.</summary>
    [TestMethod]
    public void Add_OneValue_HasCorrectValue()
    {
        var (_, gl) = Graphics2DTestHarness.CreateLayer();
        var vertices = new SpriteBatchVertices(16);
        using var texture = new Texture(gl, (1u, 1u), GlTextureTarget.Texture2D);

        vertices.Add(texture, new SpriteBatchVertex { Position = (34f, 354f) });

        Assert.AreEqual(1, vertices.VertexCount);
        Assert.AreEqual(new Vec2(34f, 354f), vertices.Vertices[0].Position);
        Assert.AreEqual(0f, vertices.Vertices[0].TexIndex);
        Assert.AreEqual(1, vertices.Sections.Length);
        Assert.AreSame(texture, vertices.SectionTextures(0)[0]);
        Assert.AreEqual(1, vertices.Sections[0].TextureCount);
        Assert.AreEqual(1, vertices.Sections[0].Count);
    }

    /// <summary>Reset clears all vertices and section metadata.</summary>
    [TestMethod]
    public void Add_OneValueThenReset_HasNoValues()
    {
        var (_, gl) = Graphics2DTestHarness.CreateLayer();
        var vertices = new SpriteBatchVertices(16);
        using var texture = new Texture(gl, (1u, 1u), GlTextureTarget.Texture2D);
        vertices.Add(texture, new SpriteBatchVertex { Position = (34f, 354f) });

        vertices.Reset();

        Assert.AreEqual(0, vertices.VertexCount);
        Assert.AreEqual(0, vertices.Sections.Length);
    }

    /// <summary>Repeated vertices for the same texture reuse texture slot zero.</summary>
    [TestMethod]
    public void Add_SameValueMultipleTimes_HasCorrectValues()
    {
        var (_, gl) = Graphics2DTestHarness.CreateLayer();
        var vertices = new SpriteBatchVertices(16);
        using var texture = new Texture(gl, (1u, 1u), GlTextureTarget.Texture2D);
        var vertex = new SpriteBatchVertex { Position = (34f, 354f) };

        for (var i = 0; i < 64; i++)
            vertices.Add(texture, vertex);

        Assert.AreEqual(64, vertices.VertexCount);
        Assert.AreEqual(1, vertices.Sections.Length);
        Assert.AreEqual(1, vertices.Sections[0].TextureCount);
        Assert.AreEqual(64, vertices.Sections[0].Count);
        Assert.AreSame(texture, vertices.SectionTextures(0)[0]);
        for (var i = 0; i < 64; i++)
            Assert.AreEqual(0f, vertices.Vertices[i].TexIndex);
    }

    /// <summary>Different textures receive stable slots within one section.</summary>
    [TestMethod]
    public void Add_InterleavedTextures_AssignsCorrectSlots()
    {
        var (_, gl) = Graphics2DTestHarness.CreateLayer();
        var vertices = new SpriteBatchVertices(16);
        using var textureA = new Texture(gl, (1u, 1u), GlTextureTarget.Texture2D);
        using var textureB = new Texture(gl, (1u, 1u), GlTextureTarget.Texture2D);

        vertices.Add(textureA, new SpriteBatchVertex { Position = Vec2.Zero });
        vertices.Add(textureB, new SpriteBatchVertex { Position = Vec2.One });
        vertices.Add(textureA, new SpriteBatchVertex { Position = (2f, 2f) });
        vertices.Add(textureB, new SpriteBatchVertex { Position = (3f, 3f) });

        Assert.AreEqual(4, vertices.VertexCount);
        Assert.AreEqual(0f, vertices.Vertices[0].TexIndex);
        Assert.AreEqual(1f, vertices.Vertices[1].TexIndex);
        Assert.AreEqual(0f, vertices.Vertices[2].TexIndex);
        Assert.AreEqual(1f, vertices.Vertices[3].TexIndex);
    }

    /// <summary>Exceeding the available texture slots starts a new section with slot zero.</summary>
    [TestMethod]
    public void Add_ExceedingMaxSlots_CreatesNewSection()
    {
        var (_, gl) = Graphics2DTestHarness.CreateLayer();
        var vertices = new SpriteBatchVertices(2);
        using var textureA = new Texture(gl, (1u, 1u), GlTextureTarget.Texture2D);
        using var textureB = new Texture(gl, (1u, 1u), GlTextureTarget.Texture2D);
        using var textureC = new Texture(gl, (1u, 1u), GlTextureTarget.Texture2D);

        vertices.Add(textureA, new SpriteBatchVertex { Position = Vec2.Zero });
        vertices.Add(textureB, new SpriteBatchVertex { Position = Vec2.One });
        vertices.Add(textureC, new SpriteBatchVertex { Position = (2f, 2f) });

        Assert.AreEqual(2, vertices.Sections.Length);
        Assert.AreEqual(2, vertices.Sections[0].TextureCount);
        Assert.AreEqual(1, vertices.Sections[1].TextureCount);
        Assert.AreSame(textureC, vertices.SectionTextures(1)[0]);
        Assert.AreEqual(0f, vertices.Vertices[2].TexIndex);
    }
}
