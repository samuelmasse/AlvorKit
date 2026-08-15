namespace AlvorKit;

/// <summary>Builds shared direct delegate types for supported exact callbacks.</summary>
internal static class MockTypedCallbackDelegateShape
{
    /// <summary>Returns the direct delegate type or null when an exact emitted delegate is required.</summary>
    internal static Type? Create(
        Type returnType,
        IReadOnlyList<Type> parameterTypes)
    {
        if (CannotBeGenericArgument(returnType))
            return null;
        foreach (Type parameterType in parameterTypes)
        {
            if (CannotBeGenericArgument(parameterType))
                return null;
        }

        if (returnType == typeof(void))
        {
            if (parameterTypes.Count == 0)
                return typeof(Action);
            return CreateStandard(
                nameof(Action),
                [.. parameterTypes]);
        }

        return CreateStandard(
            nameof(Func<>),
            [.. parameterTypes, returnType]);
    }

    /// <summary>Builds the direct delegate type from one closed runtime method.</summary>
    internal static Type? Create(MethodInfo method)
    {
        Type? candidate = Create(
            method.ReturnType,
            [.. method.GetParameters().Select(static parameter =>
                parameter.ParameterType)]);
        if (candidate is null)
            return null;

        try
        {
            MockTypedCallbackContract.ValidateInvoke(
                candidate.GetMethod(nameof(Action.Invoke))!,
                method);
            return candidate;
        }
        catch (MockException)
        {
            return null;
        }
    }

    /// <summary>Builds the direct delegate type from emitted generic proxy shapes.</summary>
    internal static Type? Create(
        Type returnType,
        IReadOnlyList<MockIlParameter> parameters,
        MethodInfo source)
    {
        if (Create(source) is null)
            return null;

        return Create(
            returnType,
            [.. parameters.Select(static parameter => parameter.Type)]);
    }

    private static Type? CreateStandard(
        string name,
        Type[] arguments)
    {
        Type? definition = typeof(Action).Assembly.GetType(
            $"System.{name}`{arguments.Length}");
        return definition?.MakeGenericType(arguments);
    }

    private static bool CannotBeGenericArgument(Type type) =>
        type.IsByRef
        || type.IsPointer
        || type.IsFunctionPointer;
}
