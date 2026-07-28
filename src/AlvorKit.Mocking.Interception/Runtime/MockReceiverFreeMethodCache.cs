namespace AlvorKit.Mocking;

/// <summary>
/// Creates stable method-shaped metadata for construction and field
/// operations consumed by the interception exact-dispatch data plane.
/// </summary>
internal sealed class MockReceiverFreeMethodCache
{
    private static readonly ConditionalWeakTable<
        Module,
        MockReceiverFreeMethodCache> Caches = [];
    private static readonly ConstructorInfo AccessConstructor =
        typeof(System.Runtime.CompilerServices.IgnoresAccessChecksToAttribute)
            .GetConstructor([typeof(string)])!;
    private readonly Lock gate = new();
    private readonly Dictionary<MockReceiverFreeMethodKey, MethodInfo>
        methods = [];
    private readonly HashSet<string> accessibleAssemblies = [];
    private readonly AssemblyBuilder assembly;
    private readonly ModuleBuilder module;
    private int nextMethodId;

    private MockReceiverFreeMethodCache(Assembly sourceAssembly)
    {
        string name =
            $"AlvorKit.Mocking.ReceiverFreeMethods.{Guid.NewGuid():N}";
        assembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName(name),
            AssemblyBuilderAccess.RunAndCollect);
        GrantAccess(typeof(MockReceiverFreeMethodCache).Assembly);
        GrantAccess(typeof(Mock).Assembly);
        GrantAccess(sourceAssembly);
        module = assembly.DefineDynamicModule(name);
    }

    /// <summary>
    /// Returns stable static method metadata with the delegate's exact
    /// signature after validating it against the intercepted member.
    /// </summary>
    internal static MethodInfo GetOrCreate(
        MockInterceptionSiteDescriptor site,
        MemberInfo operation,
        Type delegateType)
    {
        MethodInfo invoke =
            MockReceiverFreeDelegateContract.Validate(
                site,
                operation,
                delegateType.GetMethod(nameof(Action.Invoke))!);
        MockReceiverFreeMethodCache cache = Caches.GetValue(
            MockCollectibleReferenceOwner.Select(
                operation,
                delegateType),
            static source => new(source.Assembly));
        var key = new MockReceiverFreeMethodKey(
            operation,
            site.OperationKind,
            MockCanonicalSignature.Create(invoke));
        return cache.GetOrCreate(key, invoke);
    }

    private MethodInfo GetOrCreate(
        MockReceiverFreeMethodKey key,
        MethodInfo invoke)
    {
        lock (gate)
        {
            if (methods.TryGetValue(key, out MethodInfo? method))
                return method;

            GrantSignatureAccess(invoke);
            TypeBuilder type = module.DefineType(
                $"ReceiverFree_{++nextMethodId}",
                TypeAttributes.Public |
                TypeAttributes.Abstract |
                TypeAttributes.Sealed);
            MethodBuilder builder = type.DefineMethod(
                "Invoke",
                MethodAttributes.Public |
                MethodAttributes.Static |
                MethodAttributes.HideBySig,
                CallingConventions.Standard);
            SetSignature(builder, invoke);
            DefineParameters(builder, invoke);
            ILGenerator il = builder.GetILGenerator();
            il.Emit(
                OpCodes.Newobj,
                typeof(NotSupportedException).GetConstructor(
                    Type.EmptyTypes)!);
            il.Emit(OpCodes.Throw);
            Type generated = type.CreateType()!;
            method = generated.GetMethod(
                "Invoke",
                BindingFlags.Public | BindingFlags.Static)!;
            methods.Add(key, method);
            return method;
        }
    }

    private static void SetSignature(
        MethodBuilder method,
        MethodInfo invoke)
    {
        ParameterInfo[] parameters = invoke.GetParameters();
        var types = new Type[parameters.Length];
        var required = new Type[parameters.Length][];
        var optional = new Type[parameters.Length][];
        for (int index = 0; index < parameters.Length; index++)
        {
            types[index] = parameters[index].ParameterType;
            required[index] =
                parameters[index].GetRequiredCustomModifiers();
            optional[index] =
                parameters[index].GetOptionalCustomModifiers();
        }

        method.SetSignature(
            invoke.ReturnType,
            invoke.ReturnParameter.GetRequiredCustomModifiers(),
            invoke.ReturnParameter.GetOptionalCustomModifiers(),
            types,
            required,
            optional);
    }

    private static void DefineParameters(
        MethodBuilder method,
        MethodInfo invoke)
    {
        method.DefineParameter(
            0,
            invoke.ReturnParameter.Attributes,
            invoke.ReturnParameter.Name);
        ParameterInfo[] parameters = invoke.GetParameters();
        for (int index = 0; index < parameters.Length; index++)
        {
            ParameterInfo parameter = parameters[index];
            ParameterBuilder generated = method.DefineParameter(
                index + 1,
                parameter.Attributes,
                parameter.Name);
            foreach (CustomAttributeData attribute in
                parameter.GetCustomAttributesData())
            {
                if (attribute.AttributeType.FullName ==
                        "System.Runtime.CompilerServices.ScopedRefAttribute" &&
                    attribute.ConstructorArguments.Count == 0 &&
                    attribute.NamedArguments.Count == 0)
                {
                    generated.SetCustomAttribute(
                        new CustomAttributeBuilder(
                            attribute.Constructor,
                            []));
                }
            }
        }
    }

    private void GrantSignatureAccess(MethodInfo invoke)
    {
        GrantTypeAccess(invoke.ReturnType);
        foreach (Type modifier in
            invoke.ReturnParameter.GetRequiredCustomModifiers())
        {
            GrantTypeAccess(modifier);
        }
        foreach (Type modifier in
            invoke.ReturnParameter.GetOptionalCustomModifiers())
        {
            GrantTypeAccess(modifier);
        }
        foreach (ParameterInfo parameter in invoke.GetParameters())
        {
            GrantTypeAccess(parameter.ParameterType);
            foreach (Type modifier in
                parameter.GetRequiredCustomModifiers())
            {
                GrantTypeAccess(modifier);
            }
            foreach (Type modifier in
                parameter.GetOptionalCustomModifiers())
            {
                GrantTypeAccess(modifier);
            }
        }
    }

    private void GrantTypeAccess(Type type)
    {
        GrantAccess(type.Assembly);
        if (type.HasElementType)
            GrantTypeAccess(type.GetElementType()!);
        foreach (Type argument in type.GetGenericArguments())
            GrantTypeAccess(argument);
    }

    private void GrantAccess(Assembly source)
    {
        string? name = source.GetName().Name;
        if (name is null || !accessibleAssemblies.Add(name))
            return;

        assembly.SetCustomAttribute(
            new CustomAttributeBuilder(
                AccessConstructor,
                [name]));
    }
}

/// <summary>Keys one stable receiver-free logical method.</summary>
internal sealed record MockReceiverFreeMethodKey(
    MemberInfo Operation,
    MockInvocationOperationKind OperationKind,
    MockCanonicalSignature Signature);
