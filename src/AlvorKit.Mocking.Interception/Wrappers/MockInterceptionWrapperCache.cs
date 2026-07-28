namespace AlvorKit.Mocking;

/// <summary>
/// Owns reusable exact wrapper code beneath a weak source-module
/// boundary.
/// </summary>
internal sealed class MockInterceptionWrapperCache
{
    private static readonly ConditionalWeakTable<Module, MockInterceptionWrapperCache>
        Caches = [];
    private static readonly MockBackendIdentity Backend =
        new(MockBackendKind.Interception, 1);
    private static readonly ConstructorInfo AccessConstructor =
        typeof(System.Runtime.CompilerServices.IgnoresAccessChecksToAttribute)
            .GetConstructor([typeof(string)])!;
    private readonly Lock cacheLock = new();
    private readonly Dictionary<
        MockInterceptionWrapperCacheKey,
        MockInterceptionWrapperArtifact> artifacts = [];
    private readonly HashSet<string> accessibleAssemblies = [];
    private readonly AssemblyBuilder generatedAssembly;
    private readonly ModuleBuilder generatedModule;

    private MockInterceptionWrapperCache(Assembly sourceAssembly)
    {
        string name =
            $"AlvorKit.Mocking.InterceptionWrappers.{Guid.NewGuid():N}";
        generatedAssembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName(name),
            AssemblyBuilderAccess.RunAndCollect);
        GrantAccess(typeof(MockInterceptionWrapperCache).Assembly);
        GrantAccess(typeof(MockInterceptionOperationRuntime).Assembly);
        GrantAccess(sourceAssembly);
        generatedModule = generatedAssembly.DefineDynamicModule(name);
    }

    /// <summary>Gets or emits one exact wrapper for a constructed operation.</summary>
    internal static MockInterceptionWrapperArtifact GetOrCreate(
        MockInterceptionSiteDescriptor site,
        MethodInfo operation,
        Type delegateType,
        MethodInfo invoke)
    {
        MockOperationKind operationKind = site.OperationKind switch
        {
            MockInvocationOperationKind.InstanceMethod =>
                MockOperationKind.InstanceMethod,
            MockInvocationOperationKind.StaticMethod =>
                MockOperationKind.StaticMethod,
            MockInvocationOperationKind.Construction =>
                MockOperationKind.Construction,
            MockInvocationOperationKind.ConstructorBody =>
                MockOperationKind.ConstructorBody,
            MockInvocationOperationKind.FieldRead =>
                MockOperationKind.FieldRead,
            MockInvocationOperationKind.FieldWrite =>
                MockOperationKind.FieldWrite,
            MockInvocationOperationKind.StructMethod =>
                MockOperationKind.StructMethod,
            _ => throw new UnreachableException()
        };
        MockSignatureValidation validation = MockSignatureValidator.Validate(
            operation,
            Backend,
            operationKind);
        if (!validation.IsSupported)
            throw new MockException(validation.Rejection!.Message);

        MockDispatchCacheKey dispatch = MockDispatchCacheKey.Create(
            operation.DeclaringType!,
            operation,
            Backend,
            operationKind,
            validation.Signature);
        var key = new MockInterceptionWrapperCacheKey(dispatch, delegateType);
        MockInterceptionWrapperCache cache = Caches.GetValue(
            MockCollectibleReferenceOwner.Select(
                operation,
                delegateType),
            static module => new(module.Assembly));
        return cache.GetOrCreate(
            operation,
            delegateType,
            invoke,
            key);
    }

    private MockInterceptionWrapperArtifact GetOrCreate(
        MethodInfo operation,
        Type delegateType,
        MethodInfo invoke,
        MockInterceptionWrapperCacheKey key)
    {
        lock (cacheLock)
        {
            if (artifacts.TryGetValue(
                key,
                out MockInterceptionWrapperArtifact? artifact))
            {
                return artifact;
            }

            GrantTypeAccess(delegateType);
            GrantTypeAccess(operation.DeclaringType!);
            GrantTypeAccess(operation.ReturnType);
            GrantModifierAccess(operation.ReturnParameter);
            foreach (ParameterInfo parameter in operation.GetParameters())
            {
                GrantTypeAccess(parameter.ParameterType);
                GrantModifierAccess(parameter);
            }

            GrantTypeAccess(invoke.ReturnType);
            GrantModifierAccess(invoke.ReturnParameter);
            foreach (ParameterInfo parameter in invoke.GetParameters())
            {
                GrantTypeAccess(parameter.ParameterType);
                GrantModifierAccess(parameter);
            }

            MockTypedTrampolineArtifact trampoline =
                MockTypedTrampolineCache.GetOrCreate(
                    operation,
                    Backend,
                    key.Dispatch.Operation);
            artifact = MockInterceptionWrapperEmitter.Emit(
                generatedModule,
                operation,
                delegateType,
                invoke,
                trampoline,
                key.Dispatch.Operation,
                key);
            artifacts.Add(key, artifact);
            return artifact;
        }
    }

    private void GrantAccess(Assembly assembly)
    {
        string? name = assembly.GetName().Name;
        if (name is null || !accessibleAssemblies.Add(name))
            return;

        generatedAssembly.SetCustomAttribute(
            new CustomAttributeBuilder(
                AccessConstructor,
                [name]));
    }

    private void GrantModifierAccess(ParameterInfo parameter)
    {
        foreach (Type modifier in parameter.GetRequiredCustomModifiers())
            GrantTypeAccess(modifier);
        foreach (Type modifier in parameter.GetOptionalCustomModifiers())
            GrantTypeAccess(modifier);
    }

    private void GrantTypeAccess(Type type)
    {
        GrantAccess(type.Assembly);
        if (type.HasElementType)
            GrantTypeAccess(type.GetElementType()!);
        foreach (Type argument in type.GetGenericArguments())
            GrantTypeAccess(argument);
    }
}
