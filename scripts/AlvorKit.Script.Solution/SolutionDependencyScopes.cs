namespace AlvorKit;

/// <summary>Discovers shared dependency trees so SDK and package imports reuse native watch instances.</summary>
internal class SolutionDependencyScopes
{
    /// <summary>Resolves the selected SDK and NuGet's configured global package cache without assuming their locations.</summary>
    public async Task<IReadOnlyList<string>> ReadAsync(CancellationToken cancellation)
    {
        var sdk = SolutionGraph.Register();
        var start = new ProcessStartInfo("dotnet")
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        foreach (var argument in new[] { "nuget", "locals", "global-packages", "--list", "--force-english-output" })
            start.ArgumentList.Add(argument);

        using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not query NuGet's package cache.");
        var output = process.StandardOutput.ReadToEndAsync(cancellation);
        var error = process.StandardError.ReadToEndAsync(cancellation);

        try
        {
            await process.WaitForExitAsync(cancellation);
            var text = (await output).Trim();
            var diagnostic = (await error).Trim();

            if (process.ExitCode != 0)
                throw new InvalidOperationException($"NuGet package-cache query failed: {diagnostic}");

            const string prefix = "global-packages: ";

            if (!text.StartsWith(prefix, StringComparison.Ordinal) || !Path.IsPathFullyQualified(text[prefix.Length..]))
                throw new InvalidOperationException($"NuGet returned an invalid global package-cache location: {text}");

            return [sdk, Path.TrimEndingDirectorySeparator(text[prefix.Length..])];
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync(CancellationToken.None);
            }
        }
    }
}
