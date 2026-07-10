namespace AlvorKit.ECS.Demo.Bench;

/// <summary>Stores one versioned mixed-suite benchmark report.</summary>
public sealed record EcsBenchReport(
    int SchemaVersion,
    string Label,
    DateTimeOffset CreatedUtc,
    EcsBenchEnvironment Environment,
    EcsBenchReportOptions Options,
    EcsBenchResult[] CoreResults,
    EcsArchBenchResult[] ArchetypalResults);

/// <summary>Stores runtime details needed to interpret benchmark samples.</summary>
public sealed record EcsBenchEnvironment(
    string Runtime,
    string ProcessArchitecture,
    string OperatingSystem,
    string GarbageCollector,
    int ProcessorCount,
    long StopwatchFrequency);

/// <summary>Stores the user-visible options that produced a benchmark report.</summary>
public sealed record EcsBenchReportOptions(
    string Suite,
    int Operations,
    int Runs,
    int Warmups,
    int[] Widths,
    int Arches,
    int Rows,
    int Allocs,
    string[] Cases);
