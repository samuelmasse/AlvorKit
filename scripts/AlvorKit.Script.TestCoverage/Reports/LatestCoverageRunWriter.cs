namespace AlvorKit.Script.TestCoverage;

/// <summary>Writes the non-authoritative pointer to the most recent coverage run.</summary>
internal static class LatestCoverageRunWriter
{
    /// <summary>Writes a small manifest pointing to this run's immutable artifacts.</summary>
    public static void Write(
        string repoRoot,
        CoverageOutputPaths output,
        DateTimeOffset generatedAt,
        CoverageOptions options,
        bool passed)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(output.LatestRunManifest)!);
        var artifacts = CoverageArtifactPaths.Create(repoRoot, output, options);
        var manifest = new
        {
            generatedAtUtc = generatedAt,
            output.RunId,
            passed,
            artifacts,
        };
        var json = JsonSerializer.Serialize(manifest, CoverageJson.Options);
        var temporaryPath = output.LatestRunManifest + "." + Guid.NewGuid().ToString("N") + ".tmp";

        File.WriteAllText(temporaryPath, json);
        File.Move(temporaryPath, output.LatestRunManifest, overwrite: true);
    }
}
