namespace AlvorKit.Mocking;

/// <summary>
/// Captures one void struct operation through a live invocation-local
/// reference to its current storage.
/// </summary>
/// <typeparam name="T">The intercepted non-ref struct type.</typeparam>
/// <param name="value">
/// The capture-only receiver. It is never retained as setup identity.
/// </param>
public delegate void MockStructCall<T>(scoped ref T value)
    where T : struct;

/// <summary>
/// Captures one value-returning struct operation through a live
/// invocation-local reference to its current storage.
/// </summary>
/// <typeparam name="T">The intercepted non-ref struct type.</typeparam>
/// <typeparam name="TResult">The exact operation return type.</typeparam>
/// <param name="value">
/// The capture-only receiver. It is never retained as setup identity.
/// </param>
/// <returns>The operation result during capture or dispatch.</returns>
public delegate TResult MockStructCall<T, TResult>(
    scoped ref T value)
    where T : struct
    where TResult : allows ref struct;
