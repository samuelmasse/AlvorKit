namespace AlvorKit.Windowing.Test;

[TestClass]
public class GamepadsTest
{
    [TestMethod]
    public void Gamepads_Disconnected_HasCorrectValues()
    {
        var (_, loop) = WindowingTestFactory.Create();
        var gamepads = new Gamepads(loop);

        Assert.IsFalse(gamepads.IsConnected(0));
        Assert.IsFalse(gamepads.IsButtonDown(0, GamepadButtons.A));
        Assert.AreEqual(0f, gamepads.Axis(0, GamepadAxis.LeftX));
    }

    [TestMethod]
    public void Gamepads_Connected_ReadsHostState()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var gamepads = new Gamepads(loop);
        host.GamepadStates[0] = new(GamepadButtons.A, (0.5f, -0.5f), default, 0f, 1f);

        Assert.IsTrue(gamepads.IsConnected(0));
        Assert.IsTrue(gamepads.IsButtonDown(0, GamepadButtons.A));
        Assert.IsFalse(gamepads.IsButtonDown(0, GamepadButtons.B));
        Assert.AreEqual(0.5f, gamepads.Axis(0, GamepadAxis.LeftX));
        Assert.AreEqual(1f, gamepads.Axis(0, GamepadAxis.RightTrigger));
    }

    [TestMethod]
    public void Gamepads_Slots_AreIndependent()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var gamepads = new Gamepads(loop);
        host.GamepadStates[0] = new(GamepadButtons.A, default, default, 0f, 0f);
        host.GamepadStates[1] = new(GamepadButtons.B, (1f, 0f), default, 0f, 0f);

        Assert.IsTrue(gamepads.IsButtonDown(0, GamepadButtons.A));
        Assert.IsFalse(gamepads.IsButtonDown(0, GamepadButtons.B));
        Assert.IsTrue(gamepads.IsButtonDown(1, GamepadButtons.B));
        Assert.AreEqual(0f, gamepads.Axis(0, GamepadAxis.LeftX));
        Assert.AreEqual(1f, gamepads.Axis(1, GamepadAxis.LeftX));
        Assert.IsFalse(gamepads.IsConnected(2));
    }
}
