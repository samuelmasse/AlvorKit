namespace AlvorKit.Script.Lint;

/// <summary>Completed process invocation result.</summary>
/// <param name="Command">Command that was executed.</param>
/// <param name="ExitCode">Process exit code, or 1 when the process could not be started.</param>
/// <param name="Output">Captured standard output and standard error.</param>
internal sealed record CommandResult(CommandSpec Command, int ExitCode, string Output);
