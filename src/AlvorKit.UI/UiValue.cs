namespace AlvorKit.UI;

[DebuggerDisplay("{DebugDisplay,nq}")]
[DebuggerTypeProxy(typeof(UiValue<>.DebugView))]
public readonly struct UiValue<T>(T value)
{
    private readonly T value = value;

    private sealed class DebugView(UiValue<T> prop)
    {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public object? Value => prop.value;
    }

    public static implicit operator UiValue<T>(T value) => new(value);
    public static implicit operator T(UiValue<T> prop) => prop.value;

    public T Resolve() => value;

    public override string ToString() => DebugDisplay;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebugDisplay => UiDebug.Format(value);
}
