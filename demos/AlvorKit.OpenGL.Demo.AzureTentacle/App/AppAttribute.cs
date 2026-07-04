namespace AlvorKit.OpenGL.Demo.AzureTentacle;

/// <summary>Marks services that belong to the azure tentacle demo app lifetime.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public class AppAttribute : InjectorAttribute;
