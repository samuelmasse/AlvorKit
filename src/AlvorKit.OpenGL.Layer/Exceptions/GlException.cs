namespace AlvorKit.OpenGL.Layer;

/// <summary>
/// Base exception for every strict-usage violation the layer detects.
/// </summary>
/// <param name="function">The GL function whose strict-usage contract was violated.</param>
/// <param name="message">The violation details to append after the GL function name.</param>
public class GlException(string function, string message) : Exception($"gl{function}: {message}")
{
    /// <summary>
    /// Gets the GL function whose strict-usage contract was violated.
    /// </summary>
    public string Function { get; } = function;
}
