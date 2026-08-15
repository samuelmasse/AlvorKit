namespace AlvorKit;

/// <summary>
/// Mutates writable live struct storage synchronously at an interception
/// phase without retaining its managed reference.
/// </summary>
/// <typeparam name="T">The exact non-ref struct receiver type.</typeparam>
/// <param name="value">
/// The invocation-local receiver storage. The reference expires when the
/// delegate returns.
/// </param>
public delegate void MockStructMutation<T>(scoped ref T value)
    where T : struct;
