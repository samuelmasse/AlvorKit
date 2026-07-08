namespace AlvorKit.ECS.Indexed;

public class EntIdxContextBuilder
{
    private readonly EntObj ent = new();

    public EntObj Ent => ent;

    public void AddPre<T, N>(EntIdxPreHook<T> hook) where N : IComponent
    {
        ValidateComponent<T, N>();
        Add<EntIdxPre<T, N>, EntIdxPreHook<T>>(hook);
    }

    public void AddPost<T, N>(EntIdxPostHook hook) where N : IComponent
    {
        ValidateComponent<T, N>();
        Add<EntIdxPost<T, N>, EntIdxPostHook>(hook);
    }

    public void AddPreDispose(EntIdxPreDisposeHook hook) =>
    Add<EntIdxPreDispose, EntIdxPreDisposeHook>(hook);

    public void AddBag<N>(EntIdxBagMut<N> bag) where N : IComponent
    {
        ValidateBool<N>("marker");
        ThrowIfBagRegistered<EntIdxBagIndex<N>>();

        var interceptor = new EntIdxBagInterceptor<N>(bag);
        AddPost<bool, N>(interceptor.Update);
        AddPre<int, EntIdxBagIndex<N>>(interceptor.RemoveWhenIndexUnsets);
    }

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
