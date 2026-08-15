namespace AlvorKit;

/// <summary>Identifies one terminal behavior for an intercepted struct call.</summary>
internal enum MockStructBehaviorKind
{
    Callback,
    Return,
    ReturnFactory,
    Throw,
    Passthrough,
    Strict
}
