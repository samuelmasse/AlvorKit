namespace AlvorKit.Injection;

/// <summary>
/// Exception thrown when a scope is invalid, uninitialized, or nested in an unsupported way.
/// </summary>
public class InjectorScopeException(InjectorScope scope, string message) : Exception(GetMessage(scope, message))
{
    /// <summary>
    /// Formats an error message with the active injector scope stack.
    /// </summary>
    public static string GetMessage(InjectorScope scope, string message)
    {
        var stack = new Stack<InjectorScopeState>();

        var node = scope.State;
        while (node != null)
        {
            stack.Push(node);
            node = node.Parent;
        }

        return message + "\nScope: " + string.Join(" -> ", stack.Select(t => t.AttributeType?.FullName ?? "Root"));
    }
}
