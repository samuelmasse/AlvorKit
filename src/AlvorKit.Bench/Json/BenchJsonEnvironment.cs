namespace AlvorKit;

/// <summary>Records runtime, operating system, architecture, processor count, and GC mode.</summary>
public record BenchJsonEnvironment(
    string Runtime,
    string OperatingSystem,
    string ProcessArchitecture,
    int ProcessorCount,
    bool ServerGarbageCollection);
