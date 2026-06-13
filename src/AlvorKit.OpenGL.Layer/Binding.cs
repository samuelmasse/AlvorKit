namespace AlvorKit.OpenGL.Layer;

internal struct Binding(bool zeroIsValid = false)
{
    private readonly bool zeroIsValid = zeroIsValid;
    private uint current;

    internal readonly uint Current => current;

    internal void Bind(string function, uint value)
    {
        if (value != 0 && current != 0)
            throw new GlAlreadyBoundException(function, $"attempted to bind {value}, but {current} is already bound.");
        if (value == 0 && current == 0 && !zeroIsValid)
            throw new GlNotBoundException(function, "attempted to unbind, but nothing is bound.");
        current = value;
    }

    internal void Begin(string function, uint value)
    {
        if (current != 0)
            throw new GlAlreadyBoundException(function, $"attempted to begin {value}, but {current} is already active.");
        current = value;
    }

    internal void End(string function)
    {
        if (current == 0)
            throw new GlNotBoundException(function, "attempted to end, but nothing is active.");
        current = 0;
    }
}
