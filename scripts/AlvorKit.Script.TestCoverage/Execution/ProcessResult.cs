namespace AlvorKit.Script.TestCoverage;

/// <summary>Captured result from an external process invocation.</summary>
/// <param name="ExitCode">Process exit code.</param>
/// <param name="Output">Combined standard output and standard error text.</param>
internal sealed record ProcessResult(int ExitCode, string Output);
