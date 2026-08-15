using System.Collections.Immutable;

namespace AlvorKit;

/// <summary>
/// Defines collectible exact interception methods with original parameter metadata.
/// </summary>
internal static class MockTypedTrampolineEmitter
{
    private static int nextTypeId;

    /// <summary>
    /// Emits one reusable exact prefix for a validated runtime method.
    /// </summary>
    internal static MockTypedTrampolineArtifact Emit(
        ModuleBuilder module,
        MethodInfo target,
        MockDispatchCacheKey key)
    {
        ParameterInfo[] parameters = target.GetParameters();
        ImmutableArray<int> carrierIndices = CreateCarrierIndices(parameters);
        Type[] wrapperTypes = CreateWrapperTypes(target, parameters);
        TypeBuilder type = module.DefineType(
            $"TypedTrampoline_{Interlocked.Increment(ref nextTypeId)}",
            TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);
        MethodBuilder prefix = type.DefineMethod(
            "Prefix",
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
            CallingConventions.Standard);
        MethodBuilder finalizer = type.DefineMethod(
            "Finalizer",
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
            CallingConventions.Standard);

        SetPrefixSignature(prefix, target, parameters, wrapperTypes);
        DefinePrefixParameters(prefix, target, parameters, wrapperTypes.Length);
        MockTypedTrampolineIl.Emit(
            prefix.GetILGenerator(),
            target,
            parameters,
            carrierIndices,
            MockBackendLabel.For(
                key.Backend.Kind,
                key.Operation));
        MockTypedFinalizerEmitter.SetSignature(finalizer, target, parameters);
        MockTypedFinalizerEmitter.DefineParameters(finalizer, target, parameters);
        MockTypedFinalizerIl.Emit(finalizer.GetILGenerator(), target, parameters, carrierIndices);

        Type generatedType = type.CreateType()!;
        MethodInfo generatedPrefix = generatedType.GetMethod(
            "Prefix",
            BindingFlags.Public | BindingFlags.Static)!;
        MethodInfo generatedFinalizer = generatedType.GetMethod(
            "Finalizer",
            BindingFlags.Public | BindingFlags.Static)!;
        return new MockTypedTrampolineArtifact(
            key,
            generatedPrefix,
            generatedFinalizer,
            carrierIndices);
    }

    private static Type[] CreateWrapperTypes(
        MethodInfo target,
        ParameterInfo[] parameters)
    {
        bool isVoid = target.ReturnType == typeof(void);
        var types = new Type[parameters.Length + (isVoid ? 3 : 4)];
        types[0] = typeof(MethodInfo);
        types[1] = typeof(object);
        int parameterOffset = 2;

        if (!isVoid)
        {
            types[2] = MockManagedReferenceAbi.IsSupported(target.ReturnType)
                ? MockManagedReferenceAbi.InjectionType(target.ReturnType)
                : target.ReturnType.IsByRef
                    ? target.ReturnType
                    : target.ReturnType.MakeByRefType();
            parameterOffset = 3;
        }

        for (int index = 0; index < parameters.Length; index++)
            types[index + parameterOffset] = parameters[index].ParameterType;

        types[^1] = typeof(MockDispatchContinuation).MakeByRefType();
        return types;
    }

    private static void SetPrefixSignature(
        MethodBuilder prefix,
        MethodInfo target,
        ParameterInfo[] parameters,
        Type[] wrapperTypes)
    {
        var requiredModifiers = new Type[wrapperTypes.Length][];
        var optionalModifiers = new Type[wrapperTypes.Length][];
        for (int index = 0; index < wrapperTypes.Length; index++)
        {
            requiredModifiers[index] = [];
            optionalModifiers[index] = [];
        }

        int parameterOffset = target.ReturnType == typeof(void) ? 2 : 3;
        if (target.ReturnType != typeof(void)
            && !MockManagedReferenceAbi.IsSupported(target.ReturnType))
        {
            requiredModifiers[2] = target.ReturnParameter.GetRequiredCustomModifiers();
            optionalModifiers[2] = target.ReturnParameter.GetOptionalCustomModifiers();
        }

        for (int index = 0; index < parameters.Length; index++)
        {
            requiredModifiers[index + parameterOffset] = parameters[index].GetRequiredCustomModifiers();
            optionalModifiers[index + parameterOffset] = parameters[index].GetOptionalCustomModifiers();
        }

        prefix.SetSignature(
            typeof(bool),
            null,
            null,
            wrapperTypes,
            requiredModifiers,
            optionalModifiers);
    }

    private static void DefinePrefixParameters(
        MethodBuilder prefix,
        MethodInfo target,
        ParameterInfo[] parameters,
        int wrapperParameterCount)
    {
        prefix.DefineParameter(1, ParameterAttributes.None, "__originalMethod");
        prefix.DefineParameter(2, ParameterAttributes.None, "__instance");
        int parameterOffset = 2;

        if (target.ReturnType != typeof(void))
        {
            prefix.DefineParameter(
                3,
                MockManagedReferenceAbi.IsSupported(target.ReturnType)
                    ? ParameterAttributes.None
                    : target.ReturnParameter.Attributes,
                MockManagedReferenceAbi.IsSupported(target.ReturnType)
                    ? "__resultRef"
                    : "__result");
            parameterOffset = 3;
        }

        for (int index = 0; index < parameters.Length; index++)
        {
            ParameterInfo parameter = parameters[index];
            ParameterBuilder generatedParameter = prefix.DefineParameter(
                index + parameterOffset + 1,
                parameter.Attributes,
                parameter.Name);
            CopyScopedMetadata(parameter, generatedParameter);
        }

        prefix.DefineParameter(
            wrapperParameterCount,
            ParameterAttributes.Out,
            "__state");
    }

    private static void CopyScopedMetadata(
        ParameterInfo source,
        ParameterBuilder destination)
    {
        foreach (CustomAttributeData attribute in source.GetCustomAttributesData())
        {
            if (attribute.AttributeType.FullName !=
                    "System.Runtime.CompilerServices.ScopedRefAttribute"
                || attribute.ConstructorArguments.Count != 0
                || attribute.NamedArguments.Count != 0)
            {
                continue;
            }

            destination.SetCustomAttribute(
                new CustomAttributeBuilder(attribute.Constructor, []));
        }
    }

    private static ImmutableArray<int> CreateCarrierIndices(ParameterInfo[] parameters)
        => MockIlParameter.CreateCarrierIndices(
            MockIlParameter.Create(parameters));
}
