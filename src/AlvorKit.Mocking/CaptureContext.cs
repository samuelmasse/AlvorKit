namespace AlvorKit.Mocking;

/// <summary>Thread-local state describing the invocation currently being captured for setup or event raising.</summary>
/// <param name="IsActive">Whether this thread is currently capturing an invocation.</param>
/// <param name="IsDisambiguating">Whether this capture pass is writing matcher fingerprints.</param>
/// <param name="Instance">The captured mocked object instance.</param>
/// <param name="Method">The captured method or accessor.</param>
/// <param name="Args">The captured method arguments in mock matching order.</param>
internal record struct CaptureContext(
    bool IsActive,
    bool IsDisambiguating,
    object? Instance,
    MethodInfo? Method,
    object?[]? Args);
