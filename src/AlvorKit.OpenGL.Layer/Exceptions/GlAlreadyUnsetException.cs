namespace AlvorKit.OpenGL.Layer;

/// <summary>
/// Exception thrown when a state value is reset while it is not set.
/// </summary>
/// <param name="function">The GL function whose strict-usage contract was violated.</param>
/// <param name="message">The violation details to append after the GL function name.</param>
public sealed class GlAlreadyUnsetException(string function, string message) : GlException(function, message);
