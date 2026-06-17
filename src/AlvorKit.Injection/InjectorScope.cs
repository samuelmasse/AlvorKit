namespace AlvorKit.Injection;

/// <summary>
/// Base class for root and nested injector scopes.
/// </summary>
public abstract partial class InjectorScope
{
    /// <summary>
    /// Mutable state backing this scope, assigned by the root injector or by a parent scope.
    /// </summary>
    internal InjectorScopeState? State { get; set; } = null;

    /// <summary>
    /// Creates a child scope with its own scoped service cache and attribute gate.
    /// </summary>
    public T Scope<T>() where T : InjectorScope
    {
        ValidateInitialized(State);
        lock (State.Root)
        {
            var type = typeof(T);
            var constructor = GetConstructor(State.Root, type);
            var attributeType = GetAttributeType(State.Root, type);
            ValidateAttributeType(type, attributeType);

            var scope = (T)constructor.Invoke([]);
            scope.State = new(State.Root, State, attributeType);
            scope.Add(scope);

            return scope;
        }
    }

    /// <summary>
    /// Adds an inclusion regex; once any include exists, services must match one include from this scope or an ancestor.
    /// </summary>
    public void Include(Regex pattern)
    {
        ValidateInitialized(State);
        lock (State.Root)
        {
            State.Include(pattern);
        }
    }

    /// <summary>
    /// Adds a custom dependency construction handler to this scope.
    /// </summary>
    public void Handler(InjectorCustomHandler handler)
    {
        ValidateInitialized(State);
        lock (State.Root)
        {
            State.Handler(handler);
        }
    }

    /// <summary>
    /// Gets or creates the cached service instance for <paramref name="type"/> in the nearest valid scope.
    /// </summary>
    public object Get(Type type)
    {
        ValidateInitialized(State);
        lock (State.Root)
        {
            return State.Get(type);
        }
    }

    /// <summary>
    /// Gets or creates the cached service instance for <typeparamref name="T"/> in the nearest valid scope.
    /// </summary>
    public T Get<T>()
    {
        ValidateInitialized(State);
        lock (State.Root)
        {
            return (T)State.Get(typeof(T));
        }
    }

    /// <summary>
    /// Creates a new instance of <paramref name="type"/> without caching the constructed instance.
    /// </summary>
    public object New(Type type)
    {
        ValidateInitialized(State);
        lock (State.Root)
        {
            return State.New(type);
        }
    }

    /// <summary>
    /// Creates a new instance of <typeparamref name="T"/> without caching the constructed instance.
    /// </summary>
    public T New<T>()
    {
        ValidateInitialized(State);
        lock (State.Root)
        {
            return (T)State.New(typeof(T));
        }
    }

    /// <summary>
    /// Registers an already-created instance in this scope.
    /// </summary>
    public void Add(object instance)
    {
        ValidateInitialized(State);
        lock (State.Root)
        {
            State.Add(instance);
        }
    }

}

/// <summary>
/// Base class for scopes whose services must be marked with <typeparamref name="T"/>.
/// </summary>
public class InjectorScope<T> : InjectorScope where T : InjectorAttribute;
