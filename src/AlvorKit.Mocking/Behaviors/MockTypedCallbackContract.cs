namespace AlvorKit.Mocking;

/// <summary>Validates and normalizes source callbacks to one stable exact delegate type.</summary>
internal static class MockTypedCallbackContract
{
    /// <summary>Validates and normalizes a callback before setup publication.</summary>
    internal static Delegate Normalize(
        Delegate callback,
        MethodInfo capturedMethod)
    {
        ArgumentNullException.ThrowIfNull(callback);
        ArgumentNullException.ThrowIfNull(capturedMethod);
        Validate(callback, capturedMethod);
        return MockRuntimeBackendRegistry.Proxy.NormalizeCallback(
            callback,
            capturedMethod);
    }

    /// <summary>Validates a source callback against one closed captured method.</summary>
    internal static void Validate(
        Delegate callback,
        MethodInfo capturedMethod)
    {
        if (capturedMethod.ContainsGenericParameters)
        {
            throw new MockException(
                "The captured callback signature must be closed.");
        }

        ValidateSourceInvoke(
            callback.GetType().GetMethod(nameof(Action.Invoke))!,
            capturedMethod);
        ValidateAsyncBoundary(callback, capturedMethod);
    }

    /// <summary>Validates an emitted stored delegate against every exact signature facet.</summary>
    internal static void ValidateInvoke(
        MethodInfo invoke,
        MethodInfo capturedMethod)
    {
        if (invoke.ReturnType != capturedMethod.ReturnType)
            throw Mismatch("return type");

        ValidateParameter(
            invoke.ReturnParameter,
            capturedMethod.ReturnParameter,
            "return");
        ParameterInfo[] actual = invoke.GetParameters();
        ParameterInfo[] expected = capturedMethod.GetParameters();
        if (actual.Length != expected.Length)
            throw Mismatch("parameter count");

        for (var index = 0; index < actual.Length; index++)
        {
            ValidateParameter(
                actual[index],
                expected[index],
                $"parameter {index}");
        }
    }

    private static void ValidateSourceInvoke(
        MethodInfo invoke,
        MethodInfo capturedMethod)
    {
        if (invoke.ReturnType != capturedMethod.ReturnType)
            throw Mismatch("return type");

        ValidateParameter(
            invoke.ReturnParameter,
            capturedMethod.ReturnParameter,
            "return");
        ParameterInfo[] actual = invoke.GetParameters();
        ParameterInfo[] expected = capturedMethod.GetParameters();
        if (actual.Length != expected.Length)
            throw Mismatch("parameter count");

        for (var index = 0; index < actual.Length; index++)
        {
            ValidateSourceParameter(
                actual[index],
                expected[index],
                $"parameter {index}");
        }
    }

    private static void ValidateParameter(
        ParameterInfo actual,
        ParameterInfo expected,
        string location)
    {
        if (actual.ParameterType != expected.ParameterType)
            throw Mismatch($"{location} type");
        if (actual.IsIn != expected.IsIn)
            throw Mismatch($"{location} IsIn metadata");
        if (actual.IsOut != expected.IsOut)
            throw Mismatch($"{location} IsOut metadata");
        if (!actual.GetRequiredCustomModifiers().SequenceEqual(
            expected.GetRequiredCustomModifiers()))
        {
            throw Mismatch(
                $"{location} required custom modifiers");
        }

        if (!actual.GetOptionalCustomModifiers().SequenceEqual(
            expected.GetOptionalCustomModifiers()))
        {
            throw Mismatch(
                $"{location} optional custom modifiers");
        }

        if (HasScopedRef(actual) != HasScopedRef(expected))
            throw Mismatch($"{location} scoped metadata");
    }

    private static void ValidateSourceParameter(
        ParameterInfo actual,
        ParameterInfo expected,
        string location)
    {
        if (actual.ParameterType != expected.ParameterType)
            throw Mismatch($"{location} type");
        if (actual.IsIn != expected.IsIn)
            throw Mismatch($"{location} IsIn metadata");
        if (actual.IsOut != expected.IsOut)
            throw Mismatch($"{location} IsOut metadata");
        if (!SourceRequiredModifiersMatch(actual, expected))
            throw Mismatch($"{location} required custom modifiers");
        if (!actual.GetOptionalCustomModifiers().SequenceEqual(
            expected.GetOptionalCustomModifiers()))
        {
            throw Mismatch(
                $"{location} optional custom modifiers");
        }

        if (HasScopedRef(actual) != HasScopedRef(expected))
            throw Mismatch($"{location} scoped metadata");
    }

    private static bool SourceRequiredModifiersMatch(
        ParameterInfo actual,
        ParameterInfo expected)
    {
        Type[] actualModifiers = actual.GetRequiredCustomModifiers();
        Type[] expectedModifiers = expected.GetRequiredCustomModifiers();
        if (actualModifiers.SequenceEqual(expectedModifiers))
            return true;

        return actual.IsIn
            && actualModifiers.SequenceEqual(
                expectedModifiers.Where(static modifier =>
                    modifier !=
                    typeof(System.Runtime.InteropServices.InAttribute)));
    }

    private static bool HasScopedRef(ParameterInfo parameter) =>
        parameter.GetCustomAttributesData().Any(static attribute =>
            attribute.AttributeType.FullName ==
            "System.Runtime.CompilerServices.ScopedRefAttribute");

    private static MockException Mismatch(string facet) =>
        new(
            $"The callback Invoke {facet} does not match the closed captured " +
            "signature.");

    private static void ValidateAsyncBoundary(
        Delegate callback,
        MethodInfo capturedMethod)
    {
        MethodInfo invoke = callback.GetType().GetMethod(
            nameof(Action.Invoke))!;
        bool isStateMachine =
            callback.Method.GetCustomAttribute<
                System.Runtime.CompilerServices.AsyncStateMachineAttribute>()
            is not null;
        if (invoke.ReturnType == typeof(void) && isStateMachine)
        {
            throw new MockException(
                "Async-void callbacks are not supported.");
        }

        if (!isStateMachine)
            return;

        foreach (ParameterInfo parameter in capturedMethod.GetParameters())
        {
            Type valueType = parameter.ParameterType.IsByRef
                ? parameter.ParameterType.GetElementType()!
                : parameter.ParameterType;
            if (MockTypeShape.MayBeByRefLike(valueType))
            {
                throw new MockException(
                    "An async callback cannot accept a borrowed argument. " +
                    "Use a synchronous callback that copies the value before " +
                    "returning asynchronous work.");
            }
        }
    }
}
