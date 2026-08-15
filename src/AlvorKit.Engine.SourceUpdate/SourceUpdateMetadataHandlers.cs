namespace AlvorKit;

/// <summary>Invokes the managed Hot Reload cache-notification contract after a committed update.</summary>
internal static class SourceUpdateMetadataHandlers
{
    internal static string[] Notify(Type[] changedTypes)
    {
        var warnings = new List<string>();
        MetadataUpdateHandlerAttribute[] attributes;
        try
        {
            attributes =
            [
                .. DependencyOrder(AppDomain.CurrentDomain.GetAssemblies())
                    .SelectMany(static assembly =>
                        assembly.GetCustomAttributes<MetadataUpdateHandlerAttribute>())
            ];
        }
        catch (Exception exception)
        {
            return [$"Metadata-update handler discovery failed: {exception.Message}"];
        }

        var handlers = attributes
            .Select(static attribute => attribute.HandlerType)
            .Distinct()
            .ToArray();
        Invoke(handlers, "ClearCache", changedTypes, warnings);
        Invoke(handlers, "UpdateApplication", changedTypes, warnings);
        return [.. warnings];
    }

    private static void Invoke(
        Type[] handlers,
        string methodName,
        Type[] changedTypes,
        List<string> warnings)
    {
        foreach (var handler in handlers)
        {
            MethodInfo? method;
            try
            {
                method = handler.GetMethod(
                    methodName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                    [typeof(Type[])]);
            }
            catch (Exception exception)
            {
                warnings.Add($"{handler.FullName}.{methodName} resolution failed: {exception.Message}");
                continue;
            }

            if (method is null)
                continue;

            try
            {
                method.Invoke(null, [changedTypes]);
            }
            catch (Exception exception)
            {
                var cause = exception is TargetInvocationException { InnerException: { } inner }
                    ? inner
                    : exception;
                warnings.Add($"{handler.FullName}.{methodName} failed: {cause.Message}");
            }
        }
    }

    private static Assembly[] DependencyOrder(Assembly[] assemblies)
    {
        var byName = assemblies
            .Where(static assembly => !assembly.IsDynamic)
            .GroupBy(static assembly => assembly.GetName().Name, StringComparer.Ordinal)
            .ToDictionary(static group => group.Key!, static group => group.First(), StringComparer.Ordinal);
        var ordered = new List<Assembly>(byName.Count);
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        foreach (var assembly in byName.Values.OrderBy(static assembly => assembly.FullName, StringComparer.Ordinal))
            Visit(assembly, byName, visiting, visited, ordered);
        return [.. ordered];
    }

    private static void Visit(
        Assembly assembly,
        Dictionary<string, Assembly> byName,
        HashSet<string> visiting,
        HashSet<string> visited,
        List<Assembly> ordered)
    {
        var name = assembly.GetName().Name!;
        if (!visited.Add(name))
            return;
        if (!visiting.Add(name))
            return;

        foreach (var reference in assembly.GetReferencedAssemblies())
        {
            if (reference.Name is { } dependencyName &&
                byName.TryGetValue(dependencyName, out var dependency))
            {
                Visit(dependency, byName, visiting, visited, ordered);
            }
        }

        visiting.Remove(name);
        ordered.Add(assembly);
    }
}
