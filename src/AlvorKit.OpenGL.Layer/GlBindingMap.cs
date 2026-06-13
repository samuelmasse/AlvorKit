namespace AlvorKit.OpenGL.Layer;

internal readonly struct GlBindingMap<TKey>() where TKey : notnull
{
    private readonly Dictionary<TKey, uint> bound = [];

    internal IReadOnlyDictionary<TKey, uint> Bound => bound;

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
}
