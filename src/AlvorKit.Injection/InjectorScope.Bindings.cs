namespace AlvorKit.Injection;

/// <summary>
/// Service binding helpers for <see cref="InjectorScope"/>.
/// </summary>
public abstract partial class InjectorScope
{
    /// <summary>
    /// Binds all compatible scoped service surfaces implemented by <typeparamref name="TImplementation"/>.
    /// </summary>
    /// <typeparam name="TImplementation">Concrete implementation type constructed by the injector.</typeparam>
    public void Bind<TImplementation>()
    {
        ValidateInitialized(State);
        lock (State.Root)
        {
            State.Bind(typeof(TImplementation));
        }
    }

    /// <summary>
    /// Binds all compatible scoped service surfaces implemented by <paramref name="implementationType"/>.
    /// </summary>
    /// <param name="implementationType">Concrete implementation type constructed by the injector.</param>
    public void Bind(Type implementationType)
    {
        ValidateInitialized(State);
        lock (State.Root)
        {
            State.Bind(implementationType);
        }
    }

    /// <summary>
    /// Binds <typeparamref name="TService"/> to <typeparamref name="TImplementation"/> in this scope.
    /// </summary>
    /// <typeparam name="TService">Service type requested by constructors or callers.</typeparam>
    /// <typeparam name="TImplementation">Concrete implementation type constructed by the injector.</typeparam>
    public void Bind<TService, TImplementation>() where TImplementation : TService
    {
        ValidateInitialized(State);
        lock (State.Root)
        {
            State.Bind(typeof(TService), typeof(TImplementation));
        }
    }

    /// <summary>
    /// Binds <paramref name="serviceType"/> to <paramref name="implementationType"/> in this scope.
    /// </summary>
    /// <param name="serviceType">Service type requested by constructors or callers.</param>
    /// <param name="implementationType">Concrete implementation type constructed by the injector.</param>
    public void Bind(Type serviceType, Type implementationType)
    {
        ValidateInitialized(State);
        lock (State.Root)
        {
            State.Bind(serviceType, implementationType);
        }
    }

    /// <summary>
    /// Binds all compatible scoped service surfaces implemented by <paramref name="instance"/>.
    /// </summary>
    /// <param name="instance">Existing instance returned for compatible service requests.</param>
    public void Bind(object instance)
    {
        ValidateInitialized(State);
        lock (State.Root)
        {
            State.Bind(instance);
        }
    }

    /// <summary>
    /// Registers <paramref name="instance"/> as <typeparamref name="TService"/> in this scope.
    /// </summary>
    /// <typeparam name="TService">Service type requested by constructors or callers.</typeparam>
    /// <param name="instance">Existing instance returned for service requests.</param>
    public void Add<TService>(TService instance)
    {
        ValidateInitialized(State);
        lock (State.Root)
        {
            State.Add(typeof(TService), instance!);
        }
    }

    /// <summary>
    /// Registers <paramref name="instance"/> as <paramref name="serviceType"/> in this scope.
    /// </summary>
    /// <param name="serviceType">Service type requested by constructors or callers.</param>
    /// <param name="instance">Existing instance returned for service requests.</param>
    public void Add(Type serviceType, object instance)
    {
        ValidateInitialized(State);
        lock (State.Root)
        {
            State.Add(serviceType, instance);
        }
    }
}
