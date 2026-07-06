namespace AlvorStarter.App;

/// <summary>Marks services owned by the application lifetime.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = false)]
public class AppAttribute : InjectorAttribute;
