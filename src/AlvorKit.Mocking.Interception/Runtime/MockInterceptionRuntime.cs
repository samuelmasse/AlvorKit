namespace AlvorKit.Mocking;

/// <summary>
/// Binds one prepared operation to the exact Mocking control-plane wrapper.
/// </summary>
internal static class MockInterceptionRuntime
{
    /// <summary>
    /// Binds one explicitly selected owned caller to an ordinary instance operation.
    /// </summary>
    internal static TDelegate BindOwnedInstanceCaller<TDelegate>(
        MethodInfo caller,
        int originalIlOffset,
        MethodInfo operation,
        TDelegate original)
        where TDelegate : Delegate
    {
        ArgumentNullException.ThrowIfNull(caller);
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(original);
        return Bind(
            new MockInterceptionSiteDescriptor(
                caller.Module.ModuleVersionId,
                caller.MetadataToken,
                originalIlOffset,
                MockInvocationOperationKind.InstanceMethod),
            operation,
            original);
    }

    /// <summary>
    /// Binds one definition-wide constructor route to its extracted post-initializer remainder.
    /// </summary>
    internal static TDelegate BindConstructorRemainder<TDelegate>(
        ConstructorInfo constructor,
        int originalIlOffset,
        TDelegate original)
        where TDelegate : Delegate
    {
        ArgumentNullException.ThrowIfNull(constructor);
        ArgumentNullException.ThrowIfNull(original);
        return Bind(
            new MockInterceptionSiteDescriptor(
                constructor.Module.ModuleVersionId,
                constructor.MetadataToken,
                originalIlOffset,
                MockInvocationOperationKind.ConstructorBody),
            constructor,
            original);
    }

    /// <summary>Binds one explicitly selected caller to an exact construction operation.</summary>
    internal static TDelegate BindConstructionCaller<TDelegate>(
        MethodInfo caller,
        int originalIlOffset,
        ConstructorInfo constructor,
        TDelegate original)
        where TDelegate : Delegate
    {
        ArgumentNullException.ThrowIfNull(caller);
        ArgumentNullException.ThrowIfNull(constructor);
        ArgumentNullException.ThrowIfNull(original);
        return Bind(
            new MockInterceptionSiteDescriptor(
                caller.Module.ModuleVersionId,
                caller.MetadataToken,
                originalIlOffset,
                MockInvocationOperationKind.Construction),
            constructor,
            original);
    }

    /// <summary>Creates an exact operation wrapper around its preserved original delegate.</summary>
    internal static TDelegate Bind<TDelegate>(
        MockInterceptionSiteDescriptor site,
        MemberInfo operation,
        TDelegate original)
        where TDelegate : Delegate
    {
        MethodInfo logicalMethod;
        if (operation is MethodInfo method &&
            site.OperationKind !=
                MockInvocationOperationKind.StructMethod)
        {
            logicalMethod = method;
        }
        else
        {
            logicalMethod = MockReceiverFreeMethodCache.GetOrCreate(
                site,
                operation,
                typeof(TDelegate));
        }

        if (logicalMethod.ReturnType.IsByRef &&
            !MockManagedReferenceAbi.IsSupported(
                logicalMethod.ReturnType))
        {
            throw new MockException(
                $"Interception site '{site}' is invalid because " +
                "managed-reference " +
                "returns to ref-struct, pointer, function-pointer, or open " +
                "element types are unsupported.");
        }

        MethodInfo invoke = MockInterceptionDelegateContract.Validate(
            typeof(TDelegate).GetMethod(nameof(Action.Invoke))!,
            logicalMethod,
            site);
        if (site.OperationKind ==
            MockInvocationOperationKind.InstanceMethod)
        {
            MockInterceptionOperationRuntime.Register(logicalMethod);
        }

        MockInterceptionWrapperArtifact artifact =
            MockInterceptionWrapperCache.GetOrCreate(
                site,
                logicalMethod,
                typeof(TDelegate),
                invoke);
        var state = new MockInterceptionBindingState(
            site,
            operation,
            logicalMethod,
            original);
        try
        {
            return (TDelegate)artifact.Wrapper.CreateDelegate(
                typeof(TDelegate),
                state);
        }
        catch (ArgumentException exception)
        {
            throw new MockException(
                $"Interception site '{site}' could not bind its exact runtime " +
                $"wrapper: {exception.Message}");
        }
    }
}
