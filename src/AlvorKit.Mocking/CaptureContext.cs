namespace AlvorKit;

/// <summary>Thread-local state describing one setup, verification, or event capture.</summary>
/// <param name="IsActive">Whether this thread is currently capturing an invocation.</param>
/// <param name="IsDisambiguating">Whether this capture is replaying ordinary matcher placement.</param>
/// <param name="Operation">Why the invocation is being captured.</param>
/// <param name="ExpectedOperationKind">
/// Optional receiver-free kind that may accept the invocation.
/// </param>
/// <param name="InvocationCount">How many mocked calls were observed in this pass.</param>
/// <param name="Instance">The captured mocked object instance.</param>
/// <param name="Method">The captured method or accessor.</param>
/// <param name="Args">The captured method arguments in mock matching order.</param>
internal record struct CaptureContext(
    bool IsActive,
    bool IsDisambiguating,
    CaptureOperation Operation,
    MockInvocationOperationKind? ExpectedOperationKind,
    int InvocationCount,
    object? Instance,
    MethodInfo? Method,
    object?[]? Args);
