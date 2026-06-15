namespace AlvorKit.Script.Lint;

/// <summary>Default process runner backed by System.Diagnostics.Process.</summary>
[ExcludeFromCodeCoverage]
internal sealed class ProcessRunner : IProcessRunner
{
    /// <summary>Runs a process while capturing output so parallel command logs stay readable.</summary>
    public async Task<CommandResult> RunAsync(CommandSpec command)
    {
        using var process = Start(command);
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return new(command, process.ExitCode, await outputTask + await errorTask);
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

        foreach (var arg in command.Arguments)
            startInfo.ArgumentList.Add(arg);

        return Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Could not start {command.FileName}.");
    }
}
