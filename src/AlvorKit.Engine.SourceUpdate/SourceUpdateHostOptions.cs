namespace AlvorKit;

/// <summary>Configures an explicitly allowlisted editable-source runtime bridge.</summary>
public sealed class SourceUpdateHostOptions
{
    /// <summary>Environment variable containing the immutable editable launch manifest path.</summary>
    public const string LaunchManifestVariable = "ALVORKIT_SOURCE_UPDATE_MANIFEST";

    private SourceUpdateHostOptions(
        Assembly assembly,
        SourceUpdateEditableLaunchManifest launch)
    {
        Assembly = assembly;
        Launch = launch;
    }

    /// <summary>Gets the exact loaded assembly allowed to receive metadata deltas.</summary>
    public Assembly Assembly { get; }

    /// <summary>Gets the immutable build and artifact identity captured before process launch.</summary>
    public SourceUpdateEditableLaunchManifest Launch { get; }

    /// <summary>Gets the maximum size of each metadata, IL, or PDB delta.</summary>
    public int MaximumDeltaBytes { get; init; } = 4 * 1024 * 1024;

    /// <summary>Loads and validates the editable launch manifest supplied by AlvorSense.</summary>
    public static SourceUpdateHostOptions FromEnvironment(Assembly assembly)
    {
        var path = Environment.GetEnvironmentVariable(LaunchManifestVariable);
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidOperationException(
                $"Source Update requires an editable launch manifest in {LaunchManifestVariable}.");
        }

        var launch = JsonSerializer.Deserialize<SourceUpdateEditableLaunchManifest>(
            File.ReadAllText(Path.GetFullPath(path), Encoding.UTF8),
            SourceUpdateJson.Options)
            ?? throw new InvalidOperationException("The Source Update launch manifest is invalid.");
        return new(assembly, launch);
    }

    internal static SourceUpdateHostOptions ForTest(
        Assembly assembly,
        SourceUpdateEditableLaunchManifest launch) =>
        new(assembly, launch);
}
