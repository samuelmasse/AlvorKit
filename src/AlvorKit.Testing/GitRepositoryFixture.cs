namespace AlvorKit;

/// <summary>Creates isolated Git metadata for tests that exercise repository file selection.</summary>
public static class GitRepositoryFixture
{
    /// <summary>Initializes an empty checkout without inheriting user ignore rules or requiring a commit identity.</summary>
    public static void Initialize(string root)
    {
        Directory.CreateDirectory(root);
        Run(root, "init", "--quiet", "--initial-branch=main");
        Run(root, "config", "core.excludesFile", "");
    }

    /// <summary>Runs fixture-local Git, leaving its files writable for temporary workspace cleanup.</summary>
    public static void Run(string root, params string[] arguments)
    {
        var start = new ProcessStartInfo("git")
        {
            WorkingDirectory = root,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        foreach (var argument in arguments)
            start.ArgumentList.Add(argument);

        using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start fixture Git command.");
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        process.WaitForExit();
        output.GetAwaiter().GetResult();

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"Fixture Git command failed: {error.GetAwaiter().GetResult().Trim()}");

        foreach (var file in Directory.EnumerateFiles(Path.Combine(root, ".git"), "*", SearchOption.AllDirectories))
            File.SetAttributes(file, File.GetAttributes(file) & ~FileAttributes.ReadOnly);
    }
}
