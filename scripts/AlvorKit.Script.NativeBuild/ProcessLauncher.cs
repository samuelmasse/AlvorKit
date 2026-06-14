namespace AlvorKit.Script.NativeBuild;

/// <summary>Creates configured Process instances for command specs.</summary>
internal static class ProcessLauncher
{
    /// <summary>Starts a process with shell expansion disabled.</summary>
    public static Process Start(CommandSpec command, bool redirect)
    {
        var startInfo = new ProcessStartInfo(command.FileName)
        {
            UseShellExecute = false,
            RedirectStandardOutput = redirect,
            RedirectStandardError = redirect
        };
        if (command.WorkingDirectory is not null)
            startInfo.WorkingDirectory = command.WorkingDirectory;
        foreach (var arg in command.Arguments)
            startInfo.ArgumentList.Add(arg);
        return Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Could not start {command.FileName}.");
    }
}
