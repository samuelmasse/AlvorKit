namespace AlvorKit.Windowing;

/// <summary>Reads polled gamepad state for a window loop.</summary>
internal sealed class WindowGamepads(IWindowHost window)
{
    /// <summary>Returns whether a gamepad is connected at the slot.</summary>
    internal bool IsConnected(int index) => window.TryGetGamepad(index, out _);

    /// <summary>Returns whether any of the requested buttons are down on the gamepad slot.</summary>
    internal bool IsButtonDown(int index, GamepadButtons button) =>
        window.TryGetGamepad(index, out var state) && state.IsButtonDown(button);

    /// <summary>Returns the axis value for the gamepad slot, or 0 when disconnected.</summary>
    internal float Axis(int index, GamepadAxis axis) =>
        window.TryGetGamepad(index, out var state) ? state.Axis(axis) : 0f;
}
