using System.Text.Json;

namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Source-filtered allocation stacks that can be inspected directly or opened as a Speedscope flame chart.</summary>
public class InterceptionAllocationSourceReport
{
    /// <summary>Source-resolved weighted stacks in capture order.</summary>
    private readonly InterceptionAllocationSourceSample[] samples;
    /// <summary>Aggregated source lines ordered by attributed allocation count.</summary>
    private readonly InterceptionAllocationLine[] topLines;
    /// <summary>Exact object count for the original native capture window.</summary>
    private readonly ulong exactTotalObjectAllocations;
    /// <summary>Distance between scheduled native stack samples.</summary>
    private readonly uint sampleInterval;
    /// <summary>Scheduled samples omitted after native storage filled.</summary>
    private readonly ulong droppedSamples;
    /// <summary>Scheduled samples whose CoreCLR stack walk failed.</summary>
    private readonly ulong failedStackWalks;
    /// <summary>Weighted object count represented by source-resolved samples.</summary>
    private readonly ulong attributedObjectAllocations;

    /// <summary>Gets the exact number of objects allocated in the original capture window.</summary>
    public ulong ExactTotalObjectAllocations =>
        exactTotalObjectAllocations;

    /// <summary>Gets the number of allocations represented by source-resolved retained stacks.</summary>
    public ulong AttributedObjectAllocations =>
        attributedObjectAllocations;

    /// <summary>Gets whether every exact allocation has an individually retained, source-resolved stack.</summary>
    public bool IsLineAttributionExact =>
        sampleInterval == 1 &&
        droppedSamples == 0 &&
        failedStackWalks == 0 &&
        attributedObjectAllocations == exactTotalObjectAllocations;

    /// <summary>Gets source-resolved root-to-leaf samples.</summary>
    public IReadOnlyList<InterceptionAllocationSourceSample> Samples =>
        samples;

    /// <summary>Gets source lines ordered by attributed allocation count.</summary>
    public IReadOnlyList<InterceptionAllocationLine> TopLines =>
        topLines;

    /// <summary>Creates an immutable source report and precomputes its line aggregates.</summary>
    internal InterceptionAllocationSourceReport(
        ulong exactTotalObjectAllocations,
        uint sampleInterval,
        ulong droppedSamples,
        ulong failedStackWalks,
        InterceptionAllocationSourceSample[] samples)
    {
        this.exactTotalObjectAllocations = exactTotalObjectAllocations;
        this.sampleInterval = sampleInterval;
        this.droppedSamples = droppedSamples;
        this.failedStackWalks = failedStackWalks;
        this.samples = samples;
        attributedObjectAllocations = samples.Aggregate(0UL, static (total, sample) => total + sample.Weight);
        topLines = AggregateLines(samples);
    }

    /// <summary>Writes the retained weighted stacks in the Speedscope sampled-profile format.</summary>
    public void WriteSpeedscope(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        using var stream = File.Create(fullPath);
        using var writer = new Utf8JsonWriter(
            stream,
            new() { Indented = true });
        WriteSpeedscope(writer);
    }

    /// <summary>Writes the complete Speedscope document to an existing JSON writer.</summary>
    private void WriteSpeedscope(Utf8JsonWriter writer)
    {
        var frameIndexes =
            new Dictionary<InterceptionAllocationSourceFrame, int>();
        foreach (var sample in samples)
        {
            foreach (var frame in sample.Frames)
                frameIndexes.TryAdd(frame, frameIndexes.Count);
        }

        writer.WriteStartObject();
        writer.WriteString(
            "$schema",
            "https://www.speedscope.app/file-format-schema.json");
        writer.WriteString("exporter", "AlvorKit allocation profiler");
        writer.WritePropertyName("shared");
        writer.WriteStartObject();
        writer.WritePropertyName("frames");
        writer.WriteStartArray();
        foreach (var frame in frameIndexes.OrderBy(static pair => pair.Value))
        {
            writer.WriteStartObject();
            writer.WriteString("name", FrameName(frame.Key));
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
        WriteProfile(writer, frameIndexes);
        writer.WriteNumber("activeProfileIndex", 0);
        writer.WriteEndObject();
    }

    /// <summary>Writes one weighted sampled profile using the shared frame indexes.</summary>
    private void WriteProfile(
        Utf8JsonWriter writer,
        IReadOnlyDictionary<InterceptionAllocationSourceFrame, int> indexes)
    {
        writer.WritePropertyName("profiles");
        writer.WriteStartArray();
        writer.WriteStartObject();
        writer.WriteString("type", "sampled");
        writer.WriteString("name", "Managed object allocations");
        writer.WriteString("unit", "none");
        writer.WriteNumber("startValue", 0);
        writer.WriteNumber("endValue", attributedObjectAllocations);
        writer.WritePropertyName("samples");
        writer.WriteStartArray();
        foreach (var sample in samples)
        {
            writer.WriteStartArray();
            foreach (var frame in sample.Frames)
                writer.WriteNumberValue(indexes[frame]);
            writer.WriteEndArray();
        }
        writer.WriteEndArray();
        writer.WritePropertyName("weights");
        writer.WriteStartArray();
        foreach (var sample in samples)
            writer.WriteNumberValue(sample.Weight);
        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.WriteEndArray();
    }

    /// <summary>Attributes every weighted stack to its deepest source-resolved frame.</summary>
    private static InterceptionAllocationLine[] AggregateLines(
        IReadOnlyList<InterceptionAllocationSourceSample> samples)
    {
        var totals =
            new Dictionary<(string Method, string Document, int Line),
                (ulong Allocations, uint Samples)>();
        foreach (var sample in samples)
        {
            var frame = sample.Frames.LastOrDefault(
                static value =>
                    value.Document is not null &&
                    value.Line is not null);
            if (frame.Document is null || frame.Line is not { } line)
                continue;
            var key = (frame.Method, frame.Document, line);
            totals.TryGetValue(key, out var prior);
            totals[key] = (
                prior.Allocations + sample.Weight,
                prior.Samples + 1);
        }

        return
        [
            .. totals
                .Select(static value => new InterceptionAllocationLine(
                    value.Key.Method,
                    value.Key.Document,
                    value.Key.Line,
                    value.Value.Allocations,
                    value.Value.Samples))
                .OrderByDescending(static value =>
                    value.AttributedObjectAllocations)
                .ThenBy(static value => value.Document)
                .ThenBy(static value => value.Line)
        ];
    }

    /// <summary>Formats one Speedscope frame name with source location when available.</summary>
    private static string FrameName(
        InterceptionAllocationSourceFrame frame) =>
        frame.Document is null || frame.Line is null
            ? frame.Method
            : $"{frame.Method} ({frame.Document}:{frame.Line})";
}
