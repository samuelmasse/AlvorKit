namespace AlvorKit.Graphics2D.Test;

/// <summary>Tests the top-level sprite batch lifecycle.</summary>
[TestClass]
public sealed class SpriteBatchTest
{
    /// <summary>Beginning a batch twice surfaces the lifecycle guard.</summary>
    [TestMethod]
    public void Begin_Twice_Throws()
    {
        using var spriteBatch = CreateSpriteBatch();

        spriteBatch.Begin(Vec2.One);

        Assert.Throws<InvalidOperationException>(() => spriteBatch.Begin(Vec2.One));
    }

    /// <summary>Ending without a matching begin surfaces the lifecycle guard.</summary>
    [TestMethod]
    public void End_WithNoBegin_Throws()
    {
        using var spriteBatch = CreateSpriteBatch();

        Assert.Throws<InvalidOperationException>(spriteBatch.End);
    }

    /// <summary>An empty begin/end pair succeeds and leaves the batch reusable.</summary>
    [TestMethod]
    public void BeginAndEnd_EmptyBatch_Succeeds()
    {
        using var spriteBatch = CreateSpriteBatch();

        spriteBatch.Begin(new Vec2(100f, 100f));
        spriteBatch.End();
        spriteBatch.Begin(new Vec2(100f, 100f));
        spriteBatch.End();
    }

    /// <summary>The default constructor builds its built-in shaders from the reported texture-slot count.</summary>
    [TestMethod]
    public void Constructor_WithDefaultShaders_Succeeds()
    {
        var (_, gl) = Graphics2DTestHarness.CreateLayer();

        using var spriteBatch = new SpriteBatch(gl);

        Assert.IsNotNull(spriteBatch.Writer);
    }

    /// <summary>Disposing a freshly created batch deletes all owned OpenGL resources.</summary>
    [TestMethod]
    public void Dispose_DeletesOwnedResources()
    {
        var (backend, spriteBatch) = CreateSpriteBatchWithBackend();

        spriteBatch.Dispose();

        Assert.IsTrue(backend.Deleted.Count >= 5);
    }

    /// <summary>A drawn batch flushes one draw call through the backend.</summary>
    [TestMethod]
    public void Draw_Basic_EndFlushesDrawCall()
    {
        var (backend, spriteBatch) = CreateSpriteBatchWithBackend();
        using (spriteBatch)
        {
            spriteBatch.Begin(new Vec2(100f, 100f));
            spriteBatch.Writer.Draw(Vec2.Zero, new Vec2(10f, 10f));
            spriteBatch.End();
        }

        Assert.AreEqual(1, backend.DrawElementsCalls);
    }

    /// <summary>Many draws can be flushed repeatedly after buffers have grown.</summary>
    [TestMethod]
    public void Draw_Many_EndSucceedsTwice()
    {
        using var spriteBatch = CreateSpriteBatch();

        for (var pass = 0; pass < 2; pass++)
        {
            spriteBatch.Begin(new Vec2(100f, 100f));
            for (var i = 0; i < 128; i++)
                spriteBatch.Writer.Draw(Vec2.Zero, Vec2.One);
            spriteBatch.End();
        }
    }

    /// <summary>Draw overload combinations accept every rotation and flip combination.</summary>
    [TestMethod]
    public void Draw_VariousOptions_Succeeds()
    {
        var (_, gl) = Graphics2DTestHarness.CreateLayer();
        using var spriteBatch = CreateSpriteBatch(gl);
        using var texture = new Texture(gl, Vec2u.One, GlTextureTarget.Texture2D);

        foreach (var rotation in Enum.GetValues<SpriteBatchRotation>())
        {
            spriteBatch.Writer.Draw(texture, Vec2.Zero, Vec2.One, Vec2.Zero, Vec2.One, Vec4.One, rotation, SpriteBatchFlip.None);
            spriteBatch.Writer.Draw(texture, Vec2.Zero, Vec2.One, Vec2.Zero, Vec2.One, Vec4.One, rotation, SpriteBatchFlip.Horizontal);
            spriteBatch.Writer.Draw(texture, Vec2.Zero, Vec2.One, Vec2.Zero, Vec2.One, Vec4.One, rotation, SpriteBatchFlip.Vertical);
            spriteBatch.Writer.Draw(
                texture,
                Vec2.Zero,
                Vec2.One,
                Vec2.Zero,
                Vec2.One,
                Vec4.One,
                rotation,
                SpriteBatchFlip.Horizontal | SpriteBatchFlip.Vertical);
        }
    }

    /// <summary>Creates a sprite batch with a fresh recording backend.</summary>
    private static SpriteBatch CreateSpriteBatch()
    {
        var (_, layer) = Graphics2DTestHarness.CreateLayer();
        return CreateSpriteBatch(layer);
    }

    /// <summary>Creates a sprite batch with a fresh recording backend and returns both objects.</summary>
    private static (Graphics2DTestGl Backend, SpriteBatch SpriteBatch) CreateSpriteBatchWithBackend()
    {
        var (backend, layer) = Graphics2DTestHarness.CreateLayer();
        return (backend, CreateSpriteBatch(layer));
    }

    /// <summary>Creates a sprite batch over the supplied strict layer using minimal shader source.</summary>
    private static SpriteBatch CreateSpriteBatch(GlLayer gl) => new(gl, string.Empty, string.Empty);
}
