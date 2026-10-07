namespace AlvorKit;

/// <summary>A suite flattened into executable cases in declaration order.</summary>
public record BenchCatalog(string SuiteName, BenchCase[] Cases);
