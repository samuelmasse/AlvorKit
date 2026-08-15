namespace AlvorKit;

/// <summary>Creates, records, audits, and closes ignored agent workspaces for live AlvorKit processes.</summary>
public sealed class LiveWorkspaceStore
{
    /// <summary>Current workspace schema written to <c>session.json</c>.</summary>
    public const int SchemaVersion = 2;

    /// <summary>Repository-backed template family for human-readable workspace files.</summary>
    private static readonly RepositoryTemplateSet Templates =
        RepositoryTemplates.ForArea(typeof(LiveWorkspaceStore), "live-session");

    /// <summary>Absolute repository root that owns the ignored workspace directory.</summary>
    private readonly string repositoryRoot;

    /// <summary>Versioned manifest and JSON persistence boundary.</summary>
    private readonly LiveWorkspaceManifestStore manifests = new();

    /// <summary>Append-only logical operation recorder.</summary>
    private readonly LiveWorkspaceEventLog events;

    /// <summary>Persistent-effect audit and closure policy.</summary>
    private readonly LiveWorkspaceInterventionTracker interventions;

    /// <summary>Creates a store rooted in one repository.</summary>
    /// <param name="repositoryRoot">Repository root that owns <c>tmp/live</c>.</param>
    public LiveWorkspaceStore(string repositoryRoot)
    {
        this.repositoryRoot = Path.GetFullPath(repositoryRoot);
        events = new(manifests);
        interventions = new(manifests);
    }

    /// <summary>Gets the default ignored live-workspace root beneath this repository.</summary>
    public string Root => Path.Combine(repositoryRoot, "tmp", "live");

    /// <summary>Creates one workspace and its standard source, event, baseline, and evidence directories.</summary>
    public LiveWorkspaceManifest Create(
        string id,
        string purpose,
        LiveWorkspaceTarget target,
        string? alvorSenseSessionId,
        long baselineGraphRevision)
    {
        id = SafeName(id, nameof(id));
        if (string.IsNullOrWhiteSpace(purpose))
            throw new ArgumentException("Workspace purpose must not be blank.", nameof(purpose));

        var path = Path.Combine(Root, id);
        if (Directory.Exists(path))
            throw new InvalidOperationException($"Live workspace already exists: {path}");

        Directory.CreateDirectory(path);
        foreach (var child in new[]
        {
            "lc",
            "source",
            Path.Combine("source", "diffs"),
            Path.Combine("source", "evidence"),
            "bridge",
            "puppet",
            "events",
            "evidence",
            "baseline"
        })
            Directory.CreateDirectory(Path.Combine(path, child));

        var now = DateTimeOffset.UtcNow;
        var manifest = new LiveWorkspaceManifest(
            SchemaVersion,
            id,
            purpose.Trim(),
            LiveWorkspaceStatus.Active,
            repositoryRoot,
            path,
            now,
            now,
            target,
            EmptyToNull(alvorSenseSessionId),
            baselineGraphRevision,
            1,
            []);
        manifests.Save(manifest);
        File.WriteAllText(
            Path.Combine(path, "SESSION.md"),
            Templates.Render(
                "SESSION.md.tmpl",
                ("WorkspaceId", manifest.Id),
                ("Purpose", manifest.Purpose),
                ("Status", manifest.Status.ToString()),
                ("CreatedUtc", manifest.CreatedUtc.ToString("O", CultureInfo.InvariantCulture)),
                ("LiveCodeName", target.Name),
                ("LiveCodeSessionId", target.SessionId),
                ("ProcessId", target.ProcessId.ToString(CultureInfo.InvariantCulture)),
                ("AlvorSenseSessionId", manifest.AlvorSenseSessionId ?? "not associated"),
                ("BaselineGraphRevision", baselineGraphRevision.ToString(CultureInfo.InvariantCulture))),
            Encoding.UTF8);
        return manifest;
    }

    /// <summary>Reads one workspace by safe id or explicit workspace directory.</summary>
    public LiveWorkspaceManifest Read(string selector)
    {
        var path = Resolve(selector);
        var manifestPath = Path.Combine(path, "session.json");
        if (!File.Exists(manifestPath))
            throw new InvalidOperationException($"Live workspace manifest was not found: {manifestPath}");
        return manifests.Read(manifestPath);
    }

    /// <summary>Writes a named baseline snapshot beneath an active workspace.</summary>
    public string WriteBaseline<T>(LiveWorkspaceManifest manifest, string name, T value)
    {
        RequireActive(manifest);
        var path = Path.Combine(manifest.WorkspacePath, "baseline", SafeFileName(name));
        manifests.Write(path, value);
        return path;
    }

    /// <summary>Records exact logical request and result values for one operation.</summary>
    public LiveWorkspaceEventResult Record<TRequest, TResult>(
        string selector,
        string operation,
        TRequest request,
        TResult result)
    {
        var manifest = Read(selector);
        RequireActive(manifest);
        var operationName = SafeName(operation, nameof(operation));
        return events.Record(manifest, operationName, request, result);
    }

    /// <summary>Returns an absolute path and SHA-256 identity for an existing submission file.</summary>
    public LiveWorkspaceSource Source(string selector, string sourcePath, string expectedArea)
    {
        var manifest = Read(selector);
        RequireActive(manifest);
        var area = SafeName(expectedArea, nameof(expectedArea));
        var allowedRoot = Path.GetFullPath(Path.Combine(manifest.WorkspacePath, area)) +
            Path.DirectorySeparatorChar;
        var path = Path.GetFullPath(sourcePath);
        if (!path.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Submission '{path}' must be inside the workspace '{area}' directory.");
        }
        if (!File.Exists(path))
            throw new FileNotFoundException($"Submission file was not found: {path}", path);

        using var stream = File.OpenRead(path);
        return new(path, Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant(), stream.Length);
    }

    /// <summary>Adds or replaces one persistent intervention in the workspace audit.</summary>
    public LiveWorkspaceManifest UpsertIntervention(
        string selector,
        LiveWorkspaceIntervention intervention)
    {
        var manifest = Read(selector);
        RequireActive(manifest);
        return interventions.Upsert(manifest, intervention);
    }

    /// <summary>Marks one tracked intervention resolved after its cleanup was proved.</summary>
    public LiveWorkspaceManifest ResolveIntervention(string selector, string interventionId)
    {
        var manifest = Read(selector);
        RequireActive(manifest);
        return interventions.Resolve(manifest, interventionId);
    }

    /// <summary>Associates or replaces the AlvorSense session used for user-visible evidence.</summary>
    public LiveWorkspaceManifest AssociateAlvorSense(string selector, string? sessionId)
    {
        var manifest = Read(selector);
        RequireActive(manifest);
        var updated = manifest with
        {
            AlvorSenseSessionId = EmptyToNull(sessionId),
            UpdatedUtc = DateTimeOffset.UtcNow
        };
        manifests.Save(updated);
        return updated;
    }

    /// <summary>Closes a workspace only after every persistent intervention is resolved.</summary>
    public LiveWorkspaceManifest Close(string selector)
    {
        var manifest = Read(selector);
        RequireActive(manifest);
        return interventions.Close(manifest);
    }

    /// <summary>Resolves a safe workspace id or explicit workspace directory.</summary>
    private string Resolve(string selector)
    {
        if (string.IsNullOrWhiteSpace(selector))
            throw new ArgumentException("Workspace selector must not be blank.", nameof(selector));
        return Path.IsPathRooted(selector) || selector.Contains(Path.DirectorySeparatorChar) ||
            selector.Contains(Path.AltDirectorySeparatorChar)
            ? Path.GetFullPath(selector)
            : Path.Combine(Root, SafeName(selector, nameof(selector)));
    }

    /// <summary>Validates one directory-name segment used in workspace paths.</summary>
    private static string SafeName(string value, string parameterName)
    {
        value = value.Trim();
        if (value.Length == 0 ||
            value is "." or ".." ||
            value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            value.Contains(Path.DirectorySeparatorChar) ||
            value.Contains(Path.AltDirectorySeparatorChar))
        {
            throw new ArgumentException(
                "Value must be one safe directory-name segment.",
                parameterName);
        }
        return value;
    }

    /// <summary>Validates one filename used beneath the baseline directory.</summary>
    private static string SafeFileName(string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            Path.GetFileName(value) != value ||
            value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new ArgumentException("Baseline name must be one safe filename.", nameof(value));
        }
        return value;
    }

    /// <summary>Rejects mutations against a closed workspace.</summary>
    private static void RequireActive(LiveWorkspaceManifest manifest)
    {
        if (manifest.Status != LiveWorkspaceStatus.Active)
            throw new InvalidOperationException($"Live workspace '{manifest.Id}' is closed.");
    }

    /// <summary>Normalizes optional user text to either a trimmed value or <see langword="null"/>.</summary>
    private static string? EmptyToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
