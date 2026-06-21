namespace AlvorKit.Script.BindgenReview;

/// <summary>Supported commands for disposable bindgen review snapshots.</summary>
internal enum BindgenReviewCommandKind
{
    /// <summary>Create a review directory and generate the before snapshot.</summary>
    Start,

    /// <summary>Generate the after snapshot for an existing review directory.</summary>
    After,

    /// <summary>Print the generated-code diff for an existing review directory.</summary>
    Diff,

    /// <summary>Delete an existing review directory after inspection.</summary>
    Clean,

    /// <summary>Generate the after snapshot, print the diff, and clean the review directory.</summary>
    Finish
}

/// <summary>Parsed command-line request for one bindgen review operation.</summary>
/// <param name="Kind">Requested operation.</param>
/// <param name="RepoRoot">Absolute repository root.</param>
/// <param name="Library">Native library selected by a start command.</param>
/// <param name="ReviewRoot">Existing review directory selected by non-start commands.</param>
/// <param name="CaseName">Optional human-readable case slug.</param>
/// <param name="Keep">Whether finish should keep the review directory after printing the diff.</param>
internal sealed record BindgenReviewCommand(
    BindgenReviewCommandKind Kind,
    string RepoRoot,
    string? Library,
    string? ReviewRoot,
    string? CaseName,
    bool Keep);
