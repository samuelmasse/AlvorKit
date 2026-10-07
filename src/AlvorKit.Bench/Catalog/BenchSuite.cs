namespace AlvorKit;

/// <summary>Names the top-level suite and owns its authored benchmark tree.</summary>
public record BenchSuite(string Name, BenchNode[] Children);
