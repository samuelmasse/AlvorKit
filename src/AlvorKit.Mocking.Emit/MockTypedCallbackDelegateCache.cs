namespace AlvorKit.Mocking;

/// <summary>Owns shared stable exact callback delegate types beneath weak source-module boundaries.</summary>
internal sealed class MockTypedCallbackDelegateCache
{
    private static readonly ConditionalWeakTable<Module, MockTypedCallbackDelegateCache>
        Caches = [];
    private static readonly ConstructorInfo AccessConstructor =
        typeof(System.Runtime.CompilerServices.IgnoresAccessChecksToAttribute)
            .GetConstructor([typeof(string)])!;
    private readonly Lock cacheLock = new();
    private readonly Dictionary<MethodInfo, Type> types = [];
    private readonly HashSet<string> accessibleAssemblies = [];
    private readonly AssemblyBuilder generatedAssembly;
    private readonly ModuleBuilder generatedModule;
    private int nextId;

    private MockTypedCallbackDelegateCache(Assembly sourceAssembly)
    {
        string name =
            $"AlvorKit.Mocking.TypedCallbacks.{Guid.NewGuid():N}";
        generatedAssembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName(name),
            AssemblyBuilderAccess.RunAndCollect);
        GrantAccess(typeof(MockTypedCallbackDelegateCache).Assembly);
        GrantAccess(typeof(Mock).Assembly);
        GrantAccess(sourceAssembly);
        generatedModule = generatedAssembly.DefineDynamicModule(name);
    }

    /// <summary>Returns one exact delegate type for a closed captured method.</summary>
    internal static Type GetOrCreate(MethodInfo method)
    {
        if (method.ContainsGenericParameters)
        {
            throw new MockException(
                "The captured callback signature must be closed.");
        }

        if (method.IsGenericMethod
            && MockTypedCallbackDelegateShape.Create(method)
            is Type standardType)
        {
            return standardType;
        }

        method = MockProxyMethodSource.Resolve(method);
        MockTypedCallbackDelegateCache cache = Caches.GetValue(
            MockCollectibleReferenceOwner.Select(method),
            static module => new(module.Assembly));
        return cache.GetOrCreateCore(method);
    }

    private Type GetOrCreateCore(MethodInfo method)
    {
        lock (cacheLock)
        {
            if (types.TryGetValue(method, out Type? type))
                return type;

            GrantSignatureAccess(method);
            type = Emit(method);
            types.Add(method, type);
            return type;
        }
    }

    private Type Emit(MethodInfo method)
    {
        TypeBuilder type = generatedModule.DefineType(
            $"StableCallback_{Interlocked.Increment(ref nextId)}",
            TypeAttributes.Public |
            TypeAttributes.Class |
            TypeAttributes.Sealed,
            typeof(MulticastDelegate));
        ConstructorBuilder constructor = type.DefineConstructor(
            MethodAttributes.Public |
            MethodAttributes.HideBySig |
            MethodAttributes.RTSpecialName,
            CallingConventions.Standard,
            [typeof(object), typeof(nint)]);
        constructor.SetImplementationFlags(
            MethodImplAttributes.Runtime |
            MethodImplAttributes.Managed);

        ParameterInfo[] parameters = method.GetParameters();
        MethodBuilder invoke = type.DefineMethod(
            nameof(Action.Invoke),
            MethodAttributes.Public |
            MethodAttributes.HideBySig |
            MethodAttributes.NewSlot |
            MethodAttributes.Virtual,
            CallingConventions.Standard);
        invoke.SetSignature(
            method.ReturnType,
            method.ReturnParameter.GetRequiredCustomModifiers(),
            method.ReturnParameter.GetOptionalCustomModifiers(),
            [.. parameters.Select(static parameter => parameter.ParameterType)],
            [.. parameters.Select(static parameter =>
                parameter.GetRequiredCustomModifiers())],
            [.. parameters.Select(static parameter =>
                parameter.GetOptionalCustomModifiers())]);
        DefineParameter(invoke, 0, method.ReturnParameter);
        for (var index = 0; index < parameters.Length; index++)
            DefineParameter(invoke, index + 1, parameters[index]);
        invoke.SetImplementationFlags(
            MethodImplAttributes.Runtime |
            MethodImplAttributes.Managed);
        return type.CreateType()!;
    }

    private static void DefineParameter(
        MethodBuilder invoke,
        int position,
        ParameterInfo source)
    {
        ParameterBuilder parameter = invoke.DefineParameter(
            position,
            source.Attributes,
            source.Name);
        foreach (CustomAttributeData attribute in source.GetCustomAttributesData())
        {
            if (attribute.AttributeType.FullName !=
                "System.Runtime.CompilerServices.ScopedRefAttribute")
            {
                continue;
            }

            parameter.SetCustomAttribute(
                new CustomAttributeBuilder(attribute.Constructor, []));
        }
    }

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
}
