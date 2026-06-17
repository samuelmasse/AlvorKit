namespace AlvorKit.Injection;

/// <summary>
/// Exception thrown when dependency resolution fails inside an initialized injector scope.
/// </summary>
public class InjectorException(InjectorPath path, string message, Exception? innerException) :
    Exception(GetMessage(path, message), innerException)
{
    /// <summary>
    /// Creates an injector exception with the current resolution path and no inner exception.
    /// </summary>
    public InjectorException(InjectorPath path, string message) : this(path, message, null) { }

    /// <summary>
    /// Formats an error message with the active dependency resolution path.
    /// </summary>
    public static string GetMessage(InjectorPath path, string message) =>
        message + "\nPath: " + string.Join(" -> ", path.Stack.Reverse().Select(t => t.FullName));
}
