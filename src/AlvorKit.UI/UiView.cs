namespace AlvorKit;

/// <summary>A mounted subtree and the data setter that updates its retained content.</summary>
/// <param name="root">The subtree's mounted root node.</param>
/// <param name="set">Applies data synchronously without running input or tick callbacks.</param>
public readonly struct UiView<T>(EntMut root, Action<T> set)
{
    /// <summary>Gets the mounted root whose identity and local state survive data changes.</summary>
    public EntMut Root => root;

    /// <summary>Applies initial or replacement data before the next layout.</summary>
    public void Set(T value) => set(value);
}
