namespace AlvorKit.OpenGL.Layer;

internal struct GlBinding
{
    private uint current;

    internal readonly uint Current => current;

    internal void Bind(string function, uint value)
    {
        if (current != 0)
            throw new GlAlreadyBoundException(function, $"attempted to bind {value}, but {current} is already bound; unbind it first.");
        current = value;
    }

    internal void Unbind(string function)
    {
        if (current == 0)
            throw new GlNotBoundException(function, "attempted to unbind, but nothing is bound.");
        current = 0;
    }
}
