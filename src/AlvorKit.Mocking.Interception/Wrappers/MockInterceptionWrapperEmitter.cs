namespace AlvorKit;

/// <summary>Defines one collectible exact wrapper around a preserved original delegate.</summary>
internal static class MockInterceptionWrapperEmitter
{
    private static int nextTypeId;

    /// <summary>Emits one static method whose state argument is closed at bind time.</summary>
    internal static MockInterceptionWrapperArtifact Emit(
        ModuleBuilder module,
        MethodInfo operation,
        Type delegateType,
        MethodInfo invoke,
        MockTypedTrampolineArtifact trampoline,
        MockOperationKind operationKind,
        MockInterceptionWrapperCacheKey key)
    {
        TypeBuilder type = module.DefineType(
            $"InterceptionWrapper_{Interlocked.Increment(ref nextTypeId)}",
            TypeAttributes.Public |
            TypeAttributes.Abstract |
            TypeAttributes.Sealed);
        MethodBuilder wrapper = type.DefineMethod(
            "Invoke",
            MethodAttributes.Public |
            MethodAttributes.Static |
            MethodAttributes.HideBySig,
            CallingConventions.Standard);
        SetSignature(wrapper, invoke);
        DefineParameters(wrapper, invoke);
        MockInterceptionWrapperIl.Emit(
            wrapper.GetILGenerator(),
            operation,
            delegateType,
            invoke,
            trampoline,
            operationKind);

        Type generated = type.CreateType()!;
        return new(
            key,
            generated.GetMethod(
                "Invoke",
                BindingFlags.Public | BindingFlags.Static)!);
    }

    private static void SetSignature(
        MethodBuilder wrapper,
        MethodInfo invoke)
    {
        ParameterInfo[] parameters = invoke.GetParameters();
        var types = new Type[parameters.Length + 1];
        var required = new Type[types.Length][];
        var optional = new Type[types.Length][];
        types[0] = typeof(MockInterceptionBindingState);
        required[0] = [];
        optional[0] = [];

        for (int index = 0; index < parameters.Length; index++)
        {
            ParameterInfo parameter = parameters[index];
            types[index + 1] = parameter.ParameterType;
            required[index + 1] =
                parameter.GetRequiredCustomModifiers();
            optional[index + 1] =
                parameter.GetOptionalCustomModifiers();
        }

        wrapper.SetSignature(
            invoke.ReturnType,
            invoke.ReturnParameter.GetRequiredCustomModifiers(),
            invoke.ReturnParameter.GetOptionalCustomModifiers(),
            types,
            required,
            optional);
    }

    private static void DefineParameters(
        MethodBuilder wrapper,
        MethodInfo invoke)
    {
        wrapper.DefineParameter(
            0,
            invoke.ReturnParameter.Attributes,
            invoke.ReturnParameter.Name);
        wrapper.DefineParameter(
            1,
            ParameterAttributes.None,
            "__site");
        ParameterInfo[] parameters = invoke.GetParameters();
        for (int index = 0; index < parameters.Length; index++)
        {
            ParameterInfo parameter = parameters[index];
            ParameterBuilder generated = wrapper.DefineParameter(
                index + 2,
                parameter.Attributes,
                parameter.Name);
            CopyScopedMetadata(parameter, generated);
        }
    }

    private static void CopyScopedMetadata(
        ParameterInfo source,
        ParameterBuilder destination)
    {
        foreach (CustomAttributeData attribute in
            source.GetCustomAttributesData())
        {
            if (attribute.AttributeType.FullName !=
                    "System.Runtime.CompilerServices.ScopedRefAttribute" ||
                attribute.ConstructorArguments.Count != 0 ||
                attribute.NamedArguments.Count != 0)
            {
                continue;
            }

            destination.SetCustomAttribute(
                new CustomAttributeBuilder(attribute.Constructor, []));
        }
    }
}
