namespace AlvorKit.OpenGL.Layer;

/// <summary>
/// Tracks a family of live GL resources and converts between generated ids and typed handles.
/// </summary>
/// <typeparam name="THandle">The typed handle used by the GL resource family.</typeparam>
/// <param name="name">The display name used in tracking exceptions.</param>
/// <param name="fromId">The converter from a raw generated GL id to a typed handle.</param>
/// <param name="toId">The converter from a typed handle back to a raw GL id.</param>
internal sealed unsafe class GlResourceSet<THandle>(
    string name,
    Func<uint, THandle> fromId,
    Func<THandle, uint> toId) where THandle : struct
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
            Track(fromId(ids[i]));
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
    /// <returns>The typed handles that were removed.</returns>
    /// <exception cref="GlResourceNotTrackedException{THandle}">Thrown when any handle is not tracked.</exception>
    internal THandle[] Untrack(string function, int count, nint handles)
    {
        var values = Read(count, handles);
        foreach (var value in values)
        {
            if (!items.Contains(value))
                throw new GlResourceNotTrackedException<THandle>(function, name, value);
        }
        foreach (var value in values)
            items.Remove(value);
        return values;
    }

    /// <summary>
    /// Removes every tracked handle and returns them as typed handles.
    /// </summary>
    /// <returns>The handles that were tracked before the drain.</returns>
    internal THandle[] Drain()
    {
        var values = new THandle[items.Count];
        items.CopyTo(values);
        items.Clear();
        return values;
    }

    /// <summary>
    /// Removes every tracked handle and returns them as raw ids.
    /// </summary>
    /// <returns>The raw GL ids that were tracked before the drain.</returns>
    internal uint[] DrainIds()
    {
        var values = Drain();
        var ids = new uint[values.Length];
        for (var i = 0; i < values.Length; i++)
            ids[i] = toId(values[i]);
        return ids;
    }

    /// <summary>
    /// Reads a contiguous native array of raw ids as typed handles.
    /// </summary>
    /// <param name="count">The number of handles in the native array.</param>
    /// <param name="handles">The native pointer to the first raw id.</param>
    /// <returns>The typed handles read from the native array.</returns>
    private THandle[] Read(int count, nint handles)
    {
        var ids = (uint*)handles;
        var values = new THandle[count];
        for (var i = 0; i < count; i++)
            values[i] = fromId(ids[i]);
        return values;
    }
}
