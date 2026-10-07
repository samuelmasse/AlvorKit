namespace AlvorKit;

/// <summary>Prints benchmark hierarchy, samples, comparisons, and allocation summaries.</summary>
[Bench]
public class BenchConsole
{
    /// <summary>Spaces used for each displayed hierarchy level.</summary>
    private const int IndentWidth = 2;
    /// <summary>Compact label identifying a retained-sample mean.</summary>
    private const string MeanLabel = "m";

    /// <summary>Renderer for owned native-memory snapshots.</summary>
    private readonly BenchMemoryConsole memory = new();
    /// <summary>Terminal-dependent wrapping and color operations.</summary>
    private readonly BenchTerminal terminal = new();

    /// <summary>Renders a native-memory snapshot or its retained mean.</summary>
    public void WriteMemoryTree(BenchMemoryUsage usage, int depth, bool mean) => memory.Write(usage, depth, mean);

    /// <summary>Starts the human-readable report with the suite name.</summary>
    public void WriteSuite(string name)
    {
        Console.WriteLine(name);
        Console.WriteLine();
    }

    /// <summary>Prints selected paths, descriptions, and comparison roles without execution.</summary>
    public void WriteCatalog(BenchCatalog catalog, BenchCase[] benchCases)
    {
        Console.WriteLine(catalog.SuiteName);
        Console.WriteLine($"{benchCases.Length:N0} runnable benchmark{(benchCases.Length == 1 ? "" : "s")}");
        Console.WriteLine();

        string[] previousSegments = [];

        foreach (var benchCase in benchCases)
        {
            var caseDepth = WriteCase(benchCase, previousSegments, out var segments);
            WriteComparison(benchCase, caseDepth + 1);
            previousSegments = segments;
        }
    }

    /// <summary>Reports command errors on standard error.</summary>
    public void WriteError(string message) => Console.Error.WriteLine($"Error: {message}");

    /// <summary>Reports the absolute path of a completed export.</summary>
    public void WriteJsonPath(string path) => Console.WriteLine($"JSON: {path}");

    /// <summary>Writes a complete JSON document without human-readable decoration.</summary>
    public void WriteJson(string json) => Console.WriteLine(json);

    /// <summary>Aligns all run numbers using the complete measurement count.</summary>
    public int GetRunLabelWidth(int runCount) => runCount.ToString().Length;

    /// <summary>Prints only hierarchy segments not shared with the previous case.</summary>
    public int WriteCase(BenchCase benchCase, ReadOnlySpan<string> previousSegments, out string[] segments)
    {
        segments = benchCase.Id.Split('/');
        var commonDepth = CommonDepth(previousSegments, segments);
        var caseDepth = segments.Length - 1;

        for (var depth = commonDepth; depth <= caseDepth; depth++)
            WriteNode(segments[depth], SegmentDescription(benchCase, depth, caseDepth), depth);

        return caseDepth;
    }

    /// <summary>Prints a node label and wraps its optional description.</summary>
    public void WriteNode(string name, string description, int depth)
    {
        Console.Write(BenchFormatting.Indent(depth));
        terminal.WriteLabel(name, depth);

        if (description.Length == 0)
        {
            Console.WriteLine();
            return;
        }

        Console.Write(": ");
        terminal.WriteWrapped(description, depth * IndentWidth + name.Length + 2);
    }

    /// <summary>Separates completed benchmark groups.</summary>
    public void WriteBlankLine() => Console.WriteLine();

    /// <summary>Updates an interactive progress line; redirected output suppresses transient rows.</summary>
    [ExcludeFromCodeCoverage(Justification = "Interactive cursor updates require a real terminal.")]
    public int WriteTransient(BenchResult result, int runNumber, int labelWidth, int depth, int previousLength)
    {
        if (Console.IsOutputRedirected)
            return previousLength;

        var label = runNumber.ToString().PadLeft(labelWidth);
        var lineLength = SampleLength(result, label, depth, "");
        Console.Write('\r');
        WriteSample(result, label, depth, "", false);

        if (lineLength < previousLength)
            Console.Write(new string(' ', previousLength - lineLength));

        return lineLength;
    }

    /// <summary>Prints a retained sample and clears any preceding transient row.</summary>
    public void WriteRetained(BenchResult result, int runNumber, int labelWidth, int depth, int previousLineLength)
    {
        var label = runNumber.ToString().PadLeft(labelWidth);
        WriteFinalSample(result, label, depth, "", previousLineLength);
    }

    /// <summary>Prints the retained mean with an optional elapsed-time comparison.</summary>
    public void WriteMean(
        BenchResult result,
        int labelWidth,
        int depth,
        double? baselineMultiplier,
        int previousLineLength)
    {
        var comparison = baselineMultiplier.HasValue ? Comparison(baselineMultiplier.Value) : "";
        WriteFinalSample(result, MeanLabel.PadLeft(labelWidth), depth, comparison, previousLineLength);
    }

    /// <summary>Replaces transient output without leaving trailing characters.</summary>
    private void WriteFinalSample(
        BenchResult result,
        string label,
        int depth,
        string suffix,
        int previousLineLength)
    {
        if (previousLineLength == 0)
        {
            WriteSample(result, label, depth, suffix, true);
            return;
        }

        var lineLength = SampleLength(result, label, depth, suffix);
        Console.Write('\r');
        WriteSample(result, label, depth, suffix, false);

        if (lineLength < previousLineLength)
            Console.Write(new string(' ', previousLineLength - lineLength));

        Console.WriteLine();
    }

    /// <summary>Prints one sample with aligned time, throughput, and allocation columns.</summary>
    private void WriteSample(BenchResult result, string label, int depth, string suffix, bool newLine)
    {
        Console.Write(BenchFormatting.Indent(depth));
        terminal.WriteLabel(label, depth);
        Console.Write(SampleBody(result));
        Console.Write(suffix);

        if (newLine)
            Console.WriteLine();
    }

    /// <summary>Computes the printable row length for clearing prior transient output.</summary>
    private int SampleLength(BenchResult result, string label, int depth, string suffix) =>
        depth * IndentWidth + label.Length + SampleBody(result).Length + suffix.Length;

    /// <summary>Formats timing and supplied allocation counters without hiding tiny per-operation values.</summary>
    private string SampleBody(BenchResult result)
    {
        var rate = result.OperationsPerSecond.ToString(result.RateFormat);
        var memory = result.UnmanagedBytes.HasValue ? $"  {BenchFormatting.FormatBytes(result.UnmanagedBytes.Value),12}" : "";
        var allocated = result.WorkloadAllocatedBytes.HasValue
            ? $"  {(double)result.WorkloadAllocatedBytes.Value / result.OperationCount:G4} B/op" : "";
        return $": {result.Elapsed.TotalMilliseconds,8:N2} ms  {rate,14} {result.Unit}/s{memory}{allocated}";
    }

    /// <summary>Expresses elapsed-time ratios as faster or slower relative to the baseline.</summary>
    private string Comparison(double multiplier)
    {
        var percent = Math.Abs(multiplier - 1) * 100;
        var direction = multiplier >= 1 ? "slower" : "faster";
        return $"  {multiplier:N2}x  {percent:N2}% {direction}";
    }

    /// <summary>Uses authored segment descriptions, or the leaf description for flat callers.</summary>
    private string SegmentDescription(BenchCase benchCase, int depth, int caseDepth)
    {
        if (depth < benchCase.SegmentDescriptions.Length)
            return benchCase.SegmentDescriptions[depth];

        return depth == caseDepth ? benchCase.Description : "";
    }

    /// <summary>Prints the baseline relationship beneath a catalog entry.</summary>
    private void WriteComparison(BenchCase benchCase, int depth)
    {
        switch (benchCase.Role)
        {
            case BenchCaseRole.Baseline:
                WriteIndented("comparison baseline", depth);
                break;
            case BenchCaseRole.Candidate:
                WriteIndented($"comparison candidate; baseline: {benchCase.BaselineId}", depth);
                break;
        }
    }

    /// <summary>Wraps an annotation aligned with its hierarchy depth.</summary>
    private void WriteIndented(string text, int depth)
    {
        var indentation = depth * IndentWidth;
        Console.Write(BenchFormatting.Indent(depth));
        terminal.WriteWrapped(text, indentation);
    }

    /// <summary>Finds the shared path prefix to avoid repeating group headings.</summary>
    private static int CommonDepth(ReadOnlySpan<string> left, ReadOnlySpan<string> right)
    {
        var commonDepth = 0;
        var maximumDepth = Math.Min(left.Length, right.Length);

        while (commonDepth < maximumDepth && left[commonDepth] == right[commonDepth])
            commonDepth++;

        return commonDepth;
    }

}
