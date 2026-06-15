namespace AlvorKit.OpenGL.Layer;

/// <summary>
/// Exception thrown when a bind would conflict with another live binding.
/// </summary>
/// <param name="function">The GL function whose strict-usage contract was violated.</param>
/// <param name="message">The violation details to append after the GL function name.</param>
public sealed class GlBindConflictException(string function, string message) : GlException(function, message);
