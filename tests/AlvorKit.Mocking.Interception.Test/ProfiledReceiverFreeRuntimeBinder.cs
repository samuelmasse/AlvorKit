namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Exercises the production receiver-free wrapper seam through its internal descriptor boundary.</summary>
internal static class ProfiledReceiverFreeRuntimeBinder
{
    private static readonly Type DescriptorType =
        typeof(Mock).Assembly.GetType(
            "AlvorKit.Mocking.MockInterceptionSiteDescriptor",
            throwOnError: true)!;
    private static readonly Type OperationKindType =
        typeof(Mock).Assembly.GetType(
            "AlvorKit.Mocking.MockInvocationOperationKind",
            throwOnError: true)!;
    private static readonly MethodInfo BindDefinition =
        typeof(MockInterceptionPreparationCoordinator).Assembly
            .GetType(
                "AlvorKit.Mocking.MockInterceptionRuntime",
                throwOnError: true)!
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(method =>
                method.Name == "Bind" &&
                method.IsGenericMethodDefinition);

    /// <summary>Binds one exact production receiver-free operation wrapper.</summary>
    internal static TDelegate Bind<TDelegate>(
        MethodInfo caller,
        MemberInfo operation,
        string operationKind,
        TDelegate original)
        where TDelegate : Delegate
    {
        object kind = Enum.Parse(OperationKindType, operationKind);
        object descriptor = Activator.CreateInstance(
            DescriptorType,
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            [
                caller.Module.ModuleVersionId,
                caller.MetadataToken,
                ProfiledReceiverFreeOperationOffset.Find(
                    caller,
                    operation),
                kind,
            ],
            culture: null)!;
        return (TDelegate)BindDefinition
            .MakeGenericMethod(typeof(TDelegate))
            .Invoke(null, [descriptor, operation, original])!;
    }
}
