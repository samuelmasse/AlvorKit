namespace AlvorKit;

/// <summary>Appends exact request and result artifacts to a live workspace event stream.</summary>
/// <param name="manifests">Manifest and JSON persistence boundary.</param>
internal sealed class LiveWorkspaceEventLog(LiveWorkspaceManifestStore manifests)
{
    /// <summary>Manifest and JSON persistence boundary.</summary>
    private readonly LiveWorkspaceManifestStore manifests = manifests;

    /// <summary>Records one event and advances the authoritative event sequence.</summary>
    /// <typeparam name="TRequest">Logical request type.</typeparam>
    /// <typeparam name="TResult">Logical result type.</typeparam>
    /// <param name="manifest">Active workspace manifest.</param>
    /// <param name="operation">Safe operation-name segment.</param>
    /// <param name="request">Exact logical request value.</param>
    /// <param name="result">Exact logical result value.</param>
    /// <returns>The assigned sequence and artifact directory.</returns>
    internal LiveWorkspaceEventResult Record<TRequest, TResult>(
        LiveWorkspaceManifest manifest,
        string operation,
        TRequest request,
        TResult result)
    {
        var eventId = manifest.NextEventId;
        string eventPath;
        while (true)
        {
            eventPath = Path.Combine(
                manifest.WorkspacePath,
                "events",
                $"{eventId:0000}-{operation}");
            try
            {
                Directory.CreateDirectory(eventPath);
                if (Directory.GetFileSystemEntries(eventPath).Length == 0)
                    break;
            }
            catch (IOException)
            {
            }
            eventId++;
        }

        manifests.Write(Path.Combine(eventPath, "request.json"), request);
        manifests.Write(Path.Combine(eventPath, "result.json"), result);
        manifests.Save(manifest with
        {
            NextEventId = eventId + 1,
            UpdatedUtc = DateTimeOffset.UtcNow
        });
        return new(eventId, operation, eventPath);
    }
}
