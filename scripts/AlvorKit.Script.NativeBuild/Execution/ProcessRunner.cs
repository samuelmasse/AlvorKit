namespace AlvorKit.Script.NativeBuild;

/// <summary>Default process runner backed by System.Diagnostics.Process.</summary>
[ExcludeFromCodeCoverage]
internal sealed class ProcessRunner : IProcessRunner
{
    /// <summary>Runs a process and streams its output to the current console.</summary>
    public async Task RunAsync(CommandSpec command)
    {
        if (command.CreateWorkingDirectory && command.WorkingDirectory is not null)
            Directory.CreateDirectory(command.WorkingDirectory);

        Console.WriteLine("> " + CommandText.Display(command));
        var process = Start(command, redirect: false);
        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"{command.FileName} failed with exit code {process.ExitCode}.");
    }

    /// <summary>Runs a process and captures standard output for parser-based checks.</summary>
    public async Task<string> CaptureAsync(CommandSpec command)
    {
        var process = Start(command, redirect: true);
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"{command.FileName} failed with exit code {process.ExitCode}: {error}");
        return output;
    }

    /// <summary>Starts a process with shell expansion disabled.</summary>
    private Process Start(CommandSpec command, bool redirect)
    {
        var startInfo = CreateStartInfo(command, redirect);
        return Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Could not start {command.FileName}.");
    }

    /// <summary>Creates a shell-free process start configuration for one command.</summary>
    internal ProcessStartInfo CreateStartInfo(CommandSpec command, bool redirect)
    {
        var startInfo = new ProcessStartInfo(command.FileName)
        {
            UseShellExecute = false,
            RedirectStandardOutput = redirect,
            RedirectStandardError = redirect
        };
        if (command.WorkingDirectory is not null)
            startInfo.WorkingDirectory = command.WorkingDirectory;
        if (command.Environment is not null)
            foreach (var (name, value) in command.Environment)
                startInfo.Environment[name] = value;
        foreach (var arg in command.Arguments)
            startInfo.ArgumentList.Add(arg);
        return startInfo;
    }
}
