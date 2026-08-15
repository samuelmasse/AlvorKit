namespace AlvorKit;

/// <summary>
/// Captures one immutable, heap-safe behavior claim selected for an active
/// invocation.
/// </summary>
internal readonly record struct MockBehaviorExecution(
    MockBehaviorExecutionKind Kind,
    object? Value,
    object?[] ReferenceValues,
    Delegate? Callback);
