namespace AlvorKit.Windowing.Test;

[TestClass]
public class WindowInputTest
{
    /// <summary>Verifies writable input settings are exposed separately from keyboard and mouse readers.</summary>
    [TestMethod]
    public void WindowInput_WritableState_ForwardsToLoopState()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var input = new WindowInput(loop);
        var mouse = new Mouse(loop);

        input.Clipboard = "hello";
        input.CursorMode = CursorMode.Captured;
        input.CursorShape = CursorShape.Text;
        input.MousePosition = new(12, 34);
        input.Track = true;

        Assert.AreEqual("hello", host.Clipboard);
        Assert.AreEqual("hello", input.Clipboard);
        Assert.AreEqual(CursorMode.Captured, host.CursorMode);
        Assert.AreEqual(CursorMode.Captured, input.CursorMode);
        Assert.AreEqual(CursorMode.Captured, mouse.CursorMode);
        Assert.AreEqual(CursorShape.Text, host.CursorShape);
        Assert.AreEqual(CursorShape.Text, input.CursorShape);
        Assert.AreEqual(CursorShape.Text, mouse.CursorShape);
        Assert.AreEqual(new Vec2(12, 34), host.MousePosition);
        Assert.AreEqual(new Vec2(12, 34), mouse.Position);
        Assert.IsTrue(input.Track);
        Assert.IsTrue(mouse.Track);
    }
}
