namespace AlvorKit.ECS.Generator;

[AttributeUsage(AttributeTargets.Interface)]
public sealed class ComponentsAttribute : Attribute
{
        public bool SkipBuilder { get; set; }
}
