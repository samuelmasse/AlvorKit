namespace AlvorKit.Windowing.Test;

[TestClass]
public class GamepadStateTest
{
    [TestMethod]
    public void GamepadState_Default_ReportsNothingPressed()
    {
        var state = default(GamepadState);

        Assert.IsFalse(state.IsButtonDown(GamepadButtons.A));
        Assert.IsFalse(state.IsButtonDown(GamepadButtons.DPadLeft));
        Assert.AreEqual(0f, state.Axis(GamepadAxis.LeftX));
        Assert.AreEqual(0f, state.Axis(GamepadAxis.RightTrigger));
    }

    [TestMethod]
    public void GamepadState_IsButtonDown_MatchesButtonFlags()
    {
        var state = new GamepadState(GamepadButtons.A | GamepadButtons.Start | GamepadButtons.DPadLeft, default, default, 0f, 0f);

        Assert.IsTrue(state.IsButtonDown(GamepadButtons.A));
        Assert.IsTrue(state.IsButtonDown(GamepadButtons.Start));
        Assert.IsTrue(state.IsButtonDown(GamepadButtons.DPadLeft));
        Assert.IsFalse(state.IsButtonDown(GamepadButtons.B));
        Assert.IsFalse(state.IsButtonDown(GamepadButtons.Guide));
        Assert.IsTrue(state.IsButtonDown(GamepadButtons.A | GamepadButtons.B));
    }

    [TestMethod]
    public void GamepadState_Axis_ReturnsEachAxisValue()
    {
        var state = new GamepadState(GamepadButtons.None, (0.1f, -0.2f), (0.3f, -0.4f), 0.5f, 0.6f);

        Assert.AreEqual(0.1f, state.Axis(GamepadAxis.LeftX));
        Assert.AreEqual(-0.2f, state.Axis(GamepadAxis.LeftY));
        Assert.AreEqual(0.3f, state.Axis(GamepadAxis.RightX));
        Assert.AreEqual(-0.4f, state.Axis(GamepadAxis.RightY));
        Assert.AreEqual(0.5f, state.Axis(GamepadAxis.LeftTrigger));
        Assert.AreEqual(0.6f, state.Axis(GamepadAxis.RightTrigger));
    }

    [TestMethod]
    public void GamepadState_Axis_InvalidAxisThrows()
    {
        var state = default(GamepadState);

        Assert.ThrowsException<ArgumentOutOfRangeException>(() => state.Axis((GamepadAxis)999));
    }
}
