namespace AlvorKit.Script.BindgenReview;

/// <summary>Manifest that marks a directory as a disposable bindgen review snapshot.</summary>
/// <param name="Library">Native library selected for generation.</param>
/// <param name="CaseName">Human-readable case name supplied by the agent.</param>
/// <param name="RelativeRoot">Repository-relative review directory.</param>
/// <param name="CreatedAt">UTC timestamp when the review directory was created.</param>
internal sealed record BindgenReviewManifest(
    string Library,
    string CaseName,
    string RelativeRoot,
    DateTimeOffset CreatedAt);
