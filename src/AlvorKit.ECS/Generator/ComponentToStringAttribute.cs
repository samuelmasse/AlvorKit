namespace AlvorKit.ECS.Generator;

/// <summary>Marks a component that should appear in generated entity string output.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public sealed class ComponentToStringAttribute : Attribute;
