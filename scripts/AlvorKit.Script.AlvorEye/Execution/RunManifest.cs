namespace AlvorKit.Script.AlvorEye;

/// <summary>Artifact manifest written for every AlvorEye run.</summary>
internal sealed class RunManifest
{
    /// <summary>Run id associated with the artifact directory.</summary>
    public required string RunId { get; init; }

    /// <summary>Session id, when the run created one.</summary>
    public string? SessionId { get; set; }

    /// <summary>Run output directory.</summary>
    public required string RunDirectory { get; init; }

    /// <summary>UTC timestamp when the run began.</summary>
    public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>Captured frame artifacts.</summary>
    public List<FrameArtifact> Frames { get; } = [];

    /// <summary>Action events and status records.</summary>
    public List<ManifestEvent> Events { get; } = [];
}

/// <summary>One captured frame recorded in the manifest.</summary>
internal sealed record FrameArtifact(string Name, string Path);

/// <summary>One action or lifecycle event recorded in the manifest.</summary>
internal sealed record ManifestEvent(DateTimeOffset Time, string Action, string Status, string? Path = null, string? Message = null);
