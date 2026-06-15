namespace AlvorKit.Script.BindgenReview;

/// <summary>Result returned by one bindgen review operation.</summary>
/// <param name="ExitCode">Process exit code to return to the caller.</param>
/// <param name="Lines">Console output lines to print in order.</param>
internal sealed record BindgenReviewResult(int ExitCode, IReadOnlyList<string> Lines)
{
    /// <summary>Creates a successful result with one output line.</summary>
    /// <param name="line">Line to print.</param>
    public static BindgenReviewResult Success(string line) =>
        new(0, [line]);

    /// <summary>Adds lines before the current output without changing the exit code.</summary>
    /// <param name="lines">Lines to prepend.</param>
    public BindgenReviewResult Prepend(params string[] lines) =>
        this with { Lines = [.. lines, .. Lines] };

    /// <summary>Adds one line after the current output without changing the exit code.</summary>
    /// <param name="line">Line to append.</param>
    public BindgenReviewResult Append(string line) =>
        this with { Lines = [.. Lines, line] };
}
