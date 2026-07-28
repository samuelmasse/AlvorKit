namespace AlvorKit.Mocking;

/// <summary>Validates a generated exact site delegate against its operation.</summary>
internal static class MockInterceptionDelegateContract
{
    /// <summary>
    /// Validates the explicit receiver followed by every declared method
    /// parameter and the exact return metadata.
    /// </summary>
    internal static MethodInfo Validate(
        MethodInfo invoke,
        MethodInfo operation,
        MockInterceptionSiteDescriptor site)
    {
        bool receiverFree =
            site.OperationKind != MockInvocationOperationKind.InstanceMethod;
        if (receiverFree != operation.IsStatic ||
            operation.DeclaringType is null)
        {
            throw Failure(
                site,
                receiverFree
                    ? "a static-method site requires a static method"
                    : "an instance-method site requires a non-static method");
        }
        if (operation.ContainsGenericParameters ||
            operation.DeclaringType.ContainsGenericParameters)
        {
            throw Failure(
                site,
                "the executable instance signature still contains open generic parameters");
        }

        ValidateParameter(
            invoke.ReturnParameter,
            operation.ReturnParameter,
            site,
            "return");
        ParameterInfo[] actual = invoke.GetParameters();
        ParameterInfo[] expected = operation.GetParameters();
        int receiverCount = receiverFree ? 0 : 1;
        if (actual.Length != expected.Length + receiverCount)
        {
            throw Failure(
                site,
                $"the generated delegate has {actual.Length} parameters; " +
                $"expected {receiverCount} receiver parameters plus " +
                $"{expected.Length} declared parameters");
        }

        if (!receiverFree &&
            (actual[0].ParameterType != operation.DeclaringType ||
             actual[0].ParameterType.IsByRef))
        {
            throw Failure(
                site,
                $"receiver type '{actual[0].ParameterType}' does not exactly " +
                $"match '{operation.DeclaringType}'");
        }

        for (int index = 0; index < expected.Length; index++)
        {
            ValidateParameter(
                actual[index + receiverCount],
                expected[index],
                site,
                $"parameter {index}");
        }

        return invoke;
    }

    private static void ValidateParameter(
        ParameterInfo actual,
        ParameterInfo expected,
        MockInterceptionSiteDescriptor site,
        string location)
    {
        if (actual.ParameterType != expected.ParameterType ||
            actual.IsIn != expected.IsIn ||
            actual.IsOut != expected.IsOut ||
            !actual.GetRequiredCustomModifiers().SequenceEqual(
                expected.GetRequiredCustomModifiers()) ||
            !actual.GetOptionalCustomModifiers().SequenceEqual(
                expected.GetOptionalCustomModifiers()) ||
            HasScopedRef(actual) != HasScopedRef(expected))
        {
            throw Failure(
                site,
                $"generated delegate {location} metadata does not exactly " +
                "match the intercepted operation");
        }
    }

    private static bool HasScopedRef(ParameterInfo parameter) =>
        parameter.GetCustomAttributesData().Any(static attribute =>
            attribute.AttributeType.FullName ==
            "System.Runtime.CompilerServices.ScopedRefAttribute");

    private static MockException Failure(
        MockInterceptionSiteDescriptor site,
        string detail) =>
        new($"Interception site '{site}' is invalid because {detail}.");
}
