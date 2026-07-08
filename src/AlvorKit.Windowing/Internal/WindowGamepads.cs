namespace AlvorKit.Windowing;

/// <summary>Reads polled gamepad state for a window loop.</summary>
internal sealed class WindowGamepads
{
    private const int Count = 16;

    private readonly IWindowHost window;
    private readonly GamepadState[] states = new GamepadState[Count];
    private readonly bool[] connected = new bool[Count];
    private readonly int[] ticks = new int[Count];
    private int tick = 1;

    /// <summary>Creates a per-loop lazy gamepad cache around the host window.</summary>
    internal WindowGamepads(IWindowHost window)
    {
        this.window = window;
    }

    /// <summary>Starts a new polling tick; slots are queried lazily when read.</summary>
    internal void Tick()
    {
        tick++;
        if (tick == int.MaxValue)
        {
            Array.Clear(ticks);
            tick = 1;
        }
    }

    /// <summary>Returns whether a gamepad is connected at the slot.</summary>
    internal bool IsConnected(int index) => TryGet(index, out _);

    /// <summary>Returns whether any of the requested buttons are down on the gamepad slot.</summary>
    internal bool IsButtonDown(int index, GamepadButtons button) => TryGet(index, out var state) && state.IsButtonDown(button);

    /// <summary>Returns the axis value for the gamepad slot, or 0 when disconnected.</summary>
    internal float Axis(int index, GamepadAxis axis) => TryGet(index, out var state) ? state.Axis(axis) : 0f;

    private bool TryGet(int index, out GamepadState state)
    {
        if ((uint)index >= Count)
        {
            state = default;
            return false;
        }

        if (ticks[index] != tick)
        {
            connected[index] = window.TryGetGamepad(index, out states[index]);
            ticks[index] = tick;
        }

        state = states[index];
        return connected[index];
    }
}
