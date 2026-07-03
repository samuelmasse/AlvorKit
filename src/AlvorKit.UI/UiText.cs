namespace AlvorKit.UI;

[DebuggerDisplay("{DebugDisplay,nq}")]
[DebuggerTypeProxy(typeof(DebugView))]
public readonly struct UiText(string value, Func<ReadOnlySpan<char>>? func)
{
    private readonly string value = value;
    private readonly Func<ReadOnlySpan<char>>? func = func;

    private sealed class DebugView(UiText text)
    {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public object? Value => text.func != null ? text.func : text.value;
    }

    public static implicit operator UiText(string value) => new(value, null);
    public static implicit operator UiText(Func<ReadOnlySpan<char>> func) => new(null!, func);

    public ReadOnlySpan<char> Resolve() => func != null ? func() : value;
    public override string ToString() => DebugDisplay;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebugDisplay => func != null ? UiDebug.FormatDelegate(func) : UiDebug.Format(value);
}
