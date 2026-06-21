namespace AlvorKit.Windowing.Test;

[TestClass]
public class MousePositionTest
{
    [TestMethod]
    public void Mouse_PositionEmptyState_HasCorrectValues()
    {
        var (_, loop) = WindowingTestFactory.Create();
        var mouse = new Mouse(loop);

        Assert.AreEqual(Vec2.Zero, mouse.Position);
        Assert.AreEqual(Vec2.Zero, mouse.Delta);
        Assert.IsFalse(mouse.Track);
    }

    [TestMethod]
    public void Mouse_Position_Works()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var mouse = new Mouse(loop);

        host.RaiseMouseMove(new(40, 40));

        Assert.AreEqual(new Vec2(40, 40), mouse.Position);
    }

    [TestMethod]
    public void Mouse_Position_CanBeSet()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var mouse = new Mouse(loop);
        var input = new WindowInput(loop)
        {
            MousePosition = new(50, 50)
        };

        Assert.AreEqual(new Vec2(50, 50), mouse.Position);
        Assert.AreEqual(new Vec2(50, 50), input.MousePosition);
        Assert.AreEqual(new Vec2(50, 50), host.MousePosition);
    }

    [TestMethod]
    public void Mouse_Position_SetDoesNotProduceDelta()
    {
        var (host, loop) = WindowingTestFactory.Create(new(100, 100));
        var mouse = new Mouse(loop);
        var input = new WindowInput(loop);
        host.IsFocused = true;
        input.Track = true;

        host.RaiseMouseMove(new(10, 10));
        host.RaiseUpdate();
        host.RaiseMouseMove(new(20, 20));
        host.RaiseUpdate();
        host.RaiseMouseMove(new(30, 30));
        host.RaiseUpdate();
        input.MousePosition = new(500, 500);
        host.RaiseUpdate();

        Assert.AreEqual(Vec2.Zero, mouse.Delta);
    }

    [TestMethod]
    public void Mouse_Delta_CorrectValueWhenTracked()
    {
        var (host, loop) = WindowingTestFactory.Create(new(100, 100));
        var mouse = new Mouse(loop);
        var input = new WindowInput(loop);
        host.IsFocused = true;

        host.RaiseMouseMove(new(40, 40));
        host.RaiseUpdate();
        input.Track = true;
        host.RaiseMouseMove(new(20, 20));
        host.RaiseUpdate();
        host.RaiseMouseMove(new(25, 25));
        host.RaiseUpdate();
        host.RaiseMouseMove(new(100, 100));
        host.RaiseUpdate();

        Assert.AreEqual(new Vec2(75, 75), mouse.Delta);

        host.RaiseResize(new(200, 200));
        host.RaiseMouseMove(new(235, 235));
        host.RaiseUpdate();

        Assert.AreEqual(Vec2.Zero, mouse.Delta);

        host.RaiseMouseMove(new(74, 74));
        host.RaiseUpdate();
        host.RaiseMouseMove(new(30, 30));
        host.RaiseUpdate();

        Assert.AreEqual(new Vec2(-44, -44), mouse.Delta);
    }
}
