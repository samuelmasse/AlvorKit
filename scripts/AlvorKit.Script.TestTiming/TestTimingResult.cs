namespace AlvorKit.Script.TestTiming;

/// <summary>One test case duration parsed from TRX output.</summary>
/// <param name="testName">Display name of the test case.</param>
/// <param name="outcome">TRX outcome recorded for the test case.</param>
/// <param name="duration">Elapsed time recorded for the test case.</param>
/// <param name="sourcePath">TRX file that contained the result.</param>
internal sealed class TestTimingResult(string testName, string outcome, TimeSpan duration, string sourcePath)
{
    /// <summary>Display name of the test case.</summary>
    public string TestName { get; } = testName;

    /// <summary>TRX outcome recorded for the test case.</summary>
    public string Outcome { get; } = outcome;

    /// <summary>Elapsed time recorded for the test case.</summary>
    public TimeSpan Duration { get; } = duration;

    /// <summary>TRX file that contained the result.</summary>
    public string SourcePath { get; } = sourcePath;
}
