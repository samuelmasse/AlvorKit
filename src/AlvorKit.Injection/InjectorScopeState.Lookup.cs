namespace AlvorKit.Injection;

/// <summary>
/// Handler, attribute, and include lookup helpers for <see cref="InjectorScopeState"/>.
/// </summary>
public partial record InjectorScopeState
{
    /// <summary>
    /// Gets the single injector attribute type applied to <paramref name="type"/>, if any.
    /// </summary>
    public Type? GetInjectorAttributeType(Type type, InjectorPath path)
    {
        if (path.InjectorAttributeTypeCache.TryGetValue(type, out var val))
            return val;

        Type? injectorAttributeType = null;

        foreach (var attr in type.GetCustomAttributes())
        {
            var attrType = attr.GetType();

            if (attrType.BaseType == typeof(InjectorAttribute))
            {
                if (injectorAttributeType != null)
                    throw new InjectorException(path, $"Type '{type.FullName}' has " +
                        $"too many attributes that derive from InjectorAttribute.");

                injectorAttributeType = attrType;
            }
        }

        path.InjectorAttributeTypeCache.Add(type, injectorAttributeType);
        return injectorAttributeType;
    }

    /// <summary>
    /// Finds the first custom handler in this scope chain that accepts <paramref name="type"/>.
    /// </summary>
    private InjectorHandler FindHandler(Type type, InjectorPath path)
    {
        var state = this;

        while (state != null)
        {
            if (state.handlers != null)
            {
                foreach (var handler in state.handlers)
                {
                    bool handles;

                    try
                    {
                        handles = handler.Handles(type);
                    }
                    catch (Exception e)
                    {
                        throw new InjectorException(path, $"Custom handler '{handler}' threw an exception while checking " +
                            $"if it can handle type '{type.FullName}'.", e);
                    }

                    if (handles)
                        return handler;
                }
            }

            state = state.Parent;
        }

        return InjectorDefaultHandler.Instance;
    }

    /// <summary>
    /// Returns whether <paramref name="type"/> is included by this scope chain.
    /// </summary>
    private bool IsIncluded(Type type)
    {
        var fullName = type.FullName;
        bool includeByDefault = true;
        var state = this;

        while (state != null)
        {
            if (state.includes != null)
            {
                foreach (var pattern in state.includes)
                {
                    includeByDefault = false;

                    if (fullName != null && pattern.Match(fullName).Success)
                        return true;
                }
            }

            state = state.Parent;
        }

        return includeByDefault;
    }
}
