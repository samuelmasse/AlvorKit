namespace AlvorKit;

/// <summary>
/// Carries one immutable struct behavior without invoking a callback or
/// retaining live receiver storage.
/// </summary>
internal sealed class MockStructBehavior
{
    private MockStructBehavior(
        MockStructBehaviorKind kind,
        object? value = null,
        Delegate? callback = null,
        Exception? exception = null)
    {
        Kind = kind;
        Value = value;
        Callback = callback;
        Exception = exception;
    }

    /// <summary>Gets the selected execution behavior.</summary>
    internal MockStructBehaviorKind Kind { get; }

    /// <summary>Gets the configured heap-safe return value.</summary>
    internal object? Value { get; }

    /// <summary>Gets the configured exact callback or return factory.</summary>
    internal Delegate? Callback { get; }

    /// <summary>Gets the configured exception.</summary>
    internal Exception? Exception { get; }

    /// <summary>Creates an exact callback behavior.</summary>
    internal static MockStructBehavior CallbackBehavior(
        Delegate callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        return new(MockStructBehaviorKind.Callback, callback: callback);
    }

    /// <summary>Creates a heap-safe constant return behavior.</summary>
    internal static MockStructBehavior Return(object? value) =>
        new(MockStructBehaviorKind.Return, value: value);

    /// <summary>Creates an exact return-factory behavior.</summary>
    internal static MockStructBehavior ReturnFactory(Delegate factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        return new(
            MockStructBehaviorKind.ReturnFactory,
            callback: factory);
    }

    /// <summary>Creates an exception behavior.</summary>
    internal static MockStructBehavior Throw(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return new(MockStructBehaviorKind.Throw, exception: exception);
    }

    /// <summary>Creates a preserved-original behavior.</summary>
    internal static MockStructBehavior Passthrough() =>
        new(MockStructBehaviorKind.Passthrough);

    /// <summary>Creates an unexpected-invocation behavior.</summary>
    internal static MockStructBehavior Strict() =>
        new(MockStructBehaviorKind.Strict);
}
