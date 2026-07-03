namespace AlvorKit.UI;

[DebuggerDisplay("{DebugDisplay,nq}")]
[DebuggerTypeProxy(typeof(UiCallback<>.DebugView))]
public readonly struct UiCallback<T>(T value)
{
    private readonly T value = value;

    private sealed class DebugView(UiCallback<T> cb)
    {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public object? Value => cb.value;
    }

    public static implicit operator UiCallback<T>(T value) => new(value);

    public T Resolve() => value;
    public override string ToString() => DebugDisplay;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebugDisplay => value is Delegate del ? UiDebug.FormatDelegate(del) : UiDebug.Format(value);
}
