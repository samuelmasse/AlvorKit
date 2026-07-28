namespace AlvorKit.Mocking.Performance.Fixture;

/// <summary>
/// Calls one value-and-span operation through an exact receiver-first frame.
/// </summary>
internal delegate int InterceptionTypedDispatchCall(
    TypedDispatchTarget target,
    int value,
    Span<int> values);
