namespace AlvorKit.LiveCode;

/// <summary>Wraps one frozen command result with heartbeat evidence captured before and after execution.</summary>
public sealed record LiveCodeFrozenInspectionExecutionResult(
    LiveCodeFrozenInspectionSnapshot Started,
    LiveCodeFrozenInspectionSnapshot Completed,
    LiveCodeExecutionResult Execution);
