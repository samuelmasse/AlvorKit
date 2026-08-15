namespace AlvorKit;

/// <summary>Exact source result and identities produced by one numbered unified diff.</summary>
internal sealed record SourceUpdateDiffResult(
    string OldPath,
    string NewPath,
    string Source,
    string DiffSha256,
    string PreviousSourceSha256,
    string ResultSourceSha256);
