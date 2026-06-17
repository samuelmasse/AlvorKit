namespace AlvorKit.Injection;

/// <summary>
/// Marks services that are valid only inside an injector scope carrying the same attribute type.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public abstract class InjectorAttribute : Attribute;
