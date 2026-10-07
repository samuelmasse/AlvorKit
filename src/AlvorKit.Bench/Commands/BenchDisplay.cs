namespace AlvorKit;

/// <summary>Human-output options, independent of benchmark selection and JSON export.</summary>
public readonly record struct BenchDisplay(bool Quiet, bool ShowSamples, bool ShowMemoryTree);
