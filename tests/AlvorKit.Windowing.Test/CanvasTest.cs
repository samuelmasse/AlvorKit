namespace AlvorKit;

[TestClass]
public class CanvasTest
{
    [TestMethod]
    public void Canvas_SizeInitial_IsSetToCorrectValue()
    {
        var (_, loop) = WindowingTestFactory.Create(new(200, 200));
        var canvas = new WindowCanvas(loop);

        Assert.AreEqual(new Vec2u(200u, 200u), canvas.Size);
    }

    [TestMethod]
    public void Canvas_SizeAfterResize_IsSetToCorrectValue()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var canvas = new WindowCanvas(loop);
        var updateInvoked = 0;
        var renderInvoked = 0;
        loop.Update += (e) =>
        {
            Assert.AreEqual(0, e);
            updateInvoked++;
        };
        loop.Render += () => renderInvoked++;

        host.RaiseResize(new(400, 400));

        Assert.AreEqual(new Vec2u(400u, 400u), canvas.Size);
        Assert.AreEqual(1, updateInvoked);
        Assert.AreEqual(1, renderInvoked);
        Assert.AreEqual(1, host.SwapBuffersCount);
    }

    /// <summary>Minimize events with either empty axis cannot replace a valid drawable or trigger rendering.</summary>
    [TestMethod]
    [DataRow(0u, 0u)]
    [DataRow(0u, 300u)]
    [DataRow(300u, 0u)]
    public void Canvas_ResizeToZero_IsNoOp(uint width, uint height)
    {
        var (host, loop) = WindowingTestFactory.Create(new(300, 300));
        var canvas = new WindowCanvas(loop);

        host.RaiseResize(new(width, height));

        Assert.AreEqual(new Vec2u(300u, 300u), canvas.Size);
        Assert.AreEqual(0, host.SwapBuffersCount);
    }
}
