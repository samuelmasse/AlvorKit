namespace AlvorKit.Injection;

/// <summary>Identifies one scope instance for the lifetime of an injector scope graph.</summary>
public readonly record struct InjectorScopeId(long Value)
{
    /// <inheritdoc />
    public override string ToString() => $"scope-{Value}";
}
