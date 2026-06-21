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

        fixture.Writer.Draw(fixture.Texture, Vec2.Zero, new Vec2(40f, 40f));

        Assert.AreEqual(4, fixture.Vertices.VertexCount);
        Assert.AreEqual(1, fixture.Vertices.Sections.Length);
        Assert.AreSame(fixture.Texture, fixture.Vertices.SectionTextures(0)[0]);
        AssertVertex(fixture.Vertices.Vertices[0], new Vec2(-0.8f, 0.8f), new Vec2(0.25f, 0.75f));
        AssertVertex(fixture.Vertices.Vertices[1], new Vec2(-0.4f, 0.8f), new Vec2(0.75f, 0.75f));
        AssertVertex(fixture.Vertices.Vertices[2], new Vec2(-0.8f, 0.4f), new Vec2(0.25f, 0.25f));
        AssertVertex(fixture.Vertices.Vertices[3], new Vec2(-0.4f, 0.4f), new Vec2(0.75f, 0.25f));
    }

    /// <summary>Clipping outside the draw rectangle skips vertex generation.</summary>
    [TestMethod]
    public void Draw_WithClipOutsideDraw_SkipsVertices()
    {
        var fixture = CreateWriter();
        fixture.Writer.Clip = new SpriteBatchClip(50f, 50f, 60f, 60f);

        fixture.Writer.Draw(fixture.Texture, Vec2.Zero, new Vec2(40f, 40f));

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
            Vec2.Zero,
            new Vec2(40f, 40f),
            Vec2.Zero,
            new Vec2(40f, 40f),
            Vec4.One,
            SpriteBatchRotation.None,
            SpriteBatchFlip.Horizontal);

        AssertVertex(fixture.Vertices.Vertices[0], new Vec2(-1f, 1f), new Vec2(1f, 1f));
        AssertVertex(fixture.Vertices.Vertices[1], new Vec2(-0.6f, 1f), new Vec2(0.5f, 1f));
        AssertVertex(fixture.Vertices.Vertices[2], new Vec2(-1f, 0.2f), new Vec2(1f, 0f));
        AssertVertex(fixture.Vertices.Vertices[3], new Vec2(-0.6f, 0.2f), new Vec2(0.5f, 0f));
    }

    /// <summary>Right-angle rotation preserves texture-coordinate corner mapping.</summary>
    [TestMethod]
    public void Draw_WithRotation_PreservesTexCoordMapping()
    {
        var fixture = CreateWriter();

        fixture.Writer.Draw(
            fixture.Texture,
            Vec2.Zero,
            new Vec2(40f, 40f),
            Vec2.Zero,
            new Vec2(40f, 40f),
            Vec4.One,
            SpriteBatchRotation.Clockwise90,
            SpriteBatchFlip.None);

        AssertVertex(fixture.Vertices.Vertices[0], new Vec2(-1f, 1f), new Vec2(0f, 0f));
        AssertVertex(fixture.Vertices.Vertices[1], new Vec2(-0.2f, 1f), new Vec2(0f, 1f));
        AssertVertex(fixture.Vertices.Vertices[2], new Vec2(-1f, 0.2f), new Vec2(1f, 0f));
        AssertVertex(fixture.Vertices.Vertices[3], new Vec2(-0.2f, 0.2f), new Vec2(1f, 1f));
    }

    /// <summary>Zero or negative destination and source sizes skip vertex generation.</summary>
    [TestMethod]
    public void Draw_WithInvalidSizes_SkipsVertices()
    {
        var fixture = CreateWriter();

        fixture.Writer.Draw(fixture.Texture, Vec2.Zero, Vec2.Zero);
        fixture.Writer.Draw(fixture.Texture, Vec2.Zero, new Vec2(-1f, 1f));
        fixture.Writer.Draw(fixture.Texture, Vec2.Zero, Vec2.One, Vec2.Zero, Vec2.Zero);

        Assert.AreEqual(0, fixture.Vertices.VertexCount);
    }

    /// <summary>Line drawing emits one quad worth of vertices.</summary>
    [TestMethod]
    public void DrawLine_WithHorizontalLine_EmitsQuad()
    {
        var fixture = CreateWriter();

        fixture.Writer.DrawLine(Vec2.Zero, new Vec2(100f, 0f), 10f, Vec4.One);

        Assert.AreEqual(4, fixture.Vertices.VertexCount);
    }

    /// <summary>Clipping a line trims the generated geometry to the clip rectangle.</summary>
    [TestMethod]
    public void DrawLine_WithClip_ClipsHorizontalLine()
    {
        var fixture = CreateWriter();
        fixture.Writer.Clip = new SpriteBatchClip(25f, 40f, 75f, 60f);

        fixture.Writer.DrawLine(new Vec2(0f, 50f), new Vec2(100f, 50f), 10f, Vec4.One);

        Assert.AreEqual(8, fixture.Vertices.VertexCount);
        AssertVerticesInsideClip(fixture.Vertices.Vertices, fixture.Writer.Clip.Value);
        AssertHasCanvasPosition(fixture.Vertices.Vertices, new Vec2(25f, 40f));
        AssertHasCanvasPosition(fixture.Vertices.Vertices, new Vec2(75f, 60f));
    }

    /// <summary>Clipping a diagonal line trims the generated polygon to the clip rectangle.</summary>
    [TestMethod]
    public void DrawLine_WithClip_ClipsDiagonalLine()
    {
        var fixture = CreateWriter();
        fixture.Writer.Clip = new SpriteBatchClip(40f, 40f, 60f, 60f);

        fixture.Writer.DrawLine(Vec2.Zero, new Vec2(100f, 100f), 5f, Vec4.One);

        Assert.AreEqual(16, fixture.Vertices.VertexCount);
        AssertVerticesInsideClip(fixture.Vertices.Vertices, fixture.Writer.Clip.Value);
    }

    /// <summary>Clipping fully outside a line skips vertex generation.</summary>
    [TestMethod]
    public void DrawLine_WithClipOutsideLine_SkipsVertices()
    {
        var fixture = CreateWriter();
        fixture.Writer.Clip = new SpriteBatchClip(40f, 40f, 60f, 60f);

        fixture.Writer.DrawLine(new Vec2(0f, 10f), new Vec2(100f, 10f), 5f, Vec4.One);

        Assert.AreEqual(0, fixture.Vertices.VertexCount);
    }

    /// <summary>Clipping fully to the left of a line skips vertex generation.</summary>
    [TestMethod]
    public void DrawLine_WithClipLeftOfLine_SkipsVertices()
    {
        var fixture = CreateWriter();
        fixture.Writer.Clip = new SpriteBatchClip(40f, 0f, 60f, 100f);

        fixture.Writer.DrawLine(new Vec2(0f, 50f), new Vec2(10f, 50f), 5f, Vec4.One);

        Assert.AreEqual(0, fixture.Vertices.VertexCount);
    }

    /// <summary>Clipping fully to the right of a line skips vertex generation.</summary>
    [TestMethod]
    public void DrawLine_WithClipRightOfLine_SkipsVertices()
    {
        var fixture = CreateWriter();
        fixture.Writer.Clip = new SpriteBatchClip(40f, 0f, 60f, 100f);

        fixture.Writer.DrawLine(new Vec2(80f, 50f), new Vec2(100f, 50f), 5f, Vec4.One);

        Assert.AreEqual(0, fixture.Vertices.VertexCount);
    }

    /// <summary>Clipping fully above a line skips vertex generation.</summary>
    [TestMethod]
    public void DrawLine_WithClipAboveLine_SkipsVertices()
    {
        var fixture = CreateWriter();
        fixture.Writer.Clip = new SpriteBatchClip(0f, 40f, 100f, 60f);

        fixture.Writer.DrawLine(new Vec2(0f, 90f), new Vec2(100f, 90f), 5f, Vec4.One);

        Assert.AreEqual(0, fixture.Vertices.VertexCount);
    }

    /// <summary>Invalid line clips skip vertex generation.</summary>
    [TestMethod]
    public void DrawLine_WithInvalidClip_SkipsVertices()
    {
        var fixture = CreateWriter();
        fixture.Writer.Clip = new SpriteBatchClip(60f, 60f, 40f, 40f);

        fixture.Writer.DrawLine(Vec2.Zero, new Vec2(100f, 100f), 5f, Vec4.One);

        Assert.AreEqual(0, fixture.Vertices.VertexCount);
    }

    /// <summary>Zero-length lines are ignored instead of emitting invalid coordinates.</summary>
    [TestMethod]
    public void DrawLine_WithZeroLengthLine_SkipsVertices()
    {
        var fixture = CreateWriter();

        fixture.Writer.DrawLine(new Vec2(10f, 10f), new Vec2(10f, 10f), 5f, Vec4.One);

        Assert.AreEqual(0, fixture.Vertices.VertexCount);
    }

    /// <summary>Non-positive line widths skip vertex generation.</summary>
    [TestMethod]
    public void DrawLine_WithNonPositiveWidth_SkipsVertices()
    {
        var fixture = CreateWriter();

        fixture.Writer.DrawLine(Vec2.Zero, Vec2.One, 0f, Vec4.One);

        Assert.AreEqual(0, fixture.Vertices.VertexCount);
    }

    /// <summary>Convenience draw overloads forward to the full vertex-generation path.</summary>
    [TestMethod]
    public void Draw_ConvenienceOverloads_EmitQuads()
    {
        var fixture = CreateWriter();

        fixture.Writer.Draw(Vec2.Zero, Vec2.One, Vec4.One);
        fixture.Writer.Draw(fixture.Texture, Vec2.Zero);
        fixture.Writer.Draw(fixture.Texture, Vec2.Zero, Vec2.One, Vec4.One);
        fixture.Writer.Draw(fixture.Texture, Vec2.Zero, Vec2.One, Vec2.Zero, Vec2.One, Vec4.One);
        fixture.Writer.DrawLine(Vec2.Zero, Vec2.One);
        fixture.Writer.DrawLine(fixture.Texture, Vec2.Zero, Vec2.One);

        Assert.AreEqual(24, fixture.Vertices.VertexCount);
    }

    /// <summary>Creates a writer with a 100 by 100 canvas and a 40 by 40 texture.</summary>
    private static WriterFixture CreateWriter()
    {
        var (_, gl) = Graphics2DTestHarness.CreateLayer();
        var texture = new Texture(gl, new Vec2u(40u, 40u), GlTextureTarget.Texture2D);
        var canvas = new SpriteBatchCanvas { Size = new Vec2(100f, 100f) };
        var vertices = new SpriteBatchVertices(16);
        return new WriterFixture(texture, vertices, new SpriteBatchWriter(texture, canvas, vertices));
    }

    /// <summary>Asserts the position, texture coordinate, and default color for one generated vertex.</summary>
    private static void AssertVertex(SpriteBatchVertex vertex, Vec2 position, Vec2 texCoord)
    {
        AssertVector(position, vertex.Position);
        AssertVector(texCoord, vertex.TexCoord);
        AssertVector4(Vec4.One, vertex.Color);
    }

    /// <summary>Asserts two vectors are equal within the writer's floating-point tolerance.</summary>
    private static void AssertVector(Vec2 expected, Vec2 actual)
    {
        Assert.AreEqual(expected.X, actual.X, 0.0001f);
        Assert.AreEqual(expected.Y, actual.Y, 0.0001f);
    }

    /// <summary>Asserts two four-component vectors are equal within the writer's floating-point tolerance.</summary>
    private static void AssertVector4(Vec4 expected, Vec4 actual)
    {
        Assert.AreEqual(expected.X, actual.X, 0.0001f);
        Assert.AreEqual(expected.Y, actual.Y, 0.0001f);
        Assert.AreEqual(expected.Z, actual.Z, 0.0001f);
        Assert.AreEqual(expected.W, actual.W, 0.0001f);
    }

    /// <summary>Asserts every generated vertex maps back inside a canvas-space clip rectangle.</summary>
    private static void AssertVerticesInsideClip(ReadOnlySpan<SpriteBatchVertex> vertices, SpriteBatchClip clip)
    {
        foreach (var vertex in vertices)
        {
            var position = CanvasPosition(vertex.Position);
            Assert.IsTrue(position.X >= clip.Min.X - 0.0001f);
            Assert.IsTrue(position.X <= clip.Max.X + 0.0001f);
            Assert.IsTrue(position.Y >= clip.Min.Y - 0.0001f);
            Assert.IsTrue(position.Y <= clip.Max.Y + 0.0001f);
        }
    }

    /// <summary>Asserts that at least one generated vertex maps to the supplied canvas-space position.</summary>
    private static void AssertHasCanvasPosition(ReadOnlySpan<SpriteBatchVertex> vertices, Vec2 expected)
    {
        foreach (var vertex in vertices)
        {
            var position = CanvasPosition(vertex.Position);
            if (Vec2.DistanceSquared(position, expected) <= 0.0001f)
                return;
        }

        Assert.Fail($"Expected a vertex at canvas position {expected}.");
    }

    /// <summary>Converts a normalized test vertex position back into the 100 by 100 test canvas.</summary>
    private static Vec2 CanvasPosition(Vec2 position) => new((position.X + 1f) * 50f, (1f - position.Y) * 50f);

    /// <summary>Shared writer test fixture.</summary>
    private sealed record WriterFixture(Texture Texture, SpriteBatchVertices Vertices, SpriteBatchWriter Writer);
}
