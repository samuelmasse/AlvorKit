namespace AlvorKit.OpenGL.Layer;

/// <summary>
/// Tracks one strict state slot that may hold either no value or one active value.
/// </summary>
/// <typeparam name="T">The value type stored by the state slot.</typeparam>
internal struct GlStateSlot<T> where T : struct
{
    /// <summary>
    /// Stores the current state value, or <see langword="null"/> when the slot is unset.
    /// </summary>
    private T? current;

    /// <summary>
    /// Gets a value indicating whether the slot is currently set.
    /// </summary>
    internal readonly bool IsSet => current.HasValue;

    /// <summary>
    /// Gets the current state value, or <see langword="null"/> when the slot is unset.
    /// </summary>
    internal readonly T? Value => current;

    /// <summary>
    /// Records a non-overlapping state value for this slot.
    /// </summary>
    /// <param name="function">The GL function that requested the state change.</param>
    /// <param name="value">The value to record for the slot.</param>
    /// <exception cref="GlAlreadySetException">Thrown when the slot is already set.</exception>
    internal void Set(string function, T value)
    {
        if (current.HasValue)
            throw new GlAlreadySetException(function, $"attempted to set {value}, but {current.Value} is already set.");
        current = value;
    }

    /// <summary>
    /// Clears the state value recorded for this slot.
    /// </summary>
    /// <param name="function">The GL function that requested the reset.</param>
    /// <exception cref="GlAlreadyUnsetException">Thrown when the slot is already unset.</exception>
    internal void Reset(string function)
    {
        if (!current.HasValue)
            throw new GlAlreadyUnsetException(function, "attempted to reset, but nothing is set.");
        current = null;
    }
}
