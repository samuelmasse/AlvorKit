namespace AlvorKit.Graphics2D.Test;

/// <summary>Tests sprite writer geometry, clipping, rotation, and line output.</summary>
[TestClass]
public sealed class SpriteBatchWriterTest
{
    /// <summary>Clipping trims both vertex positions and texture coordinates.</summary>
    [TestMethod]
    public void Draw_WithClip_ClipsVerticesAndTexCoords()
    {
        var fixture = CreateWriter();
        fixture.Writer.Clip = new SpriteBatchClip(10f, 10f, 30f, 30f);

        fixture.Writer.Draw(fixture.Texture, Vector2.Zero, new Vector2(40f, 40f));

        Assert.AreEqual(4, fixture.Vertices.VertexCount);
        Assert.AreEqual(1, fixture.Vertices.Sections.Length);
        Assert.AreSame(fixture.Texture, fixture.Vertices.SectionTextures(0)[0]);
        AssertVertex(fixture.Vertices.Vertices[0], new Vector2(-0.8f, 0.8f), new Vector2(0.25f, 0.75f));
        AssertVertex(fixture.Vertices.Vertices[1], new Vector2(-0.4f, 0.8f), new Vector2(0.75f, 0.75f));
        AssertVertex(fixture.Vertices.Vertices[2], new Vector2(-0.8f, 0.4f), new Vector2(0.25f, 0.25f));
        AssertVertex(fixture.Vertices.Vertices[3], new Vector2(-0.4f, 0.4f), new Vector2(0.75f, 0.25f));
    }

    /// <summary>Clipping outside the draw rectangle skips vertex generation.</summary>
    [TestMethod]
    public void Draw_WithClipOutsideDraw_SkipsVertices()
    {
        var fixture = CreateWriter();
        fixture.Writer.Clip = new SpriteBatchClip(50f, 50f, 60f, 60f);

        fixture.Writer.Draw(fixture.Texture, Vector2.Zero, new Vector2(40f, 40f));

        Assert.AreEqual(0, fixture.Vertices.VertexCount);
        Assert.AreEqual(0, fixture.Vertices.Sections.Length);
    }

    /// <summary>Clipping a horizontally flipped sprite preserves the flipped texture-coordinate mapping.</summary>
    [TestMethod]
    public void Draw_WithClipAndHorizontalFlip_ClipsFlippedTexCoords()
    {
        var fixture = CreateWriter();
        fixture.Writer.Clip = new SpriteBatchClip(0f, 0f, 20f, 40f);

        fixture.Writer.Draw(
            fixture.Texture,
            Vector2.Zero,
            new Vector2(40f, 40f),
            Vector2.Zero,
            new Vector2(40f, 40f),
            Vector4.One,
            SpriteBatchRotation.None,
            SpriteBatchFlip.Horizontal);

        AssertVertex(fixture.Vertices.Vertices[0], new Vector2(-1f, 1f), new Vector2(1f, 1f));
        AssertVertex(fixture.Vertices.Vertices[1], new Vector2(-0.6f, 1f), new Vector2(0.5f, 1f));
        AssertVertex(fixture.Vertices.Vertices[2], new Vector2(-1f, 0.2f), new Vector2(1f, 0f));
        AssertVertex(fixture.Vertices.Vertices[3], new Vector2(-0.6f, 0.2f), new Vector2(0.5f, 0f));
    }

    /// <summary>Right-angle rotation preserves texture-coordinate corner mapping.</summary>
    [TestMethod]
    public void Draw_WithRotation_PreservesTexCoordMapping()
    {
        var fixture = CreateWriter();

        fixture.Writer.Draw(
            fixture.Texture,
            Vector2.Zero,
            new Vector2(40f, 40f),
            Vector2.Zero,
            new Vector2(40f, 40f),
            Vector4.One,
            SpriteBatchRotation.Clockwise90,
            SpriteBatchFlip.None);

        AssertVertex(fixture.Vertices.Vertices[0], new Vector2(-1f, 1f), new Vector2(0f, 0f));
        AssertVertex(fixture.Vertices.Vertices[1], new Vector2(-0.2f, 1f), new Vector2(0f, 1f));
        AssertVertex(fixture.Vertices.Vertices[2], new Vector2(-1f, 0.2f), new Vector2(1f, 0f));
        AssertVertex(fixture.Vertices.Vertices[3], new Vector2(-0.2f, 0.2f), new Vector2(1f, 1f));
    }

    /// <summary>Zero or negative destination and source sizes skip vertex generation.</summary>
    [TestMethod]
    public void Draw_WithInvalidSizes_SkipsVertices()
    {
        var fixture = CreateWriter();

        fixture.Writer.Draw(fixture.Texture, Vector2.Zero, Vector2.Zero);
        fixture.Writer.Draw(fixture.Texture, Vector2.Zero, new Vector2(-1f, 1f));
        fixture.Writer.Draw(fixture.Texture, Vector2.Zero, Vector2.One, Vector2.Zero, Vector2.Zero);

        Assert.AreEqual(0, fixture.Vertices.VertexCount);
    }

    /// <summary>Line drawing emits one quad worth of vertices.</summary>
    [TestMethod]
    public void DrawLine_WithHorizontalLine_EmitsQuad()
    {
        var fixture = CreateWriter();

        fixture.Writer.DrawLine(Vector2.Zero, new Vector2(100f, 0f), 10f, Vector4.One);

        Assert.AreEqual(4, fixture.Vertices.VertexCount);
    }

    /// <summary>Convenience draw overloads forward to the full vertex-generation path.</summary>
    [TestMethod]
    public void Draw_ConvenienceOverloads_EmitQuads()
    {
        var fixture = CreateWriter();

        fixture.Writer.Draw(Vector2.Zero, Vector2.One, Vector4.One);
        fixture.Writer.Draw(fixture.Texture, Vector2.Zero);
        fixture.Writer.Draw(fixture.Texture, Vector2.Zero, Vector2.One, Vector4.One);
        fixture.Writer.Draw(fixture.Texture, Vector2.Zero, Vector2.One, Vector2.Zero, Vector2.One, Vector4.One);
        fixture.Writer.DrawLine(Vector2.Zero, Vector2.One);
        fixture.Writer.DrawLine(fixture.Texture, Vector2.Zero, Vector2.One);

        Assert.AreEqual(24, fixture.Vertices.VertexCount);
    }

    /// <summary>Creates a writer with a 100 by 100 canvas and a 40 by 40 texture.</summary>
    private static WriterFixture CreateWriter()
    {
        var (_, gl) = Graphics2DTestHarness.CreateLayer();
        var texture = new Texture(gl, new Vector2(40f, 40f), GlTextureTarget.Texture2D);
        var canvas = new SpriteBatchCanvas { Size = new Vector2(100f, 100f) };
        var vertices = new SpriteBatchVertices(16);
        return new WriterFixture(texture, vertices, new SpriteBatchWriter(texture, canvas, vertices));
    }

    /// <summary>Asserts the position, texture coordinate, and default color for one generated vertex.</summary>
    private static void AssertVertex(SpriteBatchVertex vertex, Vector2 position, Vector2 texCoord)
    {
        AssertVector(position, vertex.Position);
        AssertVector(texCoord, vertex.TexCoord);
        AssertVector4(Vector4.One, vertex.Color);
    }

    /// <summary>Asserts two vectors are equal within the writer's floating-point tolerance.</summary>
    private static void AssertVector(Vector2 expected, Vector2 actual)
    {
        Assert.AreEqual(expected.X, actual.X, 0.0001f);
        Assert.AreEqual(expected.Y, actual.Y, 0.0001f);
    }

    /// <summary>Asserts two four-component vectors are equal within the writer's floating-point tolerance.</summary>
    private static void AssertVector4(Vector4 expected, Vector4 actual)
    {
        Assert.AreEqual(expected.X, actual.X, 0.0001f);
        Assert.AreEqual(expected.Y, actual.Y, 0.0001f);
        Assert.AreEqual(expected.Z, actual.Z, 0.0001f);
        Assert.AreEqual(expected.W, actual.W, 0.0001f);
    }

    /// <summary>Shared writer test fixture.</summary>
    private sealed record WriterFixture(Texture Texture, SpriteBatchVertices Vertices, SpriteBatchWriter Writer);
}
