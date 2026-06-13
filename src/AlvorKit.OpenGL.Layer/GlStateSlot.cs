namespace AlvorKit.OpenGL.Layer;

internal struct GlStateSlot<T> where T : struct
{
    private T? current;

    internal readonly bool IsSet => current.HasValue;

    internal readonly T? Value => current;

    internal void Set(string function, T value)
    {
        if (current.HasValue)
            throw new GlAlreadySetException(function, $"attempted to set {value}, but {current.Value} is already set.");
        current = value;
    }

    internal void Reset(string function)
    {
        if (!current.HasValue)
            throw new GlAlreadyUnsetException(function, "attempted to reset, but nothing is set.");
        current = null;
    }
}
