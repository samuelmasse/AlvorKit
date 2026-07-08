namespace AlvorKit.ECS.Indexed;

/// <summary>Registers indexed ECS hooks and bags for one context entity.</summary>
public class EntIdxContextBuilder
{
    private readonly EntObj ent = new();

    /// <summary>Gets the context entity that owns hook lists for this indexed scope.</summary>
    public EntObj Ent => ent;

    /// <summary>Registers a hook that runs before the requested component is written or unset.</summary>
    /// <typeparam name="T">The component value type.</typeparam>
    /// <typeparam name="N">The component marker type.</typeparam>
    /// <param name="hook">The hook to append in registration order.</param>
    public void AddPre<T, N>(EntIdxPreHook<T> hook) where N : IComponent
    {
        ValidateComponent<T, N>();
        Add<EntIdxPre<T, N>, EntIdxPreHook<T>>(hook);
    }

    /// <summary>Registers a hook that runs after the requested component is written or unset.</summary>
    /// <typeparam name="T">The component value type.</typeparam>
    /// <typeparam name="N">The component marker type.</typeparam>
    /// <param name="hook">The hook to append in registration order.</param>
    public void AddPost<T, N>(EntIdxPostHook hook) where N : IComponent
    {
        ValidateComponent<T, N>();
        Add<EntIdxPost<T, N>, EntIdxPostHook>(hook);
    }

    /// <summary>Registers a hook that runs before an entity is disposed.</summary>
    /// <param name="hook">The hook to append in registration order.</param>
    public void AddPreDispose(EntIdxPreDisposeHook hook) =>
        Add<EntIdxPreDispose, EntIdxPreDisposeHook>(hook);

    /// <summary>Registers a plain marker bag whose membership is the marker value.</summary>
    /// <typeparam name="N">The bool marker component.</typeparam>
    /// <param name="bag">The mutable bag maintained by registered interceptors.</param>
    public void AddBag<N>(EntIdxBagMut<N> bag) where N : IComponent
    {
        ValidateBool<N>("marker");
        ThrowIfBagRegistered<EntIdxBagIndex<N>>();

        var interceptor = new EntIdxBagInterceptor<N>(bag);
        AddPost<bool, N>(interceptor.Update);
        AddPre<int, EntIdxBagIndex<N>>(interceptor.RemoveWhenIndexUnsets);
    }

    /// <summary>Registers a gated marker bag whose membership is the marker value and gate value.</summary>
    /// <typeparam name="N">The bool marker component.</typeparam>
    /// <typeparam name="TGate">The bool gate component.</typeparam>
    /// <param name="bag">The mutable bag maintained by registered interceptors.</param>
    public void AddGatedBag<N, TGate>(EntIdxGatedBagMut<N, TGate> bag)
        where N : IComponent
        where TGate : IComponent
    {
        ValidateBool<N>("marker");
        ValidateBool<TGate>("gate");
        ThrowIfBagRegistered<EntIdxGatedBagIndex<N, TGate>>();

        var interceptor = new EntIdxGatedBagInterceptor<N, TGate>(bag);
        AddPost<bool, N>(interceptor.Update);
        AddPost<bool, TGate>(interceptor.Update);
        AddPre<int, EntIdxGatedBagIndex<N, TGate>>(interceptor.RemoveWhenIndexUnsets);
    }

    /// <summary>Appends a hook to a context-owned hook list component.</summary>
    /// <typeparam name="P">The context component key that stores the hook list.</typeparam>
    /// <typeparam name="PT">The hook delegate type.</typeparam>
    /// <param name="hook">The hook to append.</param>
    protected void Add<P, PT>(PT hook)
    {
        ReadOnlyMemory<PT> hooks = ent.Get<ReadOnlyMemory<PT>, P>();
        var next = new PT[hooks.Length + 1];
        hooks.Span.CopyTo(next);
        next[^1] = hook;
        ent.Set<ReadOnlyMemory<PT>, P>(next);
    }

    internal static void ValidateComponent<T, N>() where N : IComponent
    {
        var component = N.Component;
        if (component.ValueType != typeof(T))
            throw new EntIdxRegistrationException(
                $"Component {typeof(N).FullName} stores {component.ValueType.FullName}, not {typeof(T).FullName}.");
    }

    internal static void ValidateBool<N>(string role) where N : IComponent
    {
        var component = N.Component;
        if (component.ValueType != typeof(bool))
            throw new EntIdxRegistrationException(
                $"Bag {role} {typeof(N).FullName} must be a bool component, not {component.ValueType.FullName}.");
    }

    private void ThrowIfBagRegistered<TBagIndex>() where TBagIndex : IComponent
    {
        if (ent.Has<ReadOnlyMemory<EntIdxPreHook<int>>, EntIdxPre<int, TBagIndex>>())
            throw new EntIdxRegistrationException(
                $"Bag identity {typeof(TBagIndex).FullName} is already registered on this context.");
    }
}

