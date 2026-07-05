namespace AlvorKit.OpenGL.Layer;

/// <summary>
/// Tracks a family of live GL resources and converts between generated ids and typed handles.
/// </summary>
/// <typeparam name="THandle">The typed handle used by the GL resource family.</typeparam>
/// <param name="name">The display name used in tracking exceptions.</param>
internal sealed unsafe partial class GlResourceSet<THandle>(
    string name) where THandle : unmanaged
{
    /// <summary>
    /// Stores the tracked live handles.
    /// </summary>
    private readonly HashSet<THandle> items = [];

    /// <summary>
    /// Gets the tracked live handles.
    /// </summary>
    internal IReadOnlySet<THandle> Items => items;

    /// <summary>
    /// Gets the number of tracked live handles.
    /// </summary>
    internal int Count => items.Count;

    /// <summary>
    /// Returns whether the specified handle is tracked.
    /// </summary>
    /// <param name="handle">The handle to look up.</param>
    /// <returns><see langword="true"/> when the handle is tracked; otherwise, <see langword="false"/>.</returns>
    internal bool Contains(THandle handle) => items.Contains(handle);

    /// <summary>
    /// Adds one handle to the tracked live set.
    /// </summary>
    /// <param name="handle">The handle to track.</param>
    internal void Track(THandle handle) => items.Add(handle);

    /// <summary>
    /// Adds a contiguous native array of raw ids to the tracked live set.
    /// </summary>
    /// <param name="count">The number of handles in the native array.</param>
    /// <param name="handles">The native pointer to the first raw id.</param>
    internal void Track(int count, nint handles)
    {
        var ids = (uint*)handles;
        for (var i = 0; i < count; i++)
            Track(FromId(ids[i]));
    }

    /// <summary>
    /// Removes one handle from the tracked live set.
    /// </summary>
    /// <param name="function">The GL function that requested the removal.</param>
    /// <param name="handle">The handle to remove.</param>
    /// <exception cref="GlResourceNotTrackedException{THandle}">Thrown when the handle is not tracked.</exception>
    internal void Untrack(string function, THandle handle)
    {
        if (!items.Remove(handle))
            throw new GlResourceNotTrackedException<THandle>(function, name, handle);
    }

    /// <summary>
    /// Removes a contiguous native array of raw ids from the tracked live set.
    /// </summary>
    /// <param name="function">The GL function that requested the removal.</param>
    /// <param name="count">The number of handles in the native array.</param>
    /// <param name="handles">The native pointer to the first raw id.</param>
    /// <returns>A span over the native typed handles that were removed.</returns>
    /// <exception cref="GlResourceNotTrackedException{THandle}">Thrown when any handle is not tracked.</exception>
    internal ReadOnlySpan<THandle> Untrack(string function, int count, nint handles)
    {
        var values = RequireTracked(function, count, handles);
        UntrackKnown(values);
        return values;
    }

    /// <summary>
    /// Reads a native handle array and verifies every handle is currently tracked.
    /// </summary>
    /// <param name="function">The GL function that requested the validation.</param>
    /// <param name="count">The number of handles in the native array.</param>
    /// <param name="handles">The native pointer to the first raw id.</param>
    /// <returns>A span over the typed handles in the native array.</returns>
    internal ReadOnlySpan<THandle> RequireTracked(string function, int count, nint handles)
    {
        var values = NativeHandleSpan(count, handles);
        foreach (var value in values)
            if (!items.Contains(value))
                throw new GlResourceNotTrackedException<THandle>(function, name, value);
        return values;
    }

    /// <summary>
    /// Removes handles that were already validated as tracked.
    /// </summary>
    /// <param name="handles">The tracked handles to remove.</param>
    internal void UntrackKnown(ReadOnlySpan<THandle> handles)
    {
        foreach (var handle in handles)
            items.Remove(handle);
    }

    /// <summary>
    /// Removes one handle from the tracked live set when present.
    /// </summary>
    /// <param name="handle">The handle to remove.</param>
    /// <returns><see langword="true"/> when the handle was tracked and removed; otherwise, <see langword="false"/>.</returns>
    internal bool TryUntrack(THandle handle) => items.Remove(handle);

    /// <summary>
    /// Reads one tracked handle without removing it.
    /// </summary>
    /// <param name="handle">One tracked handle, or the default value when the set is empty.</param>
    /// <returns><see langword="true"/> when a handle was read; otherwise, <see langword="false"/>.</returns>
    internal bool TryPeek(out THandle handle)
    {
        foreach (var candidate in items)
        {
            handle = candidate;
            return true;
        }
        handle = default;
        return false;
    }

    /// <summary>
    /// Copies tracked handles into a caller-owned raw id buffer without removing them.
    /// </summary>
    /// <param name="destination">The caller-owned buffer that receives raw GL ids.</param>
    /// <returns>The number of raw ids written to <paramref name="destination"/>.</returns>
    internal int SnapshotIds(Span<uint> destination)
    {
        var count = 0;
        foreach (var handle in items)
        {
            if (count == destination.Length)
                break;
            destination[count++] = ToId(handle);
        }
        return count;
    }

    /// <summary>
    /// Removes one tracked handle without allocating a snapshot.
    /// </summary>
    /// <param name="handle">The handle that was removed, or the default value when the set is empty.</param>
    /// <returns><see langword="true"/> when a handle was removed; otherwise, <see langword="false"/>.</returns>
    internal bool TryDrain(out THandle handle)
    {
        var found = false;
        handle = default;
        foreach (var candidate in items)
        {
            handle = candidate;
            found = true;
            break;
        }
        if (!found)
            return false;
        items.Remove(handle);
        return true;
    }

    /// <summary>
    /// Removes tracked handles into a caller-owned raw id buffer without allocating a snapshot.
    /// </summary>
    /// <param name="destination">The caller-owned buffer that receives raw GL ids.</param>
    /// <returns>The number of raw ids written to <paramref name="destination"/>.</returns>
    internal int DrainIds(Span<uint> destination)
    {
        var count = 0;
        while (count < destination.Length && TryDrain(out var handle))
            destination[count++] = ToId(handle);
        return count;
    }

}
