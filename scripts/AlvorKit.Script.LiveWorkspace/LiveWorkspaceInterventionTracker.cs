namespace AlvorKit;

/// <summary>Maintains persistent live-process effects and enforces cleanup before workspace closure.</summary>
/// <param name="manifests">Manifest persistence boundary.</param>
internal sealed class LiveWorkspaceInterventionTracker(LiveWorkspaceManifestStore manifests)
{
    /// <summary>Manifest persistence boundary.</summary>
    private readonly LiveWorkspaceManifestStore manifests = manifests;

    /// <summary>Adds or replaces one intervention and persists the updated audit.</summary>
    /// <param name="manifest">Active workspace manifest.</param>
    /// <param name="intervention">Persistent effect to track.</param>
    /// <returns>The updated manifest.</returns>
    internal LiveWorkspaceManifest Upsert(
        LiveWorkspaceManifest manifest,
        LiveWorkspaceIntervention intervention)
    {
        var interventions = manifest.Interventions
            .Where(existing => existing.Id != intervention.Id)
            .Append(intervention)
            .OrderBy(existing => existing.Id, StringComparer.Ordinal)
            .ToArray();
        return Save(manifest with { Interventions = interventions });
    }

    /// <summary>Marks one intervention resolved and persists the updated audit.</summary>
    /// <param name="manifest">Active workspace manifest.</param>
    /// <param name="interventionId">Exact intervention identifier.</param>
    /// <returns>The updated manifest.</returns>
    internal LiveWorkspaceManifest Resolve(
        LiveWorkspaceManifest manifest,
        string interventionId)
    {
        var found = false;
        var interventions = manifest.Interventions
            .Select(intervention =>
            {
                if (intervention.Id != interventionId)
                    return intervention;
                found = true;
                return intervention with { State = LiveWorkspaceInterventionState.Resolved };
            })
            .ToArray();
        if (!found)
            throw new InvalidOperationException($"Workspace intervention was not found: {interventionId}");

        return Save(manifest with { Interventions = interventions });
    }

    /// <summary>Closes a workspace after proving that no intervention remains active.</summary>
    /// <param name="manifest">Active workspace manifest.</param>
    /// <returns>The closed manifest.</returns>
    internal LiveWorkspaceManifest Close(LiveWorkspaceManifest manifest)
    {
        var unresolved = manifest.Interventions
            .Where(intervention => intervention.State != LiveWorkspaceInterventionState.Resolved)
            .Select(intervention => intervention.Id)
            .ToArray();
        if (unresolved.Length > 0)
        {
            throw new InvalidOperationException(
                $"Live workspace has unresolved interventions: {string.Join(", ", unresolved)}.");
        }

        return Save(manifest with { Status = LiveWorkspaceStatus.Closed });
    }

    /// <summary>Persists an audit mutation with a fresh update timestamp.</summary>
    private LiveWorkspaceManifest Save(LiveWorkspaceManifest manifest)
    {
        var updated = manifest with { UpdatedUtc = DateTimeOffset.UtcNow };
        manifests.Save(updated);
        return updated;
    }
}
