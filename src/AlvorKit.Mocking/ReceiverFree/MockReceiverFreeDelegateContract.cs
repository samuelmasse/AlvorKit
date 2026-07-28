namespace AlvorKit.Mocking;

/// <summary>Validates exact construction and field site delegates.</summary>
internal static class MockReceiverFreeDelegateContract
{
    /// <summary>Returns the exact delegate entry point after validation.</summary>
    internal static MethodInfo Validate(
        MockInterceptionSiteDescriptor site,
        MemberInfo operation,
        MethodInfo invoke)
    {
        if (invoke.ContainsGenericParameters)
        {
            throw Failure(
                site,
                "the generated delegate signature remains open");
        }

        return site.OperationKind switch
        {
            MockInvocationOperationKind.Construction =>
                ValidateConstruction(
                    site,
                    operation as ConstructorInfo,
                    invoke),
            MockInvocationOperationKind.ConstructorBody =>
                ValidateConstructorBody(
                    site,
                    operation as ConstructorInfo,
                    invoke),
            MockInvocationOperationKind.FieldRead =>
                ValidateField(
                    site,
                    operation as FieldInfo,
                    invoke,
                    write: false),
            MockInvocationOperationKind.FieldWrite =>
                ValidateField(
                    site,
                    operation as FieldInfo,
                    invoke,
                    write: true),
            MockInvocationOperationKind.StructMethod =>
                ValidateStructMethod(
                    site,
                    operation as MethodInfo,
                    invoke),
            _ => throw Failure(
                site,
                $"operation kind '{site.OperationKind}' has no " +
                "receiver-free delegate contract")
        };
    }

    private static MethodInfo ValidateStructMethod(
        MockInterceptionSiteDescriptor site,
        MethodInfo? method,
        MethodInfo invoke)
    {
        if (method is not
            {
                IsStatic: false,
                DeclaringType: { } declaringType
            } ||
            !(declaringType.IsValueType ||
              declaringType.IsInterface))
        {
            throw Failure(
                site,
                "struct-method metadata requires a value-type or constrained " +
                "interface instance method");
        }

        MockReceiverFreeParameterContract.ValidateParameter(
            site,
            invoke.ReturnParameter,
            method.ReturnParameter,
            "return");
        ParameterInfo[] actual = invoke.GetParameters();
        Type? receiverType = actual.Length == 0
            ? null
            : actual[0].ParameterType.GetElementType();
        bool receiverMatches =
            receiverType is { IsValueType: true } &&
            (declaringType.IsInterface
                ? declaringType.IsAssignableFrom(receiverType)
                : receiverType == declaringType);
        if (actual.Length == 0 ||
            !actual[0].ParameterType.IsByRef ||
            !receiverMatches)
        {
            throw Failure(
                site,
                $"struct-method receiver must be a managed reference to " +
                $"a value type implementing '{declaringType}'");
        }

        MockReceiverFreeParameterContract.Validate(
            site,
            actual,
            method.GetParameters(),
            1);
        return invoke;
    }

    private static MethodInfo ValidateConstructorBody(
        MockInterceptionSiteDescriptor site,
        ConstructorInfo? constructor,
        MethodInfo invoke)
    {
        if (constructor?.DeclaringType is not { IsValueType: false }
            declaringType ||
            constructor.IsStatic)
        {
            throw Failure(
                site,
                "constructor-body metadata requires an instance class " +
                "constructor");
        }
        if (invoke.ReturnType != typeof(void))
        {
            throw Failure(
                site,
                "constructor-body delegate must return void");
        }

        ParameterInfo[] actual = invoke.GetParameters();
        if (actual.Length == 0 ||
            actual[0].ParameterType != declaringType)
        {
            throw Failure(
                site,
                $"constructor-body receiver does not match " +
                $"'{declaringType}'");
        }
        MockReceiverFreeParameterContract.Validate(
            site,
            actual,
            constructor.GetParameters(),
            1);
        return invoke;
    }

    private static MethodInfo ValidateConstruction(
        MockInterceptionSiteDescriptor site,
        ConstructorInfo? constructor,
        MethodInfo invoke)
    {
        if (constructor?.DeclaringType is null || constructor.IsStatic)
            throw Failure(site, "construction metadata is not a constructor");
        if (invoke.ReturnType != constructor.DeclaringType)
        {
            throw Failure(
                site,
                $"construction delegate returns '{invoke.ReturnType}', not " +
                $"'{constructor.DeclaringType}'");
        }

        MockReceiverFreeParameterContract.Validate(
            site,
            invoke.GetParameters(),
            constructor.GetParameters(),
            0);
        return invoke;
    }

    private static MethodInfo ValidateField(
        MockInterceptionSiteDescriptor site,
        FieldInfo? field,
        MethodInfo invoke,
        bool write)
    {
        if (field?.DeclaringType is null)
            throw Failure(site, "field metadata is not a runtime field");
        if (field.IsLiteral)
            throw Failure(site, "literal fields have no interceptable opcode");

        ParameterInfo[] actual = invoke.GetParameters();
        int receiverCount = field.IsStatic ? 0 : 1;
        int expectedCount = receiverCount + (write ? 1 : 0);
        if (actual.Length != expectedCount)
        {
            throw Failure(
                site,
                $"field delegate has {actual.Length} parameters; expected " +
                $"{expectedCount}");
        }
        if (!field.IsStatic &&
            actual[0].ParameterType != field.DeclaringType)
        {
            throw Failure(
                site,
                $"field receiver '{actual[0].ParameterType}' does not match " +
                $"'{field.DeclaringType}'");
        }
        if (write)
        {
            if (invoke.ReturnType != typeof(void) ||
                actual[receiverCount].ParameterType != field.FieldType)
            {
                throw Failure(
                    site,
                    "field-write delegate does not preserve the exact value " +
                    "type and void return");
            }
        }
        else if (invoke.ReturnType != field.FieldType)
        {
            throw Failure(
                site,
                $"field-read delegate returns '{invoke.ReturnType}', not " +
                $"'{field.FieldType}'");
        }

        return invoke;
    }

    private static MockException Failure(
        MockInterceptionSiteDescriptor site,
        string detail) =>
        new($"Interception site '{site}' is invalid because {detail}.");
}
