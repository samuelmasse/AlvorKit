namespace AlvorKit.Script.AgentLease;

/// <summary>Reads and writes advisory lease files under <c>out/agents</c>.</summary>
/// <param name="repoRoot">Repository root that owns the coordination directory.</param>
internal sealed class AgentLeaseRepository(string repoRoot)
{
    /// <summary>JSON serializer settings used for human-readable lease files.</summary>
    private static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    /// <summary>Root directory for active and stale advisory lease files.</summary>
    public string AgentsDirectory => Path.Combine(repoRoot, "out", "agents");

    /// <summary>Directory for short markdown notes when overlapping work is unavoidable.</summary>
    public string ConflictsDirectory => Path.Combine(AgentsDirectory, "conflicts");

    /// <summary>Reads all parseable lease JSON files from the coordination directory.</summary>
    public IReadOnlyList<AgentLease> ReadAll()
    {
        if (!Directory.Exists(AgentsDirectory))
            return [];

        return Directory
            .EnumerateFiles(AgentsDirectory, "*.json", SearchOption.TopDirectoryOnly)
            .Select(ReadLeaseFile)
            .OfType<AgentLease>()
            .OrderBy(lease => lease.Agent, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>Reads one lease by agent identifier, returning null when it does not exist.</summary>
    /// <param name="agent">Agent identifier whose lease should be read.</param>
    public AgentLease? Read(string agent)
    {
        var path = LeasePath(agent);
        return File.Exists(path) ? ReadLeaseFile(path) : null;
    }

    /// <summary>Writes or replaces the lease for its agent identifier.</summary>
    /// <param name="lease">Lease to persist as JSON.</param>
    public void Write(AgentLease lease)
    {
        Directory.CreateDirectory(AgentsDirectory);
        File.WriteAllText(LeasePath(lease.Agent), JsonSerializer.Serialize(lease, JsonOptions) + Environment.NewLine);
    }

    /// <summary>Deletes one lease by agent identifier.</summary>
    /// <param name="agent">Agent identifier whose lease file should be removed.</param>
    public bool Delete(string agent)
    {
        var path = LeasePath(agent);
        if (!File.Exists(path))
            return false;

        File.Delete(path);
        return true;
    }

    /// <summary>Writes a markdown conflict note and returns its absolute file path.</summary>
    /// <param name="agent">Agent writing the conflict note.</param>
    /// <param name="task">Current task summary for the note.</param>
    /// <param name="paths">Path claims involved in the overlap.</param>
    /// <param name="reason">Short reason the overlap cannot be avoided.</param>
    /// <param name="overlaps">Active overlapping leases to include for context.</param>
    /// <param name="now">Current UTC timestamp for the note filename and body.</param>
    public string WriteConflictNote(
        string agent,
        string task,
        IReadOnlyList<string> paths,
        string reason,
        IReadOnlyList<AgentLease> overlaps,
        DateTimeOffset now)
    {
        Directory.CreateDirectory(ConflictsDirectory);
        var path = Path.Combine(ConflictsDirectory, $"{now:yyyyMMdd-HHmmss}-{SafeFileName(agent)}.md");
        File.WriteAllLines(path, ConflictLines(agent, task, paths, reason, overlaps, now));
        return path;
    }

    /// <summary>Returns the absolute JSON file path for an agent identifier.</summary>
    /// <param name="agent">Agent identifier used to derive a safe filename.</param>
    private string LeasePath(string agent) => Path.Combine(AgentsDirectory, SafeFileName(agent) + ".json");

    /// <summary>Reads one lease file, ignoring invalid or empty files so stale coordination data cannot break the helper.</summary>
    /// <param name="path">Absolute path to a lease JSON file.</param>
    private static AgentLease? ReadLeaseFile(string path)
    {
        try
        {
            return JsonSerializer.Deserialize<AgentLease>(File.ReadAllText(path), JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>Builds the markdown body for a conflict note.</summary>
    /// <param name="agent">Agent writing the note.</param>
    /// <param name="task">Current task summary.</param>
    /// <param name="paths">Path claims involved in the conflict.</param>
    /// <param name="reason">Reason the overlap is unavoidable.</param>
    /// <param name="overlaps">Active overlapping leases.</param>
    /// <param name="now">Current UTC timestamp.</param>
    private static IEnumerable<string> ConflictLines(
        string agent,
        string task,
        IReadOnlyList<string> paths,
        string reason,
        IReadOnlyList<AgentLease> overlaps,
        DateTimeOffset now)
    {
        yield return "# Agent Conflict";
        yield return "";
        yield return $"- Agent: {agent}";
        yield return $"- Task: {task}";
        yield return $"- Time: {now:O}";
        yield return $"- Paths: {string.Join(", ", paths)}";
        yield return $"- Reason: {reason}";
        yield return "";
        yield return "## Active Overlaps";

        foreach (var overlap in overlaps)
            yield return $"- {overlap.Agent}: {overlap.Task} ({overlap.Mode}; {string.Join(", ", overlap.Paths)})";
    }

    /// <summary>Turns an agent identifier into a filesystem-safe filename stem.</summary>
    /// <param name="agent">Raw agent identifier.</param>
    private static string SafeFileName(string agent)
    {
        var builder = new StringBuilder(agent.Length);
        foreach (var character in agent)
            builder.Append(char.IsLetterOrDigit(character) || character is '.' or '_' or '-' ? character : '-');

        var safe = builder.ToString().Trim('-');
        return safe.Length == 0 ? "agent" : safe;
    }
}
