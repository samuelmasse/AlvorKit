namespace AlvorKit.Engine;

/// <summary>Marks services that belong to the root engine lifetime scope.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class RootAttribute : InjectorAttribute;
