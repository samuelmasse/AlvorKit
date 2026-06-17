namespace AlvorKit.Graphics2D.Test;

/// <summary>Tests small observable state on sprite batch quad buffers.</summary>
[TestClass]
public sealed class QuadBufferTest
{
    /// <summary>New quad buffers start with zero capacity before any draw path grows them.</summary>
    [TestMethod]
    public void Constructor_InitialCapacityIsZero()
    {
        var (_, gl) = Graphics2DTestHarness.CreateLayer();
        using var indexBuffer = new QuadIndexBuffer(gl);
        using var vertexBuffer = new QuadVertexBuffer<SpriteBatchVertex>(gl, SpriteBatchVertex.Size);

        Assert.AreEqual(0, indexBuffer.Capacity);
        Assert.AreEqual(0, vertexBuffer.Capacity);
    }
}
