namespace AlvorKit;

/// <summary>
/// Calls one closed generic span operation through an exact receiver-first
/// frame.
/// </summary>
internal delegate int InterceptionColdTypedDispatchCall<TTag>(
    ColdTypedDispatchTarget<TTag> target,
    Span<int> values);
