namespace AlvorKit.OpenGL.Layer;

internal readonly struct GlStateMap<TKey, TValue>() where TKey : notnull where TValue : struct
{
    private readonly Dictionary<TKey, TValue> entries = [];

    internal readonly bool HasAny => entries.Count > 0;

    internal readonly bool IsSet(TKey key) => entries.ContainsKey(key);

    internal readonly void Set(string function, TKey key, TValue value)
    {
        if (entries.ContainsKey(key))
            throw new GlAlreadySetException(function, $"attempted to set {key} to {value}, but {entries[key]} is already set.");
        entries[key] = value;
    }

    internal readonly void Reset(string function, TKey key)
    {
        if (!entries.Remove(key))
            throw new GlAlreadyUnsetException(function, $"attempted to reset {key}, but nothing is set.");
    }
}
