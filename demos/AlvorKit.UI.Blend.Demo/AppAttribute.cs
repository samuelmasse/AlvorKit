namespace AlvorKit.UI.Blend.Demo;

/// <summary>Marks services that belong to the Blend editor-shell demo lifetime.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public class AppAttribute : InjectorAttribute;
