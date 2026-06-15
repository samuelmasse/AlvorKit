namespace AlvorKit.OpenGL.Layer;

/// <summary>
/// Exception thrown when a call needs another piece of state set first.
/// </summary>
/// <param name="function">The GL function whose strict-usage contract was violated.</param>
/// <param name="message">The violation details to append after the GL function name.</param>
public sealed class GlMissingPrerequisiteException(string function, string message) : GlException(function, message);
