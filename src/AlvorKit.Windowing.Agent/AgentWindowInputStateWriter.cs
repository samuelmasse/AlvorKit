namespace AlvorKit.Windowing;

/// <summary>Formats deterministic input state for the agent command line.</summary>
internal static class AgentWindowInputStateWriter
{
    /// <summary>Writes one stable input-state line to the supplied output.</summary>
    /// <param name="output">Output stream receiving the diagnostic line.</param>
    /// <param name="focus">Current focus state reported by the host.</param>
    /// <param name="mouse">Current simulated mouse position.</param>
    /// <param name="input">Tracked input values.</param>
    internal static void Write(TextWriter output, bool focus, Vec2 mouse, AgentWindowInputState input)
    {
        output.WriteLine(string.Format(
            CultureInfo.InvariantCulture,
            "input focus={0} mouse=<{1:0.###} {2:0.###}> keys=[{3}] buttons=[{4}] text=\"{5}\"",
            focus.ToString().ToLowerInvariant(),
            mouse.X,
            mouse.Y,
            string.Join(", ", input.HeldKeys),
            string.Join(", ", input.HeldMouseButtons.Select(Name)),
            Escape(input.PendingText)));
    }

    private static string Name(MouseButton button) =>
        button switch
        {
            MouseButton.Left => nameof(MouseButton.Left),
            MouseButton.Right => nameof(MouseButton.Right),
            MouseButton.Middle => nameof(MouseButton.Middle),
            _ => button.ToString()
        };

    private static string Escape(string text) => text.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);
}
