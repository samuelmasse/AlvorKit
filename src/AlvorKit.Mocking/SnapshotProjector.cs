namespace AlvorKit;

/// <summary>Copies one live argument into a heap-safe history representation.</summary>
/// <typeparam name="T">The exact live argument type.</typeparam>
/// <typeparam name="TResult">The heap-safe retained snapshot type.</typeparam>
/// <param name="value">The invocation-local value to project synchronously.</param>
/// <returns>A heap-safe value that may be retained in invocation history.</returns>
public delegate TResult SnapshotProjector<T, TResult>(
    scoped in T value)
    where T : allows ref struct;
