namespace AlvorKit;

/// <summary>Retains one mounted child view per key and orders a dedicated parent's children from supplied data.</summary>
/// <remarks>The parent contains only these children. Keys must be unique within each supplied collection.</remarks>
public class UiChildren<TKey, T> where TKey : notnull
{
    private readonly EntMut parent;
    private readonly Func<T, TKey> key;
    private readonly Func<EntMut, UiView<T>> create;
    private readonly List<EntMut> order = [];
    private Dictionary<TKey, UiView<T>> current = [];
    private Dictionary<TKey, UiView<T>> next = [];

    internal UiChildren(EntMut parent, Func<T, TKey> key, Func<EntMut, UiView<T>> create)
    {
        this.parent = parent;
        this.key = key;
        this.create = create;
    }

    /// <summary>Updates retained views, creates new keys, removes absent keys, and applies the supplied order.</summary>
    /// <remarks>
    /// Data setters run synchronously, including for new views; UI input and tick callbacks are not invoked.
    /// Removed nodes are reclaimed by the root UI's normal cleanup. Repeated sets reuse collection storage.
    /// </remarks>
    public void Set(ReadOnlySpan<T> values)
    {
        order.Clear();

        foreach (var value in values)
        {
            var childKey = key(value);

            if (!current.TryGetValue(childKey, out var view))
                view = create(parent);

            next.Add(childKey, view);
            view.Set(value);
            order.Add(view.Root);
        }

        NodesClear(parent);

        foreach (var node in order)
            NodesAdd(parent, node);

        current.Clear();
        (current, next) = (next, current);
    }
}
