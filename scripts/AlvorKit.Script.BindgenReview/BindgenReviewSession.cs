namespace AlvorKit.Script.BindgenReview;

/// <summary>Resolved directories for one disposable bindgen review session.</summary>
/// <param name="Root">Absolute review directory.</param>
/// <param name="RelativeRoot">Repository-relative review directory.</param>
/// <param name="BeforeRoot">Absolute before-snapshot directory.</param>
/// <param name="BeforeRelativeRoot">Repository-relative before-snapshot directory.</param>
/// <param name="AfterRoot">Absolute after-snapshot directory.</param>
/// <param name="AfterRelativeRoot">Repository-relative after-snapshot directory.</param>
internal sealed record BindgenReviewSession(
    string Root,
    string RelativeRoot,
    string BeforeRoot,
    string BeforeRelativeRoot,
    string AfterRoot,
    string AfterRelativeRoot);
