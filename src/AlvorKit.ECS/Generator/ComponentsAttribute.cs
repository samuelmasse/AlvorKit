namespace AlvorKit.ECS.Generator;

/// <summary>Marks an interface whose properties should generate ECS component accessors.</summary>
[AttributeUsage(AttributeTargets.Interface)]
public sealed class ComponentsAttribute : Attribute
{
    /// <summary>Gets or sets whether builder-style mutator extension methods should be skipped.</summary>
    public bool SkipBuilder { get; set; }
}
