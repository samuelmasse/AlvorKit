namespace AlvorKit;

/// <summary>Marks suite and runner services for benchmark-scope injection.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = false)]
public class BenchAttribute : InjectorAttribute;
