namespace AlvorKit;

[AttributeUsage(AttributeTargets.Interface)]
public sealed class ComponentsAttribute : Attribute
{
    public bool SkipBuilder { get; set; }
}
