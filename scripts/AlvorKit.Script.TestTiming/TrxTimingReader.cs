namespace AlvorKit.Script.TestTiming;

/// <summary>Reads per-test timing information from MSTest TRX files.</summary>
internal sealed class TrxTimingReader
{
    /// <summary>Reads and orders test timings from several TRX files.</summary>
    /// <param name="trxPaths">TRX files to inspect.</param>
    public IReadOnlyList<TestTimingResult> ReadFiles(IEnumerable<string> trxPaths) =>
        trxPaths
            .SelectMany(ReadFile)
            .OrderByDescending(result => result.Duration)
            .ThenBy(result => result.TestName, StringComparer.Ordinal)
            .ToArray();

    /// <summary>Reads test timings from one TRX file.</summary>
    /// <param name="trxPath">TRX file to inspect.</param>
    public IReadOnlyList<TestTimingResult> ReadFile(string trxPath)
    {
        var document = XDocument.Load(trxPath);
        return document
            .Descendants()
            .Where(element => element.Name.LocalName == "UnitTestResult")
            .Select(element => ParseResult(element, trxPath))
            .Where(result => result is not null)
            .Cast<TestTimingResult>()
            .ToArray();
    }

    /// <summary>Parses one TRX result element when it has a valid duration.</summary>
    private static TestTimingResult? ParseResult(XElement element, string sourcePath)
    {
        var name = (string?)element.Attribute("testName");
        var durationText = (string?)element.Attribute("duration");
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(durationText))
            return null;

        if (!TimeSpan.TryParse(durationText, CultureInfo.InvariantCulture, out var duration))
            return null;

        var outcome = (string?)element.Attribute("outcome") ?? "";
        return new TestTimingResult(name, outcome, duration, sourcePath);
    }
}
