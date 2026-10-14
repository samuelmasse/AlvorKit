namespace AlvorKit;

/// <summary>Verifies affine sprite geometry and interpolated clipping through the production writer.</summary>
[TestClass]
public class SpriteBatchTransformTest
{
    /// <summary>Rotation changes geometry around the local origin and preserves the flipped source rectangle.</summary>
    [TestMethod]
    public void RotationAndFlipPreserveCorners()
    {
        var fixture = Create();
        var transform = Mat4.CreateTranslation(new Vec3(30, 40, 0));
        transform *= Mat4.CreateRotationZ(MathF.PI / 2);
        transform *= Mat4.CreateScale(new Vec3(2, 1, 1));
        fixture.Writer.DrawTransformed(fixture.Texture, new Box2((8, 12), (28, 22)), transform, Vec4.One,
            SpriteBatchFlip.Horizontal);

        Assert.AreEqual(4, fixture.Vertices.VertexCount);
        AssertVertex(fixture.Vertices.Vertices[0], (30, 40), (0.7f, 0.7f));
        AssertVertex(fixture.Vertices.Vertices[1], (30, 80), (0.2f, 0.7f));
        AssertVertex(fixture.Vertices.Vertices[2], (20, 40), (0.7f, 0.45f));
        AssertVertex(fixture.Vertices.Vertices[3], (20, 80), (0.2f, 0.45f));
    }

    /// <summary>Clipping a rotated sprite interpolates the texture mapping along every generated edge.</summary>
    [TestMethod]
    public void ClippedRotationInterpolatesTextureCoordinates()
    {
        var fixture = Create();
        fixture.Writer.Clip = new SpriteBatchClip((30, 30), (50, 50));
        var transform = Mat4.CreateTranslation(new Vec3(40, 20, 0)) * Mat4.CreateRotationZ(MathF.PI / 4);
        fixture.Writer.DrawTransformed(fixture.Texture, new Box2(Vec2.Zero, new Vec2(40)), transform,
            Vec4.One, SpriteBatchFlip.None);
        Assert.IsTrue(fixture.Vertices.VertexCount >= 8);

        foreach (var vertex in fixture.Vertices.Vertices)
        {
            var p = CanvasPosition(vertex.Position);
            Assert.IsTrue(p.X >= 29.999f && p.X <= 50.001f && p.Y >= 29.999f && p.Y <= 50.001f);
            var localX = ((p.X - 40) + (p.Y - 20)) / MathF.Sqrt(2);
            var localY = (-(p.X - 40) + (p.Y - 20)) / MathF.Sqrt(2);
            Assert.AreEqual(localX / 40, vertex.TexCoord.X, 0.0001f);
            Assert.AreEqual(1 - localY / 40, vertex.TexCoord.Y, 0.0001f);
        }
    }

    /// <summary>Empty source, empty clip, and disjoint clip rectangles emit no geometry.</summary>
    [TestMethod]
    public void EmptyAndDisjointInputsEmitNothing()
    {
        var fixture = Create();
        fixture.Writer.DrawTransformed(fixture.Texture, default, Mat4.Identity, Vec4.One, SpriteBatchFlip.None);
        fixture.Writer.Clip = new SpriteBatchClip((10, 10), (10, 30));
        fixture.Writer.DrawTransformed(fixture.Texture, new Box2(Vec2.Zero, new Vec2(40)), Mat4.Identity,
            Vec4.One, SpriteBatchFlip.None);
        fixture.Writer.Clip = new SpriteBatchClip((50, 50), (60, 60));
        fixture.Writer.DrawTransformed(fixture.Texture, new Box2(Vec2.Zero, new Vec2(40)), Mat4.Identity,
            Vec4.One, SpriteBatchFlip.None);
        Assert.AreEqual(0, fixture.Vertices.VertexCount);
    }

    /// <summary>Clipped draws reuse collector capacity without per-sprite managed allocations.</summary>
    [TestMethod]
    public void WarmClippedDrawDoesNotAllocate()
    {
        var fixture = Create();
        fixture.Writer.Clip = new SpriteBatchClip((10, 10), (30, 30));
        DrawAndReset(fixture);
        var before = GC.GetAllocatedBytesForCurrentThread();

        for (var i = 0; i < 1000; i++)
            DrawAndReset(fixture);

        Assert.AreEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before);
    }

    /// <summary>Exercises the clipped path and releases queued vertices for the next frame.</summary>
    private static void DrawAndReset(Fixture fixture)
    {
        fixture.Writer.DrawTransformed(fixture.Texture, new Box2(Vec2.Zero, new Vec2(40)), Mat4.Identity,
            Vec4.One, SpriteBatchFlip.None);
        fixture.Vertices.Reset();
    }

    /// <summary>Constructs a writer with real vertex collection and a recording GL layer.</summary>
    private static Fixture Create()
    {
        var (_, gl) = Graphics2DTestHarness.CreateLayer();
        var texture = new Texture(gl, (40u, 40u), GlTextureTarget.Texture2D);
        var vertices = new SpriteBatchVertices(16);
        var canvas = new SpriteBatchCanvas { Size = (100, 100) };
        return new Fixture(texture, vertices, new SpriteBatchWriter(texture, canvas, vertices));
    }

    /// <summary>Compares a generated vertex in canvas pixels and normalized texture coordinates.</summary>
    private static void AssertVertex(SpriteBatchVertex vertex, Vec2 position, Vec2 uv)
    {
        var actual = CanvasPosition(vertex.Position);
        Assert.AreEqual(position.X, actual.X, 0.0001f);
        Assert.AreEqual(position.Y, actual.Y, 0.0001f);
        Assert.AreEqual(uv.X, vertex.TexCoord.X, 0.0001f);
        Assert.AreEqual(uv.Y, vertex.TexCoord.Y, 0.0001f);
    }

    /// <summary>Converts normalized coordinates to the fixture's canvas pixels.</summary>
    private static Vec2 CanvasPosition(Vec2 value) => new((value.X + 1) * 50, (1 - value.Y) * 50);

    /// <summary>Owns the test writer's collaborators.</summary>
    private record Fixture(Texture Texture, SpriteBatchVertices Vertices, SpriteBatchWriter Writer);
}
