namespace AlvorKit.Windowing.Test;

[TestClass]
public class MouseTest
{
    [TestMethod]
    public void Mouse_EmptyState_HasCorrectValues()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var mouse = new Mouse(loop);
        var input = new WindowInput(loop);

        Assert.AreEqual(MouseButton.Left, mouse.Main);
        Assert.AreEqual(MouseButton.Right, mouse.Secondary);
        Assert.AreEqual(CursorMode.Normal, mouse.CursorMode);
        Assert.AreEqual(Vec2.Zero, mouse.Wheel);
        Assert.IsFalse(mouse.IsMainDown());
        Assert.IsTrue(mouse.IsMainUp());
        Assert.IsFalse(mouse.IsMainPressed());

        input.CursorMode = CursorMode.Captured;

        Assert.AreEqual(CursorMode.Captured, host.CursorMode);
        Assert.AreEqual(CursorMode.Captured, mouse.CursorMode);
    }

    [TestMethod]
    public void Mouse_IsMainDown_Works()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var mouse = new Mouse(loop);

        host.RaiseMouseDown(MouseButton.Left);

        Assert.IsTrue(mouse.IsMainDown());
        host.RaiseUpdate();
        Assert.IsTrue(mouse.IsMainDown());
    }

    [TestMethod]
    public void Mouse_IsMainPressed_Works()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var mouse = new Mouse(loop);

        host.RaiseMouseDown(MouseButton.Left);

        Assert.IsTrue(mouse.IsMainPressed());
        host.RaiseUpdate();
        Assert.IsFalse(mouse.IsMainPressed());
        host.RaiseMouseDown(MouseButton.Left);
        Assert.IsFalse(mouse.IsMainPressed());
    }

    [TestMethod]
    public void Mouse_IsMainUp_Works()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var mouse = new Mouse(loop);

        host.RaiseMouseDown(MouseButton.Left);
        Assert.IsFalse(mouse.IsMainUp());
        host.RaiseMouseUp(MouseButton.Left);

        Assert.IsTrue(mouse.IsMainUp());
    }

    [TestMethod]
    public void Mouse_SecondaryConvenienceMethods_Work()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var mouse = new Mouse(loop);

        Assert.IsTrue(mouse.IsSecondaryUp());

        host.RaiseMouseDown(MouseButton.Right);

        Assert.IsTrue(mouse.IsSecondaryDown());
        Assert.IsTrue(mouse.IsSecondaryPressed());
    }

    [TestMethod]
    public void Mouse_InvalidButton_ThrowsException()
    {
        var (_, loop) = WindowingTestFactory.Create();
        var mouse = new Mouse(loop);

        Assert.ThrowsException<InvalidOperationException>(() => mouse.IsButtonDown((MouseButton)(-1)));
        Assert.ThrowsException<InvalidOperationException>(() => mouse.IsButtonDown((MouseButton)int.MaxValue));
    }

    [TestMethod]
    public void Mouse_Wheel_Works()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var mouse = new Mouse(loop);

        host.RaiseMouseWheel(new(20, 20));

        Assert.AreEqual(new Vec2(20, 20), mouse.Wheel);
        host.RaiseUpdate();
        Assert.AreEqual(Vec2.Zero, mouse.Wheel);
    }
}
