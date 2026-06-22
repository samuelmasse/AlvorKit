namespace AlvorKit.Engine;

/// <summary>Dependency injection scope for services owned by the root engine lifetime.</summary>
[Root]
public sealed class RootScope : InjectorScope<RootAttribute>;
