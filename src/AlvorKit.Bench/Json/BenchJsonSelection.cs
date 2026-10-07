namespace AlvorKit;

/// <summary>Records the exact include, exclude, warmup, and sample options used by a run.</summary>
public record BenchJsonSelection(string[] Include, string[] Exclude, int WarmupCount, int SampleCount);
