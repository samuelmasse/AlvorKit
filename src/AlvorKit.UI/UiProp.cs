namespace AlvorKit.UI;

[DebuggerDisplay("{DebugDisplay,nq}")]
[DebuggerTypeProxy(typeof(UiProp<>.DebugView))]
public readonly struct UiProp<T>(T value, Func<T>? func)
{
    private readonly T value = value;
    private readonly Func<T>? func = func;

    private sealed class DebugView(UiProp<T> prop)
    {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public object? Value => prop.func != null ? prop.func : prop.value;
    }

    public static implicit operator UiProp<T>(T value) => new(value, null);
    public static implicit operator UiProp<T>(Func<T> func) => new(default!, func);

    public T Resolve() => func != null ? func() : value;
    public override string ToString() => DebugDisplay;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebugDisplay => func != null ? UiDebug.FormatDelegate(func) : UiDebug.Format(value);
}
