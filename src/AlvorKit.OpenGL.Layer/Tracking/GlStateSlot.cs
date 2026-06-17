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
        RequireCanSet(function, value);
        SetKnownUnset(value);
    }

    /// <summary>
    /// Clears the state value recorded for this slot.
    /// </summary>
    /// <param name="function">The GL function that requested the reset.</param>
    /// <exception cref="GlAlreadyUnsetException">Thrown when the slot is already unset.</exception>
    internal void Reset(string function)
    {
        RequireCanReset(function);
        ResetKnownSet();
    }

    /// <summary>
    /// Requires this slot to be unset before a later set commit.
    /// </summary>
    /// <param name="function">The GL function that requested the state change.</param>
    /// <param name="value">The value that would be recorded for the slot.</param>
    internal readonly void RequireCanSet(string function, T value)
    {
        if (current.HasValue)
            throw new GlAlreadySetException(function, $"attempted to set {value}, but {current.Value} is already set.");
    }

    /// <summary>
    /// Requires this slot to be set before a later reset commit.
    /// </summary>
    /// <param name="function">The GL function that requested the reset.</param>
    internal readonly void RequireCanReset(string function)
    {
        if (!current.HasValue)
            throw new GlAlreadyUnsetException(function, "attempted to reset, but nothing is set.");
    }

    /// <summary>
    /// Commits a set after all validation and backend work has succeeded.
    /// </summary>
    /// <param name="value">The value to record for the slot.</param>
    internal void SetKnownUnset(T value)
    {
        current = value;
    }

    /// <summary>
    /// Commits a reset after all validation and backend work has succeeded.
    /// </summary>
    internal void ResetKnownSet()
    {
        current = null;
    }
}
