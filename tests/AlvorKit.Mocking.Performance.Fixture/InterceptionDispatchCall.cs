namespace AlvorKit;

/// <summary>
/// Calls one ordinary concrete operation through an exact receiver-first frame.
/// </summary>
internal delegate int InterceptionDispatchCall(
    InterceptionDispatchTarget target,
    int value);
