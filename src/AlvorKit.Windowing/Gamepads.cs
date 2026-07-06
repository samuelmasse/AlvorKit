namespace AlvorKit.Windowing;

/// <summary>Reads polled gamepad state from a window loop.</summary>
public class Gamepads(WindowLoop window)
{
    /// <summary>Returns whether a gamepad is connected at the slot.</summary>
    public bool IsConnected(int index) => window.Gamepads.IsConnected(index);

    /// <summary>Returns whether any of the requested buttons are down on the gamepad slot.</summary>
    public bool IsButtonDown(int index, GamepadButtons button) => window.Gamepads.IsButtonDown(index, button);

    /// <summary>Returns the axis value for the gamepad slot, or 0 when disconnected.</summary>
    public float Axis(int index, GamepadAxis axis) => window.Gamepads.Axis(index, axis);
}
