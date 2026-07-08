namespace AlvorKit.ECS.Indexed;

public delegate void EntIdxPreHook<T>(EntMutIdx ent, in T value);

public delegate void EntIdxPostHook(EntMutIdx ent);

public delegate void EntIdxPreDisposeHook(EntMutIdx ent);

internal abstract class EntIdxPre<T, N> : IComponent
{
    public static EntComponent Component => new(typeof(ReadOnlyMemory<EntIdxPreHook<T>>), typeof(EntIdxPre<T, N>));
}

internal abstract class EntIdxPost<T, N> : IComponent
{
    public static EntComponent Component => new(typeof(ReadOnlyMemory<EntIdxPostHook>), typeof(EntIdxPost<T, N>));
}

internal abstract class EntIdxPreDispose : IComponent
{
    public static EntComponent Component => new(typeof(ReadOnlyMemory<EntIdxPreDisposeHook>), typeof(EntIdxPreDispose));
}
