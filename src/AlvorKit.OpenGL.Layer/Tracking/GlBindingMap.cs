namespace AlvorKit.OpenGL.Layer;

/// <summary>
/// Tracks strict object bindings keyed by GL target, unit, index, or a composite of those values.
/// </summary>
/// <typeparam name="TKey">The key that identifies a strict binding slot.</typeparam>
internal readonly struct GlBindingMap<TKey>() where TKey : notnull
{
    /// <summary>
    /// Stores the currently bound GL object ids by strict binding key.
    /// </summary>
    private readonly Dictionary<TKey, uint> bound = [];

    /// <summary>
    /// Gets the currently bound GL object ids by strict binding key.
    /// </summary>
    internal IReadOnlyDictionary<TKey, uint> Bound => bound;

    /// <summary>
    /// Gets a value indicating whether any binding key is occupied.
    /// </summary>
    internal bool HasAny => bound.Count > 0;

    /// <summary>
    /// Records a non-overlapping bind for the specified key.
    /// </summary>
    /// <param name="function">The GL function that requested the bind.</param>
    /// <param name="key">The strict binding key to occupy.</param>
    /// <param name="value">The GL object id to record for the key.</param>
    /// <exception cref="GlAlreadyBoundException">Thrown when the key is already occupied.</exception>
    internal void Bind(string function, TKey key, uint value)
    {
        if (bound.TryGetValue(key, out var current))
            throw new GlAlreadyBoundException(function, $"attempted to bind {key} to {value}, but it is already bound to {current}; unbind it first.");
        bound[key] = value;
    }

    /// <summary>
    /// Clears the recorded bind for the specified key.
    /// </summary>
    /// <param name="function">The GL function that requested the unbind.</param>
    /// <param name="key">The strict binding key to release.</param>
    /// <exception cref="GlNotBoundException">Thrown when the key is not occupied.</exception>
    internal void Unbind(string function, TKey key)
    {
        if (!bound.Remove(key))
            throw new GlNotBoundException(function, $"attempted to unbind {key}, but nothing is bound.");
    }

    /// <summary>
    /// Attempts to get the GL object id bound to the specified key.
    /// </summary>
    /// <param name="key">The strict binding key to inspect.</param>
    /// <param name="value">The GL object id bound to the key when present.</param>
    /// <returns><see langword="true"/> when the key is occupied; otherwise, <see langword="false"/>.</returns>
    internal bool TryGet(TKey key, out uint value) => bound.TryGetValue(key, out value);

    /// <summary>
    /// Returns whether any key currently binds the specified nonzero object id.
    /// </summary>
    /// <param name="value">The GL object id to inspect.</param>
    /// <returns><see langword="true"/> when <paramref name="value"/> is live-bound by any key.</returns>
    internal bool ContainsValue(uint value) => value != 0 && bound.ContainsValue(value);

    /// <summary>
    /// Clears every recorded binding without calling the backend.
    /// </summary>
    internal void Clear() => bound.Clear();

    /// <summary>
    /// Returns a struct enumerator over the current bindings.
    /// </summary>
    /// <returns>A dictionary enumerator for allocation-free scans.</returns>
    internal Dictionary<TKey, uint>.Enumerator GetEnumerator() => bound.GetEnumerator();
}
