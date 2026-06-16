namespace AlvorKit.Script.AlvorEye;

/// <summary>Identifies an action in an AlvorEye scenario timeline or JSONL session command.</summary>
internal enum AlvorEyeActionKind
{
    /// <summary>Wait for a duration without sending input.</summary>
    Wait,

    /// <summary>Capture the current target window.</summary>
    Capture,

    /// <summary>Press and release one key.</summary>
    Key,

    /// <summary>Press and hold one key.</summary>
    KeyDown,

    /// <summary>Release one key.</summary>
    KeyUp,

    /// <summary>Type Unicode text.</summary>
    Text,

    /// <summary>Move the mouse cursor relative to the target window.</summary>
    MouseMove,

    /// <summary>Click a mouse button relative to the target window.</summary>
    MouseClick,

    /// <summary>Drag a mouse button between two target-window-relative points.</summary>
    MouseDrag,

    /// <summary>Freeze the target and hand control back to the agent.</summary>
    Handoff,

    /// <summary>Resume a frozen target.</summary>
    Resume,

    /// <summary>Run simple image analysis on a captured frame.</summary>
    AnalyzeBasic,
}
