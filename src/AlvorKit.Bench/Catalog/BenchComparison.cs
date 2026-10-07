namespace AlvorKit;

/// <summary>Pairs a baseline measurement with the candidates measured against it.</summary>
public record BenchComparison(BenchApproach Baseline, BenchApproach[] Candidates);
