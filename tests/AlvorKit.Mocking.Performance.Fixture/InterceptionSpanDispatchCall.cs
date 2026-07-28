namespace AlvorKit.Mocking.Performance.Fixture;

/// <summary>
/// Calls one span operation through an exact receiver-first frame.
/// </summary>
internal delegate int InterceptionSpanDispatchCall(
    ConfiguredTypedDispatchTarget target,
    Span<int> values);
