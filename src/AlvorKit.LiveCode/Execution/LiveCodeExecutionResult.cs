namespace AlvorKit.LiveCode;

/// <summary>Structured terminal response for one submitted command.</summary>
public sealed record LiveCodeExecutionResult(
    LiveCodeExecutionStatus Status,
    long ScopeId,
    string[] Lines,
    Dictionary<string, string> Values,
    double RunMilliseconds,
    string? Error,
    string? ExceptionType,
    string? StackTrace);
