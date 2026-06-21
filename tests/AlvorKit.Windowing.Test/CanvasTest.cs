namespace AlvorKit.Windowing.Test;

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

    [TestMethod]
    public void Canvas_ResizeToZero_IsNoOp()
    {
        var (host, loop) = WindowingTestFactory.Create(new(300, 300));
        var canvas = new WindowCanvas(loop);

        host.RaiseResize(Vec2u.Zero);

        Assert.AreEqual(new Vec2u(300u, 300u), canvas.Size);
    }
}
