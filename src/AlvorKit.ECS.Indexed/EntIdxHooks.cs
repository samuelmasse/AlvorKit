namespace AlvorKit.ECS.Indexed;

/// <summary>Runs before an indexed component value is written or unset.</summary>
/// <typeparam name="T">The component value type.</typeparam>
/// <param name="ent">The entity being mutated.</param>
/// <param name="value">The incoming value, or the default value when the component is being unset.</param>
public delegate void EntIdxPreHook<T>(EntMutIdx ent, in T value);

/// <summary>Runs after an indexed component value is written or unset.</summary>
/// <param name="ent">The entity that was mutated.</param>
public delegate void EntIdxPostHook(EntMutIdx ent);

/// <summary>Runs before an indexed entity is disposed while all components are still present.</summary>
/// <param name="ent">The entity that is about to be disposed.</param>
public delegate void EntIdxPreDisposeHook(EntMutIdx ent);

/// <summary>Component key storing pre-set hooks for one component identity on a context entity.</summary>
/// <typeparam name="T">The component value type.</typeparam>
/// <typeparam name="N">The component marker type.</typeparam>
internal abstract class EntIdxPre<T, N> : IComponent
{
    /// <summary>Gets metadata for the context component that stores pre hooks.</summary>
    public static EntComponent Component => new(typeof(ReadOnlyMemory<EntIdxPreHook<T>>), typeof(EntIdxPre<T, N>));
}

/// <summary>Component key storing post-set hooks for one component identity on a context entity.</summary>
/// <typeparam name="T">The component value type.</typeparam>
/// <typeparam name="N">The component marker type.</typeparam>
internal abstract class EntIdxPost<T, N> : IComponent
{
    /// <summary>Gets metadata for the context component that stores post hooks.</summary>
    public static EntComponent Component => new(typeof(ReadOnlyMemory<EntIdxPostHook>), typeof(EntIdxPost<T, N>));
}

/// <summary>Component key storing pre-dispose hooks on a context entity.</summary>
internal abstract class EntIdxPreDispose : IComponent
{
    /// <summary>Gets metadata for the context component that stores pre-dispose hooks.</summary>
    public static EntComponent Component => new(typeof(ReadOnlyMemory<EntIdxPreDisposeHook>), typeof(EntIdxPreDispose));
}

