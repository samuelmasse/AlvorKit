namespace AlvorKit;

/// <summary>
/// Opaque metadata for one intercepted operation site in an owned assembly.
/// </summary>
public sealed class MockCallSite
{
    /// <summary>Creates one validated public site handle.</summary>
    internal MockCallSite(
        MockInterceptionSiteDescriptor descriptor,
        MemberInfo operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ValidateOperation(descriptor.OperationKind, operation);

        Descriptor = descriptor;
        Operation = operation;
    }

    /// <summary>Gets the stable internal interception-site descriptor.</summary>
    internal MockInterceptionSiteDescriptor Descriptor { get; }

    /// <summary>Gets the exact intercepted member.</summary>
    internal MemberInfo Operation { get; }

    /// <summary>Validates that this site can scope one captured setup.</summary>
    internal void Validate(
        MemberInfo operation,
        MockInvocationOperationKind operationKind)
    {
        ArgumentNullException.ThrowIfNull(operation);
        if (operationKind != Descriptor.OperationKind ||
            !Equals(operation, Operation))
        {
            throw new MockException(
                $"Call site '{Descriptor}' identifies '{Describe(Operation)}', " +
                $"not '{Describe(operation)}' with operation kind " +
                $"'{operationKind}'.");
        }
    }

    /// <inheritdoc />
    public override string ToString() => Descriptor.ToString();

    private static void ValidateOperation(
        MockInvocationOperationKind operationKind,
        MemberInfo operation)
    {
        bool valid = operationKind switch
        {
            MockInvocationOperationKind.StaticMethod =>
                operation is MethodInfo { IsStatic: true },
            MockInvocationOperationKind.Construction or
            MockInvocationOperationKind.ConstructorBody =>
                operation is ConstructorInfo,
            MockInvocationOperationKind.FieldRead or
            MockInvocationOperationKind.FieldWrite =>
                operation is FieldInfo,
            MockInvocationOperationKind.StructMethod =>
                operation is MethodInfo
                {
                    IsStatic: false,
                    DeclaringType: { } declaringType
                } &&
                (declaringType.IsValueType ||
                 declaringType.IsInterface),
            _ => false
        };
        if (!valid)
        {
            throw new MockException(
                $"Member '{Describe(operation)}' cannot identify an " +
                $"interception " +
                $"'{operationKind}' call site.");
        }
    }

    private static string Describe(MemberInfo operation) =>
        $"{operation.DeclaringType?.FullName ?? "<unknown type>"}." +
        operation.Name;
}
