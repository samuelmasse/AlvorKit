namespace AlvorKit.OpenGL.Layer;

/// <summary>
/// Tracks one strict binding slot that may hold either zero or one live GL object id.
/// </summary>
internal struct GlBinding
{
    /// <summary>
    /// Stores the currently bound GL object id, or zero when the slot is unbound.
    /// </summary>
    private uint current;

    /// <summary>
    /// Gets the currently bound GL object id, or zero when the slot is unbound.
    /// </summary>
    internal readonly uint Current => current;

    /// <summary>
    /// Returns whether this slot currently binds the specified nonzero object id.
    /// </summary>
    /// <param name="value">The GL object id to inspect.</param>
    /// <returns><see langword="true"/> when <paramref name="value"/> is live-bound in this slot.</returns>
    internal readonly bool IsBound(uint value) => value != 0 && current == value;

    /// <summary>
    /// Records a non-overlapping bind for this slot.
    /// </summary>
    /// <param name="function">The GL function that requested the bind.</param>
    /// <param name="value">The GL object id to record for the slot.</param>
    /// <exception cref="GlAlreadyBoundException">Thrown when another object is already bound.</exception>
    internal void Bind(string function, uint value)
    {
        if (current != 0)
            throw new GlAlreadyBoundException(function, $"attempted to bind {value}, but {current} is already bound; unbind it first.");
        current = value;
    }

    /// <summary>
    /// Clears a previously recorded bind for this slot.
    /// </summary>
    /// <param name="function">The GL function that requested the unbind.</param>
    /// <exception cref="GlNotBoundException">Thrown when the slot is already unbound.</exception>
    internal void Unbind(string function)
    {
        if (current == 0)
            throw new GlNotBoundException(function, "attempted to unbind, but nothing is bound.");
        current = 0;
    }
}
