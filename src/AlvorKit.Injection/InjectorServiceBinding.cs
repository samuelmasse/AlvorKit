namespace AlvorKit.Injection;

/// <summary>
/// Describes a service alias registered in an injector scope.
/// </summary>
/// <param name="ServiceType">Requested service type supplied by this binding.</param>
/// <param name="Owner">Scope state that owns the binding and its cached instance.</param>
internal abstract record InjectorServiceBinding(Type ServiceType, InjectorScopeState Owner);

/// <summary>
/// Binds a service type to an implementation type constructed by the injector.
/// </summary>
/// <param name="ServiceType">Requested service type supplied by this binding.</param>
/// <param name="Owner">Scope state that owns the binding and its cached instance.</param>
/// <param name="ImplementationType">Concrete type used to satisfy service requests.</param>
internal sealed record InjectorImplementationBinding(
    Type ServiceType,
    InjectorScopeState Owner,
    Type ImplementationType) : InjectorServiceBinding(ServiceType, Owner);

/// <summary>
/// Binds a service type to an already-created instance.
/// </summary>
/// <param name="ServiceType">Requested service type supplied by this binding.</param>
/// <param name="Owner">Scope state that owns the binding and aliases.</param>
/// <param name="Instance">Existing instance returned for service requests.</param>
internal sealed record InjectorInstanceBinding(
    Type ServiceType,
    InjectorScopeState Owner,
    object Instance) : InjectorServiceBinding(ServiceType, Owner);
