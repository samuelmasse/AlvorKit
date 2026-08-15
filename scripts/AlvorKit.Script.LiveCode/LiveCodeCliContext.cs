namespace AlvorKit;

/// <summary>
/// Shares console IO, workspace recording, source resolution, and stable JSON
/// presentation across LiveCode command groups.
/// </summary>
internal sealed class LiveCodeCliContext(
    TextReader input,
    TextWriter output,
    string repositoryRoot)
{
    /// <summary>Gets the live workspace store rooted at the invoking repository.</summary>
    internal LiveWorkspaceStore Workspaces { get; } = new(repositoryRoot);

    /// <summary>Gets the JSON contract shared by command input and output.</summary>
    internal JsonSerializerOptions Json { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>Reads source from a file or standard input.</summary>
    internal async Task<string> ReadSource(string? file) =>
        file is null
            ? await input.ReadToEndAsync()
            : await File.ReadAllTextAsync(Path.GetFullPath(file));

    /// <summary>Removes terminal encoding markers sometimes introduced by redirected input.</summary>
    internal string NormalizeRedirectedText(string source)
    {
        source = source.TrimStart();
        while (source.Length > 0 && (source[0] == '\uFEFF' || source[0] == '\uFFFD'))
            source = source[1..].TrimStart();
        if (source.StartsWith("\u00EF\u00BB\u00BF", StringComparison.Ordinal))
            source = source[3..].TrimStart();
        if (source.StartsWith("\u2229\u2557\u2510", StringComparison.Ordinal))
            source = source[3..].TrimStart();
        return source;
    }

    /// <summary>Writes bridge artifacts and returns their resolved filesystem identities.</summary>
    internal async Task<IReadOnlyCollection<LiveCodeSavedArtifact>> SaveArtifacts(
        IReadOnlyCollection<LiveCodeBridgeArtifact> artifacts)
    {
        var saved = new List<LiveCodeSavedArtifact>();
        foreach (var artifact in artifacts)
        {
            var path = Path.GetFullPath(artifact.Name);
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);
            await File.WriteAllBytesAsync(path, artifact.Data);
            saved.Add(new(
                artifact.Name,
                path,
                artifact.ContentType,
                artifact.Data.Length));
        }

        return saved;
    }

    /// <summary>Projects a bridge response with the identities of saved artifacts.</summary>
    internal static LiveCodeBridgeCliResult BridgeResult(
        LiveCodeBridgeExecutionResult result,
        IReadOnlyCollection<LiveCodeSavedArtifact> savedArtifacts) =>
        new(
            result.Status,
            result.Bridge,
            result.Version,
            result.Lines,
            result.Values,
            savedArtifacts,
            result.RunMilliseconds,
            result.Error,
            result.ExceptionType,
            result.StackTrace);

    /// <summary>Resolves and validates a workspace-owned submission source.</summary>
    internal LiveWorkspaceSource? WorkspaceSource(
        string? workspace,
        string? file,
        string area)
    {
        if (workspace is null)
            return null;
        if (file is null)
        {
            throw new InvalidOperationException(
                $"--file is required when --workspace records a {area} submission.");
        }
        return Workspaces.Source(workspace, file, area);
    }

    /// <summary>Requires a workspace to remain bound to the exact discovered process.</summary>
    internal void VerifyWorkspaceTarget(
        string? workspace,
        LiveCodeSessionManifest session)
    {
        if (workspace is null)
            return;
        var manifest = Workspaces.Read(workspace);
        if (manifest.LiveCode.SessionId != session.SessionId ||
            manifest.LiveCode.ProcessId != session.ProcessId ||
            manifest.LiveCode.StartedUtc != session.StartedUtc)
        {
            throw new InvalidOperationException(
                $"Live workspace '{manifest.Id}' belongs to session " +
                $"'{manifest.LiveCode.SessionId}' process {manifest.LiveCode.ProcessId}, " +
                $"not '{session.SessionId}' process {session.ProcessId}.");
        }
    }

    /// <summary>Records an operation when requested and writes its JSON result.</summary>
    internal void WriteRecorded<TRequest, TResult>(
        string? workspace,
        string operation,
        TRequest request,
        TResult result)
    {
        if (workspace is not null)
            Workspaces.Record(workspace, operation, request, result);
        Write(result);
    }

    /// <summary>Writes one value using the CLI's stable JSON contract.</summary>
    internal void Write<T>(T value) =>
        output.WriteLine(JsonSerializer.Serialize(value, Json));

    /// <summary>Selects one active scope by numeric ID, exact label, or type name.</summary>
    internal static LiveCodeScopeNode SelectScope(
        LiveCodeScopeGraph graph,
        string selector)
    {
        LiveCodeScopeNode[] matches;
        if (long.TryParse(selector, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
        {
            matches = [.. graph.Nodes.Where(node => node.Id == id)];
        }
        else
        {
            matches = [.. graph.Nodes.Where(node =>
                node.Label == selector
                || node.ScopeType == selector
                || node.ScopeType.EndsWith("." + selector, StringComparison.Ordinal))];
        }

        matches = [.. matches.Where(node =>
            node.Lifecycle == nameof(InjectorScopeLifecycle.Active))];
        return matches.Length switch
        {
            1 => matches[0],
            0 => throw new InvalidOperationException($"No active scope matches '{selector}'."),
            _ => throw new InvalidOperationException(
                $"Scope selector '{selector}' is ambiguous: " +
                $"{string.Join(", ", matches.Select(node => node.Id))}.")
        };
    }
}
