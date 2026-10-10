namespace AlvorKit;

/// <summary>Process outcome with complete stdout and stderr, independent of the child's timed regions.</summary>
public record BenchProcessResult(string Output, string Errors, int ExitCode);
