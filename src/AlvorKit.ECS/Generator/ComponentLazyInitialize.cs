namespace AlvorKit.ECS.Generator;

/// <summary>Marks a component property whose generated mutating getter should create a missing value.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ComponentLazyInitializeAttribute : Attribute;
