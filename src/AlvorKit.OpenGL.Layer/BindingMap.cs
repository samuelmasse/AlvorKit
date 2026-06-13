namespace AlvorKit.OpenGL.Layer;

internal readonly struct BindingMap<TKey>() where TKey : notnull
{
    private readonly Dictionary<TKey, uint> bound = [];

    internal IReadOnlyDictionary<TKey, uint> Bound => bound;

    internal void Bind(string function, TKey key, uint value)
    {
        var has = bound.TryGetValue(key, out var current);
        if (value != 0 && has)
            throw new GlAlreadyBoundException(function, $"attempted to bind {key} to {value}, but it is already bound to {current}.");
        if (value == 0 && !has)
            throw new GlNotBoundException(function, $"attempted to unbind {key}, but nothing is bound.");
        if (value == 0)
            bound.Remove(key);
        else
            bound[key] = value;
    }

    internal void Begin(string function, TKey key, uint value)
    {
        if (bound.ContainsKey(key))
            throw new GlAlreadyBoundException(function, $"attempted to begin {key}, but it is already active.");
        bound[key] = value;
    }

    internal void End(string function, TKey key)
    {
        if (!bound.Remove(key))
            throw new GlNotBoundException(function, $"attempted to end {key}, but nothing is active.");
    }

    internal bool TryGet(TKey key, out uint value) => bound.TryGetValue(key, out value);
}
