namespace AlvorKit.Windowing;

/// <summary>Runs deterministic mouse gestures through the existing agent event driver.</summary>
internal static class AgentWindowGestureDriver
{
    /// <summary>Default frame delta used by compact gesture commands.</summary>
    internal const double DefaultDelta = 0.016;

    /// <summary>Moves to a point and runs a one-frame left-button click.</summary>
    /// <param name="agent">Agent event driver to receive the gesture.</param>
    /// <param name="position">Client-space click position.</param>
    /// <param name="delta">Delta used for press and release update frames.</param>
    internal static void Click(AgentWindowEventDriver agent, Vec2 position, double delta = DefaultDelta)
    {
        agent.MoveMouse(position);
        agent.PressMouse(MouseButton.Left);
        agent.Update(delta);
        agent.ReleaseMouse(MouseButton.Left);
        agent.Update(delta);
    }

    /// <summary>Runs a deterministic left-button drag over a fixed number of updates.</summary>
    /// <param name="agent">Agent event driver to receive the gesture.</param>
    /// <param name="start">Client-space drag start position.</param>
    /// <param name="end">Client-space drag end position.</param>
    /// <param name="steps">Number of move updates between start and end.</param>
    /// <param name="delta">Delta used for each drag update and release update.</param>
    internal static void Drag(AgentWindowEventDriver agent, Vec2 start, Vec2 end, int steps, double delta)
    {
        AgentWindowState.ValidateCount(steps);
        if (steps == 0)
            throw new ArgumentOutOfRangeException(nameof(steps), "Drag steps must be positive.");

        agent.MoveMouse(start);
        agent.PressMouse(MouseButton.Left);
        agent.Advance(steps, delta, (end - start) / steps);
        agent.ReleaseMouse(MouseButton.Left);
        agent.Update(delta);
    }
}
