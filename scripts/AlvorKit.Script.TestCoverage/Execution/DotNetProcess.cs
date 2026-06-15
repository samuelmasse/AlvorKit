namespace AlvorKit.Script.TestCoverage;

/// <summary>Executes dotnet commands and captures combined process output.</summary>
internal static class DotNetProcess
{
    /// <summary>Runs dotnet with the supplied arguments from the repository root.</summary>
    public static async Task<ProcessResult> RunAsync(string repoRoot, IReadOnlyList<string> arguments)
    {
        using var process = new Process
        {
            StartInfo =
            {
                FileName = "dotnet",
                WorkingDirectory = repoRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            },
        };

        foreach (var argument in arguments)
            process.StartInfo.ArgumentList.Add(argument);

        process.Start();
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return new(process.ExitCode, await outputTask + await errorTask);
    }
}
