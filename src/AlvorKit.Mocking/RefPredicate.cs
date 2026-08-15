namespace AlvorKit;

/// <summary>
/// Evaluates one live argument synchronously without retaining or boxing it.
/// </summary>
/// <typeparam name="T">The exact live argument type.</typeparam>
/// <param name="value">The borrowed argument value.</param>
/// <returns><see langword="true"/> when the argument matches.</returns>
public delegate bool RefPredicate<T>(scoped in T value)
    where T : allows ref struct;
