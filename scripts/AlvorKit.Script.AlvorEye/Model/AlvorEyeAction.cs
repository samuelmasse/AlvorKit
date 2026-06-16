namespace AlvorKit.Script.AlvorEye;

/// <summary>One planned timeline or session action.</summary>
internal sealed class AlvorEyeAction
{
    /// <summary>The action kind.</summary>
    public required AlvorEyeActionKind Kind { get; init; }

    /// <summary>Optional human-readable action name or capture file stem.</summary>
    public string? Name { get; init; }

    /// <summary>Delay or duration attached to this action.</summary>
    public TimeSpan Delay { get; init; }

    /// <summary>Keyboard key name for key actions.</summary>
    public string? Key { get; init; }

    /// <summary>Text for text input actions.</summary>
    public string? Text { get; init; }

    /// <summary>Window-relative x coordinate for mouse actions.</summary>
    public int X { get; init; }

    /// <summary>Window-relative y coordinate for mouse actions.</summary>
    public int Y { get; init; }

    /// <summary>Window-relative destination x coordinate for drags.</summary>
    public int ToX { get; init; }

    /// <summary>Window-relative destination y coordinate for drags.</summary>
    public int ToY { get; init; }

    /// <summary>Mouse button name for click and drag actions.</summary>
    public string? Button { get; init; }

    /// <summary>Whether handoff should capture before freezing.</summary>
    public bool CaptureBeforeFreeze { get; init; } = true;

    /// <summary>Whether handoff should capture after freezing.</summary>
    public bool CaptureAfterFreeze { get; init; } = true;

    /// <summary>Optional frame path to compare during basic analysis.</summary>
    public string? CompareTo { get; init; }

    /// <summary>Optional hex color to search for during basic analysis.</summary>
    public string? Color { get; init; }
}
