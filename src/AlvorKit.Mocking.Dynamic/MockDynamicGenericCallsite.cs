namespace AlvorKit;

/// <summary>
/// Resolves capture-time generic call sites to constructed runtime methods.
/// </summary>
internal static class MockDynamicGenericCallsite
{
    private const int MaximumReceiverDepth = 4;

    /// <summary>
    /// Installs each constructed generic specialization referenced by a capture delegate.
    /// </summary>
    internal static void Prepare(Delegate capture)
    {
        List<MethodInfo> references = MockGenericIlReader.Read(capture);
        if (references.Count == 0)
            return;

        var receivers = new List<object>();
        FindReceivers(
            capture.Target,
            receivers,
            new HashSet<object>(ReferenceEqualityComparer.Instance),
            MaximumReceiverDepth,
            capture.Method.Module.Assembly);

        foreach (MethodInfo reference in references)
        {
            foreach (object receiver in receivers)
            {
                MethodInfo? runtimeMethod = ResolveRuntimeMethod(
                    reference,
                    receiver.GetType());
                if (runtimeMethod is not null
                    && !IsProxyOwned(runtimeMethod)
                    && !MockInterceptionMethodRegistry.Contains(runtimeMethod))
                {
                    throw new MockException(
                        $"Concrete generic method " +
                        $"'{runtimeMethod.DeclaringType!.FullName}." +
                        $"{runtimeMethod.Name}' requires an owned interception call " +
                        "site before it can be configured.");
                }
            }
        }
    }

    private static void FindReceivers(
        object? candidate,
        List<object> receivers,
        HashSet<object> visited,
        int depth,
        Assembly captureAssembly)
    {
        if (candidate is null || !visited.Add(candidate))
            return;
        if (Mock.GetMocked(candidate) is not null)
        {
            receivers.Add(candidate);
            return;
        }
        if (depth == 0 || !ShouldInspect(candidate.GetType(), captureAssembly))
            return;

        if (candidate is Delegate nested)
            FindReceivers(
                nested.Target,
                receivers,
                visited,
                depth - 1,
                captureAssembly);

        foreach (FieldInfo field in candidate.GetType().GetFields(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            FindReceivers(
                field.GetValue(candidate),
                receivers,
                visited,
                depth - 1,
                captureAssembly);
        }
    }

    private static bool ShouldInspect(
        Type type,
        Assembly captureAssembly)
    {
        return type.Assembly == captureAssembly
            || type.IsDefined(typeof(CompilerGeneratedAttribute), false);
    }

    private static MethodInfo? ResolveRuntimeMethod(
        MethodInfo reference,
        Type runtimeType)
    {
        MethodInfo definition = reference.GetGenericMethodDefinition();
        Type declaringType = definition.DeclaringType!;
        MethodInfo? runtimeDefinition = declaringType.IsInterface
            ? ResolveInterfaceMethod(runtimeType, definition)
            : ResolveClassMethod(runtimeType, definition);
        return runtimeDefinition?.MakeGenericMethod(
            reference.GetGenericArguments());
    }

    private static MethodInfo? ResolveInterfaceMethod(
        Type runtimeType,
        MethodInfo definition)
    {
        if (!definition.DeclaringType!.IsAssignableFrom(runtimeType))
            return null;

        InterfaceMapping mapping = runtimeType.GetInterfaceMap(
            definition.DeclaringType);
        for (int index = 0; index < mapping.InterfaceMethods.Length; index++)
        {
            MethodInfo interfaceMethod = GetDefinition(
                mapping.InterfaceMethods[index]);
            if (SameDefinition(interfaceMethod, definition))
                return GetDefinition(mapping.TargetMethods[index]);
        }

        return null;
    }

    private static MethodInfo? ResolveClassMethod(
        Type runtimeType,
        MethodInfo definition)
    {
        if (!definition.DeclaringType!.IsAssignableFrom(runtimeType))
            return null;

        foreach (MethodInfo candidate in runtimeType.GetMethods(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (!candidate.IsGenericMethodDefinition)
                continue;

            MethodInfo baseDefinition = GetDefinition(
                candidate.GetBaseDefinition());
            if (SameDefinition(candidate, definition)
                || SameDefinition(baseDefinition, definition))
            {
                return candidate;
            }
        }

        return null;
    }

    private static MethodInfo GetDefinition(MethodInfo method) =>
        method.IsGenericMethod ? method.GetGenericMethodDefinition() : method;

    private static bool SameDefinition(
        MethodInfo first,
        MethodInfo second)
    {
        return ReferenceEquals(first.Module, second.Module)
            && first.MetadataToken == second.MetadataToken;
    }

    private static bool IsProxyOwned(MethodInfo method)
    {
        Type declaringType = method.DeclaringType!;
        return typeof(IMock).IsAssignableFrom(declaringType);
    }
}
