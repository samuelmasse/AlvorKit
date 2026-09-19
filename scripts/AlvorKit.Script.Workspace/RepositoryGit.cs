namespace AlvorKit;

/// <summary>Runs read-only Git queries with lossless output and explicit process failures.</summary>
public static class RepositoryGit
{
    /// <summary>Returns UTF-8 output without trimming delimiters or interpreting paths as shell commands.</summary>
    public static string Read(string root, params string[] arguments)
    {
        var start = new ProcessStartInfo("git")
        {
            WorkingDirectory = root,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };

        foreach (var argument in arguments)
            start.ArgumentList.Add(argument);

        using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start Git.");
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"Git query failed in '{root}': {error.GetAwaiter().GetResult().Trim()}");

        return output.GetAwaiter().GetResult();
    }
}
