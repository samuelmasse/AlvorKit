namespace AlvorKit.OpenGL.Layer;

internal readonly struct GlBindingMap<TKey>() where TKey : notnull
{
    private readonly Dictionary<TKey, uint> bound = [];

    internal IReadOnlyDictionary<TKey, uint> Bound => bound;

    internal bool HasAny => bound.Count > 0;

    internal void Bind(string function, TKey key, uint value)
    {
        if (bound.TryGetValue(key, out var current))
            throw new GlAlreadyBoundException(function, $"attempted to bind {key} to {value}, but it is already bound to {current}; unbind it first.");
        bound[key] = value;
    }

    internal void Unbind(string function, TKey key)
    {
        if (!bound.Remove(key))
            throw new GlNotBoundException(function, $"attempted to unbind {key}, but nothing is bound.");
    }

    internal bool TryGet(TKey key, out uint value) => bound.TryGetValue(key, out value);

    internal void Clear() => bound.Clear();

    internal void UnbindWhere(string function, Func<TKey, bool> predicate)
    {
        List<TKey>? keys = null;
        foreach (var key in bound.Keys)
        {
            if (!predicate(key))
                continue;
            keys ??= [];
            keys.Add(key);
        }

        if (keys is null)
            throw new GlNotBoundException(function, "attempted to unbind, but nothing is bound.");

        foreach (var key in keys)
            bound.Remove(key);
    }
}
