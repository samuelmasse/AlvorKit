namespace AlvorKit;

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
        Assert.AreEqual(CursorShape.Default, mouse.CursorShape);
        Assert.AreEqual(Vec2.Zero, mouse.Wheel);
        Assert.IsFalse(mouse.IsMainDown());
        Assert.IsTrue(mouse.IsMainUp());
        Assert.IsFalse(mouse.IsMainPressed());

        input.CursorMode = CursorMode.Captured;
        mouse.CursorShape = CursorShape.Hand;

        Assert.AreEqual(CursorMode.Captured, host.CursorMode);
        Assert.AreEqual(CursorMode.Captured, mouse.CursorMode);
        Assert.AreEqual(CursorShape.Hand, host.CursorShape);
        Assert.AreEqual(CursorShape.Hand, input.CursorShape);
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

    /// <summary>All wheel events, including fractions and zero deltas, contribute to one update's input.</summary>
    [TestMethod]
    public void Mouse_Wheel_AccumulatesBurstUntilUpdate()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var mouse = new Mouse(loop);
        var observed = Vec2.Zero;
        loop.Update += _ => observed = mouse.Wheel;
        host.RaiseMouseWheel(new(0.25f, 2f));
        host.RaiseMouseWheel(new(0.5f, 3f));
        host.RaiseMouseWheel(Vec2.Zero);

        Assert.AreEqual(new Vec2(0.75f, 5f), mouse.Wheel);
        host.RaiseUpdate();
        Assert.AreEqual(new Vec2(0.75f, 5f), observed);
        Assert.AreEqual(Vec2.Zero, mouse.Wheel);
        host.RaiseUpdate();
        Assert.AreEqual(Vec2.Zero, observed);
    }

    /// <summary>Opposite events cancel, and an input reset discards every pending wheel event.</summary>
    [TestMethod]
    public void Mouse_Wheel_ReversalAndResetDiscardPendingMotion()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var mouse = new Mouse(loop);
        host.RaiseMouseWheel(new(1f, 3f));
        host.RaiseMouseWheel(new(-1f, -3f));

        Assert.AreEqual(Vec2.Zero, mouse.Wheel);
        host.RaiseMouseWheel(new(0, -2f));
        Assert.AreEqual(new Vec2(0, -2f), mouse.Wheel);
        loop.ResetInput();
        Assert.AreEqual(Vec2.Zero, mouse.Wheel);
        host.RaiseMouseWheel(new(0, 0.25f));
        Assert.AreEqual(new Vec2(0, 0.25f), mouse.Wheel);
    }

    /// <summary>Accumulating and consuming wheel bursts allocates no managed memory after initialization.</summary>
    [TestMethod]
    public void Mouse_Wheel_BurstsDoNotAllocate()
    {
        var (host, loop) = WindowingTestFactory.Create();

        for (var i = 0; i < 100; i++)
        {
            host.RaiseMouseWheel(new(0, 1));
            host.RaiseMouseWheel(new(0, -0.25f));
            loop.Mouse.Tick();
        }

        var before = GC.GetAllocatedBytesForCurrentThread();

        for (var i = 0; i < 1000; i++)
        {
            host.RaiseMouseWheel(new(0, 1));
            host.RaiseMouseWheel(new(0, -0.25f));
            loop.Mouse.Tick();
        }

        Assert.AreEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before);
    }
}
