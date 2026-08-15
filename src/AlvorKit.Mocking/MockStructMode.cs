namespace AlvorKit;

/// <summary>
/// Identifies how an intercepted struct call is selected without assigning
/// reference identity to value storage.
/// </summary>
public enum MockStructMode
{
    /// <summary>
    /// Selects every matching operation on the struct type in the current
    /// session, including operations on assigned, passed, returned, or boxed
    /// copies.
    /// </summary>
    TypeWide,

    /// <summary>
    /// Evaluates a typed predicate synchronously against the live entry value
    /// for each call. A copied or boxed value is evaluated as that copy.
    /// </summary>
    ValueMatched,

    /// <summary>
    /// Selects one exact interception call site independently of equal receiver
    /// values at other sites.
    /// </summary>
    CallSite
}
