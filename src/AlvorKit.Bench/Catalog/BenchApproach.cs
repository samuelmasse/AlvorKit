namespace AlvorKit;

/// <summary>Names one prepared measurement implementation within a comparison.</summary>
public record BenchApproach(string Name, Func<BenchResult> Run);
