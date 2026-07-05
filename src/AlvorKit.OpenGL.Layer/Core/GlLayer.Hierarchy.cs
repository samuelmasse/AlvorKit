namespace AlvorKit.OpenGL.Layer;

public unsafe partial class GlLayer
{
    /// <summary>
    /// Finds the node in this layer's ancestry (self first) whose resource set tracks a handle.
    /// </summary>
    /// <typeparam name="THandle">The typed handle used by the GL resource family.</typeparam>
    /// <param name="setOf">Selects the resource family's set on a node.</param>
    /// <param name="handle">The handle to look up.</param>
    /// <returns>The owning node, or null when no ancestor tracks the handle.</returns>
    private GlLayer? FindOwner<THandle>(Func<GlLayer, GlResourceSet<THandle>> setOf, THandle handle)
        where THandle : unmanaged
    {
        for (var node = this; node is not null; node = node.parent)
            if (setOf(node).Contains(handle))
                return node;
        return null;
    }

    /// <summary>
    /// Views a native handle array and verifies every handle is tracked by this node or an
    /// ancestor, so children may delete objects their scope inherited.
    /// </summary>
    /// <typeparam name="THandle">The typed handle used by the GL resource family.</typeparam>
    /// <param name="function">The GL function that requested the validation.</param>
    /// <param name="setOf">Selects the resource family's set on a node.</param>
    /// <param name="count">The number of handles in the native array.</param>
    /// <param name="handles">The native pointer to the first raw id.</param>
    /// <returns>A typed span over the native handle array.</returns>
    private ReadOnlySpan<THandle> RequireTrackedInTree<THandle>(
        string function,
        Func<GlLayer, GlResourceSet<THandle>> setOf,
        int count,
        nint handles)
        where THandle : unmanaged
    {
        var values = GlResourceSet<THandle>.NativeHandleSpan(count, handles);
        foreach (var value in values)
            if (FindOwner(setOf, value) is null)
                setOf(this).Untrack(function, value);
        return values;
    }

    /// <summary>
    /// Removes handles already validated by <see cref="RequireTrackedInTree{THandle}"/> from the
    /// nodes that own them.
    /// </summary>
    /// <typeparam name="THandle">The typed handle used by the GL resource family.</typeparam>
    /// <param name="setOf">Selects the resource family's set on a node.</param>
    /// <param name="handles">The tracked handles to remove.</param>
    private void UntrackInTree<THandle>(Func<GlLayer, GlResourceSet<THandle>> setOf, ReadOnlySpan<THandle> handles)
        where THandle : unmanaged
    {
        foreach (var value in handles)
            FindOwner(setOf, value)?.OwnUntrack(setOf, value);
    }

    /// <summary>
    /// Removes one known-tracked handle from this node's resource set.
    /// </summary>
    /// <typeparam name="THandle">The typed handle used by the GL resource family.</typeparam>
    /// <param name="setOf">Selects the resource family's set on a node.</param>
    /// <param name="handle">The tracked handle to remove.</param>
    private void OwnUntrack<THandle>(Func<GlLayer, GlResourceSet<THandle>> setOf, THandle handle)
        where THandle : unmanaged =>
        setOf(this).TryUntrack(handle);
}
