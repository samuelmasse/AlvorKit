namespace AlvorKit.Mocking;

/// <summary>
/// Binds intercepted operation delegates to the shared exact mocking data
/// plane.
/// </summary>
internal static class MockInterceptionOperationRuntime
{
    /// <summary>Gets the exact wrapper ABI expected by interception caches.</summary>
    internal const int AbiVersion = 1;

    /// <summary>
    /// Eagerly records one instance method as owned by interception dispatch.
    /// </summary>
    internal static void Register(MethodInfo operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        if (operation.IsStatic || operation.DeclaringType is null)
        {
            throw new MockException(
                "Only a non-static method can be eagerly registered for " +
                "interception instance dispatch.");
        }

        MockInterceptionMethodRegistry.Add(operation);
    }

    /// <summary>
    /// Returns an exact wrapper for an instance or receiver-free operation.
    /// </summary>
    internal static TDelegate Bind<TDelegate>(
        MockInterceptionSiteDescriptor site,
        MemberInfo operation,
        TDelegate original)
        where TDelegate : Delegate
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(original);

        ValidateOperation(site, operation);
        return MockRuntimeBackendRegistry.Operation.BindInterception(
            site,
            operation,
            original);
    }

    private static void ValidateOperation(
        MockInterceptionSiteDescriptor site,
        MemberInfo operation)
    {
        bool valid = site.OperationKind switch
        {
            MockInvocationOperationKind.InstanceMethod =>
                operation is MethodInfo { IsStatic: false },
            MockInvocationOperationKind.StaticMethod =>
                operation is MethodInfo { IsStatic: true },
            MockInvocationOperationKind.Construction =>
                operation is ConstructorInfo,
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
            throw Failure(
                site,
                $"operation metadata '{operation.MemberType}' does not match " +
                $"descriptor kind '{site.OperationKind}'");
        }
    }

    private static MockException Failure(
        MockInterceptionSiteDescriptor site,
        string detail) =>
        new($"Interception site '{site}' is invalid because {detail}.");
}
