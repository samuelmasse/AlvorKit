namespace AlvorKit.Windowing;

/// <summary>Polled button and axis state for one gamepad slot.</summary>
/// <param name="Buttons">The buttons currently held down.</param>
/// <param name="LeftStick">The left stick position in the range -1 to 1 per axis.</param>
/// <param name="RightStick">The right stick position in the range -1 to 1 per axis.</param>
/// <param name="LeftTrigger">The left trigger value in the range 0 to 1.</param>
/// <param name="RightTrigger">The right trigger value in the range 0 to 1.</param>
public readonly record struct GamepadState(
    GamepadButtons Buttons,
    Vec2 LeftStick,
    Vec2 RightStick,
    float LeftTrigger,
    float RightTrigger)
{
    /// <summary>Returns whether any of the requested buttons are currently down.</summary>
    public bool IsButtonDown(GamepadButtons button) => (Buttons & button) != 0;

    /// <summary>Returns the value of a single axis.</summary>
    public float Axis(GamepadAxis axis) => axis switch
    {
        GamepadAxis.LeftX => LeftStick.X,
        GamepadAxis.LeftY => LeftStick.Y,
        GamepadAxis.RightX => RightStick.X,
        GamepadAxis.RightY => RightStick.Y,
        GamepadAxis.LeftTrigger => LeftTrigger,
        GamepadAxis.RightTrigger => RightTrigger,
        _ => throw new ArgumentOutOfRangeException(nameof(axis))
    };
}
