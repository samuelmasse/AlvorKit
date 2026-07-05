namespace AlvorKit.UI.Blend.Demo.NoiseLab;

/// <summary>Marks services that belong to the Noise Lab demo lifetime.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public class AppAttribute : InjectorAttribute;
