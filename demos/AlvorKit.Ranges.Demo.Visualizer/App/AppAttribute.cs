namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Marks services that belong to the range allocator visualizer app lifetime.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public class AppAttribute : InjectorAttribute;
