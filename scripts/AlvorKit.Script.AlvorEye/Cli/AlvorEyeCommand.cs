namespace AlvorKit.Script.AlvorEye;

/// <summary>Parsed command-line request for the AlvorEye coordinator.</summary>
/// <param name="Kind">The command kind to execute.</param>
/// <param name="RepoRoot">Repository root used for output defaults and relative paths.</param>
/// <param name="ScenarioPath">Scenario JSON path for run and session commands.</param>
/// <param name="SessionId">Persistent session id for handoff and resume commands.</param>
internal sealed record AlvorEyeCommand(
    AlvorEyeCommandKind Kind,
    string RepoRoot,
    string? ScenarioPath,
    string? SessionId);
