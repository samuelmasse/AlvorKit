namespace AlvorKit.Windowing.Test;

[TestClass]
public class MouseTest
{
    [TestMethod]
    public void Mouse_EmptyState_HasCorrectValues()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var mouse = new Mouse(loop);

        Assert.AreEqual(WindowMouseButton.Left, mouse.Main);
        Assert.AreEqual(WindowMouseButton.Right, mouse.Secondary);
        Assert.AreEqual(WindowCursorMode.Normal, mouse.CursorMode);
        Assert.AreEqual(Vector2.Zero, mouse.Wheel);
        Assert.IsFalse(mouse.IsMainDown());
        Assert.IsTrue(mouse.IsMainUp());
        Assert.IsFalse(mouse.IsMainPressed());

        mouse.CursorMode = WindowCursorMode.Captured;

        Assert.AreEqual(WindowCursorMode.Captured, host.CursorMode);
    }

    [TestMethod]
    public void Mouse_IsMainDown_Works()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var mouse = new Mouse(loop);

        host.RaiseMouseDown(WindowMouseButton.Left);

        Assert.IsTrue(mouse.IsMainDown());
        mouse.Tick();
        Assert.IsTrue(mouse.IsMainDown());
    }

    [TestMethod]
    public void Mouse_IsMainPressed_Works()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var mouse = new Mouse(loop);

        host.RaiseMouseDown(WindowMouseButton.Left);

        Assert.IsTrue(mouse.IsMainPressed());
        mouse.Tick();
        Assert.IsFalse(mouse.IsMainPressed());
        host.RaiseMouseDown(WindowMouseButton.Left);
        Assert.IsFalse(mouse.IsMainPressed());
    }

    [TestMethod]
    public void Mouse_IsMainUp_Works()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var mouse = new Mouse(loop);

        host.RaiseMouseDown(WindowMouseButton.Left);
        Assert.IsFalse(mouse.IsMainUp());
        host.RaiseMouseUp(WindowMouseButton.Left);

        Assert.IsTrue(mouse.IsMainUp());
    }

    [TestMethod]
    public void Mouse_SecondaryConvenienceMethods_Work()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var mouse = new Mouse(loop);

        Assert.IsTrue(mouse.IsSecondaryUp());

        host.RaiseMouseDown(WindowMouseButton.Right);

        Assert.IsTrue(mouse.IsSecondaryDown());
        Assert.IsTrue(mouse.IsSecondaryPressed());
    }

    [TestMethod]
    public void Mouse_InvalidButton_ThrowsException()
    {
        var (_, loop) = WindowingTestFactory.Create();
        var mouse = new Mouse(loop);

        Assert.ThrowsException<InvalidOperationException>(() => mouse.IsButtonDown((WindowMouseButton)(-1)));
        Assert.ThrowsException<InvalidOperationException>(() => mouse.IsButtonDown((WindowMouseButton)int.MaxValue));
    }

    [TestMethod]
    public void Mouse_Wheel_Works()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var mouse = new Mouse(loop);

        host.RaiseMouseWheel(new(20, 20));

        Assert.AreEqual(new Vector2(20, 20), mouse.Wheel);
        mouse.Tick();
        Assert.AreEqual(Vector2.Zero, mouse.Wheel);
    }
}
