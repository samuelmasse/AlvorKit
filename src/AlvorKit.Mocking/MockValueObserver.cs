namespace AlvorKit;

/// <summary>Observes one live field value without retaining or boxing it.</summary>
/// <typeparam name="T">The exact field value type.</typeparam>
/// <param name="value">The live field value owned by the interception typed frame.</param>
public delegate void MockValueObserver<T>(scoped in T value)
    where T : allows ref struct;
