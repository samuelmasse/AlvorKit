namespace AlvorKit.Mocking;

/// <summary>
/// Owns exact interception prefix artifacts beneath a weak source-module boundary.
/// </summary>
internal sealed class MockTypedTrampolineCache
{
    private static readonly ConditionalWeakTable<Module, MockTypedTrampolineCache> caches = [];
    private static readonly ConstructorInfo AccessConstructor =
        typeof(System.Runtime.CompilerServices.IgnoresAccessChecksToAttribute)
            .GetConstructor([typeof(string)])!;
    private readonly Lock cacheLock = new();
    private readonly Dictionary<MockDispatchCacheKey, MockTypedTrampolineArtifact> artifacts = [];
    private readonly HashSet<string> accessibleAssemblies = [];
    private readonly AssemblyBuilder generatedAssembly;
    private readonly ModuleBuilder generatedModule;

    /// <summary>Creates a cache whose generated assembly can access its exact source signatures.</summary>
    private MockTypedTrampolineCache(Assembly sourceAssembly)
    {
        string name = $"AlvorKit.Mocking.TypedTrampolines.{Guid.NewGuid():N}";
        generatedAssembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName(name),
            AssemblyBuilderAccess.RunAndCollect);
        GrantAccess(typeof(MockTypedTrampolineCache).Assembly);
        GrantAccess(typeof(Mock).Assembly);
        GrantAccess(sourceAssembly);
        generatedModule = generatedAssembly.DefineDynamicModule(name);
    }

    /// <summary>
    /// Validates and returns a reusable exact artifact for one backend and
    /// runtime construction.
    /// </summary>
    internal static MockTypedTrampolineArtifact GetOrCreate(
        MethodInfo method,
        MockBackendIdentity backend,
        MockOperationKind? requestedOperation = null)
    {
        MockOperationKind operation =
            requestedOperation ??
            (method.IsStatic
                ? MockOperationKind.StaticMethod
                : MockOperationKind.InstanceMethod);
        MockSignatureValidation validation = MockSignatureValidator.Validate(
            method,
            backend,
            operation);
        if (!validation.IsSupported)
            throw new MockException(validation.Rejection!.Message);

        MockDispatchCacheKey key = MockDispatchCacheKey.Create(
            method.DeclaringType!,
            method,
            backend,
            operation,
            validation.Signature);
        MockTypedTrampolineCache cache = caches.GetValue(
            MockCollectibleReferenceOwner.Select(method),
            static module => new MockTypedTrampolineCache(
                module.Assembly));
        return cache.GetOrCreate(method, key);
    }

    private MockTypedTrampolineArtifact GetOrCreate(
        MethodInfo method,
        MockDispatchCacheKey key)
    {
        lock (cacheLock)
        {
            if (artifacts.TryGetValue(key, out MockTypedTrampolineArtifact? artifact))
                return artifact;

            GrantSignatureAccess(method);
            artifact = MockTypedTrampolineEmitter.Emit(
                generatedModule,
                method,
                key);
            artifacts.Add(key, artifact);
            return artifact;
        }
    }

    /// <summary>Grants emitted code access to every assembly named by an exact signature.</summary>
    private void GrantSignatureAccess(MethodInfo method)
    {
        GrantTypeAccess(method.DeclaringType!);
        GrantTypeAccess(method.ReturnType);
        GrantModifierAccess(method.ReturnParameter);
        foreach (ParameterInfo parameter in method.GetParameters())
        {
            GrantTypeAccess(parameter.ParameterType);
            GrantModifierAccess(parameter);
        }
    }

    /// <summary>Grants access to required and optional custom-modifier types.</summary>
    private void GrantModifierAccess(ParameterInfo parameter)
    {
        foreach (Type modifier in parameter.GetRequiredCustomModifiers())
            GrantTypeAccess(modifier);
        foreach (Type modifier in parameter.GetOptionalCustomModifiers())
            GrantTypeAccess(modifier);
    }

    /// <summary>Grants access recursively through element and constructed generic types.</summary>
    private void GrantTypeAccess(Type type)
    {
        GrantAccess(type.Assembly);
        if (type.HasElementType)
            GrantTypeAccess(type.GetElementType()!);
        foreach (Type argument in type.GetGenericArguments())
            GrantTypeAccess(argument);
    }

    /// <summary>Adds one idempotent dynamic-assembly visibility grant.</summary>
    private void GrantAccess(Assembly assembly)
    {
        string? assemblyName = assembly.GetName().Name;
        if (assemblyName is null
            || !accessibleAssemblies.Add(assemblyName))
        {
            return;
        }

        generatedAssembly.SetCustomAttribute(
            new CustomAttributeBuilder(
                AccessConstructor,
                [assemblyName]));
    }
}
