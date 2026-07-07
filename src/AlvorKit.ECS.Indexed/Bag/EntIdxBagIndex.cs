namespace AlvorKit.ECS.Indexed;

/// <summary>Component key storing an entity's dense slot in a plain marker bag.</summary>
/// <typeparam name="N">The marker component type.</typeparam>
internal abstract class EntIdxBagIndex<N> : IComponent
{
    /// <summary>Gets metadata for the integer bag slot component.</summary>
    public static EntComponent Component => new(typeof(int), typeof(EntIdxBagIndex<N>));
}

/// <summary>Component key storing an entity's dense slot in a gated marker bag.</summary>
/// <typeparam name="N">The marker component type.</typeparam>
/// <typeparam name="TGate">The gate component type.</typeparam>
internal abstract class EntIdxBagIndex<N, TGate> : IComponent
{
    /// <summary>Gets metadata for the integer bag slot component.</summary>
    public static EntComponent Component => new(typeof(int), typeof(EntIdxBagIndex<N, TGate>));
}

