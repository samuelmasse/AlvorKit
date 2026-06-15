namespace AlvorKit.OpenGL.Layer;

/// <summary>
/// Tracks strict state values keyed by GL target, index, face, parameter name, or a composite key.
/// </summary>
/// <typeparam name="TKey">The key that identifies a strict state slot.</typeparam>
/// <typeparam name="TValue">The value tracked for the state slot.</typeparam>
internal readonly struct GlStateMap<TKey, TValue>() where TKey : notnull where TValue : struct
{
    /// <summary>
    /// Stores the currently set state values by strict state key.
    /// </summary>
    private readonly Dictionary<TKey, TValue> entries = [];

    /// <summary>
    /// Gets a value indicating whether any state key is currently set.
    /// </summary>
    internal readonly bool HasAny => entries.Count > 0;

    /// <summary>
    /// Returns whether the specified key is currently set.
    /// </summary>
    /// <param name="key">The strict state key to inspect.</param>
    /// <returns><see langword="true"/> when the key is set; otherwise, <see langword="false"/>.</returns>
    internal readonly bool IsSet(TKey key) => entries.ContainsKey(key);

    /// <summary>
    /// Records a non-overlapping state value for the specified key.
    /// </summary>
    /// <param name="function">The GL function that requested the state change.</param>
    /// <param name="key">The strict state key to set.</param>
    /// <param name="value">The value to record for the key.</param>
    /// <exception cref="GlAlreadySetException">Thrown when the key is already set.</exception>
    internal readonly void Set(string function, TKey key, TValue value)
    {
        if (entries.ContainsKey(key))
            throw new GlAlreadySetException(function, $"attempted to set {key} to {value}, but {entries[key]} is already set.");
        entries[key] = value;
    }

    /// <summary>
    /// Clears the state value recorded for the specified key.
    /// </summary>
    /// <param name="function">The GL function that requested the reset.</param>
    /// <param name="key">The strict state key to reset.</param>
    /// <exception cref="GlAlreadyUnsetException">Thrown when the key is already unset.</exception>
    internal readonly void Reset(string function, TKey key)
    {
        if (!entries.Remove(key))
            throw new GlAlreadyUnsetException(function, $"attempted to reset {key}, but nothing is set.");
    }
}
