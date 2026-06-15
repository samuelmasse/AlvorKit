namespace AlvorKit.Script.BindgenReview;

/// <summary>Process invocation planned by the bindgen review coordinator.</summary>
/// <param name="FileName">Executable file name or absolute path.</param>
/// <param name="Arguments">Arguments passed without shell expansion.</param>
/// <param name="WorkingDirectory">Working directory for the process.</param>
internal sealed record CommandSpec(string FileName, IReadOnlyList<string> Arguments, string WorkingDirectory);

/// <summary>Captured child-process result.</summary>
/// <param name="ExitCode">Exit code returned by the child process.</param>
/// <param name="Output">Combined standard output and standard error.</param>
internal sealed record ProcessResult(int ExitCode, string Output);

/// <summary>Runs external commands for bindgen review operations.</summary>
internal interface IProcessRunner
{
    /// <summary>Runs a process while capturing text output.</summary>
    /// <param name="command">Command to run.</param>
    Task<ProcessResult> RunAsync(CommandSpec command);
}

/// <summary>Default process runner backed by System.Diagnostics.Process.</summary>
[ExcludeFromCodeCoverage]
internal sealed class SystemProcessRunner : IProcessRunner
{
    /// <inheritdoc />
    public async Task<ProcessResult> RunAsync(CommandSpec command)
    {
        using var process = Start(command);
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return new(process.ExitCode, await outputTask + await errorTask);
    }

    /// <summary>Starts a process with shell expansion disabled.</summary>
    private static Process Start(CommandSpec command)
    {
        var startInfo = new ProcessStartInfo(command.FileName)
        {
            WorkingDirectory = command.WorkingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        foreach (var argument in command.Arguments)
            startInfo.ArgumentList.Add(argument);

        return Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Could not start {command.FileName}.");
    }
}
