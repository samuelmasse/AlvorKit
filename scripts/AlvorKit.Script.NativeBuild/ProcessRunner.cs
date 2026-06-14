namespace AlvorKit.Script.NativeBuild;

/// <summary>Default process runner backed by System.Diagnostics.Process.</summary>
internal sealed class ProcessRunner : IProcessRunner
{
    /// <summary>Runs a process and streams its output to the current console.</summary>
    public async Task RunAsync(CommandSpec command)
    {
        if (command.CreateWorkingDirectory && command.WorkingDirectory is not null)
            Directory.CreateDirectory(command.WorkingDirectory);

        Console.WriteLine("> " + CommandText.Display(command));
        var process = ProcessLauncher.Start(command, redirect: false);
        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"{command.FileName} failed with exit code {process.ExitCode}.");
    }

    /// <summary>Runs a process and captures standard output for parser-based checks.</summary>
    public async Task<string> CaptureAsync(CommandSpec command)
    {
        var process = ProcessLauncher.Start(command, redirect: true);
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"{command.FileName} failed with exit code {process.ExitCode}: {error}");
        return output;
    }
}
