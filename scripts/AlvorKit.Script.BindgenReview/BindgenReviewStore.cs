namespace AlvorKit.Script.BindgenReview;

/// <summary>Reads, writes, and deletes disposable bindgen review session metadata.</summary>
internal sealed class BindgenReviewStore
{
    /// <summary>Manifest file name that marks directories safe for helper cleanup.</summary>
    private const string ManifestFileName = ".bindgen-review.json";

    /// <summary>JSON options used for stable, readable manifest files.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <summary>Writes the manifest for a newly created review session.</summary>
    /// <param name="session">Review session directories.</param>
    /// <param name="manifest">Manifest content to write.</param>
    public void WriteManifest(BindgenReviewSession session, BindgenReviewManifest manifest)
    {
        Directory.CreateDirectory(session.Root);
        File.WriteAllText(ManifestPath(session), JsonSerializer.Serialize(manifest, JsonOptions));
    }

    /// <summary>Reads the manifest for an existing review session.</summary>
    /// <param name="session">Review session directories.</param>
    public BindgenReviewManifest ReadManifest(BindgenReviewSession session)
    {
        var path = ManifestPath(session);
        if (!File.Exists(path))
            throw new InvalidOperationException($"Review manifest not found at {path}.");

        return JsonSerializer.Deserialize<BindgenReviewManifest>(File.ReadAllText(path), JsonOptions)
            ?? throw new InvalidOperationException($"Review manifest at {path} is empty.");
    }

    /// <summary>Deletes a review session only after confirming its manifest exists.</summary>
    /// <param name="session">Review session directories.</param>
    public void Delete(BindgenReviewSession session)
    {
        _ = ReadManifest(session);
        Directory.Delete(session.Root, recursive: true);
    }

    /// <summary>Returns the absolute manifest path for a review session.</summary>
    private static string ManifestPath(BindgenReviewSession session) =>
        Path.Combine(session.Root, ManifestFileName);
}
