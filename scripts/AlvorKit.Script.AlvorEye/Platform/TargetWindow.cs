namespace AlvorKit.Script.AlvorEye;

/// <summary>Native target window selected for an AlvorEye run.</summary>
/// <param name="Handle">Native window handle.</param>
/// <param name="Title">Current window title.</param>
/// <param name="ProcessId">Owning process id.</param>
internal readonly record struct TargetWindow(nint Handle, string Title, int ProcessId);
