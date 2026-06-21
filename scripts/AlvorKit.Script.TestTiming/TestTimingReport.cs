namespace AlvorKit.Script.TestTiming;

/// <summary>Paths and slow results produced by a timing report write.</summary>
/// <param name="totalCount">Total number of parsed test cases.</param>
/// <param name="slowResults">Test cases that exceeded the timing budget.</param>
/// <param name="markdownPath">Markdown report path.</param>
/// <param name="csvPath">CSV report path.</param>
internal sealed class TestTimingReport(int totalCount, IReadOnlyList<TestTimingResult> slowResults, string markdownPath, string csvPath)
{
    /// <summary>Total number of parsed test cases.</summary>
    public int TotalCount { get; } = totalCount;

    /// <summary>Test cases that exceeded the timing budget.</summary>
    public IReadOnlyList<TestTimingResult> SlowResults { get; } = slowResults;

    /// <summary>Markdown report path.</summary>
    public string MarkdownPath { get; } = markdownPath;

    /// <summary>CSV report path.</summary>
    public string CsvPath { get; } = csvPath;
}
