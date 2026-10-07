namespace AlvorKit;

/// <summary>Owns the shared runner services for one benchmark invocation.</summary>
[Bench]
public class BenchScope : InjectorScope<BenchAttribute>;
