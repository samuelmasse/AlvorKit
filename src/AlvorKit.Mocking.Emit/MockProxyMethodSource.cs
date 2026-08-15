namespace AlvorKit;

/// <summary>
/// Resolves runtime-emitted proxy overrides to the source metadata that owns their
/// canonical exact callback shape.
/// </summary>
internal static class MockProxyMethodSource
{
    /// <summary>Returns a proxy override's class or interface source method.</summary>
    internal static MethodInfo Resolve(MethodInfo method)
    {
        Type? proxyType = method.DeclaringType;
        if (proxyType is null ||
            !typeof(IMock).IsAssignableFrom(proxyType))
        {
            return method;
        }

        MethodInfo definition = Definition(method);
        MethodInfo baseDefinition = Definition(
            definition.GetBaseDefinition());
        if (!SameMethod(definition, baseDefinition))
            return baseDefinition;

        foreach (Type interfaceType in proxyType.GetInterfaces())
        {
            if (interfaceType == typeof(IMock))
                continue;

            InterfaceMapping mapping =
                proxyType.GetInterfaceMap(interfaceType);
            for (int index = 0;
                 index < mapping.TargetMethods.Length;
                 index++)
            {
                if (SameMethod(
                    Definition(mapping.TargetMethods[index]),
                    definition))
                {
                    return Definition(mapping.InterfaceMethods[index]);
                }
            }
        }

        return method;
    }

    private static MethodInfo Definition(MethodInfo method) =>
        method.IsGenericMethod
            ? method.GetGenericMethodDefinition()
            : method;

    private static bool SameMethod(
        MethodInfo first,
        MethodInfo second) =>
        first.Module == second.Module &&
        first.MetadataToken == second.MetadataToken;
}
