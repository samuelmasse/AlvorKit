namespace AlvorKit;

/// <summary>
/// Calls one ref/out operation through an exact receiver-first frame.
/// </summary>
internal delegate int InterceptionRefOutDispatchCall(
    PartialRefOutDispatchTarget target,
    ref int value,
    out int doubled);
