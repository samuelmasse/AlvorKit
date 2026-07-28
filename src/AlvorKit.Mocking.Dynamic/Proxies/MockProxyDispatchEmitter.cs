using System.Collections.Immutable;

namespace AlvorKit.Mocking;

/// <summary>
/// Defines the exact backend-neutral dispatch methods used by generated proxy
/// bodies.
/// </summary>
internal static class MockProxyDispatchEmitter
{
    private static readonly IReadOnlyDictionary<Type, Type>
        EmptySubstitutions = new Dictionary<Type, Type>();

    /// <summary>Defines a closed non-generic proxy prefix.</summary>
    internal static MethodBuilder DefinePrefix(
        TypeBuilder cache,
        MethodInfo source,
        Type returnType,
        MockIlParameter[] parameters,
        Type? callbackType) =>
        DefinePrefix(
            cache,
            source,
            returnType,
            parameters,
            EmptySubstitutions,
            callbackType);

    /// <summary>Defines a proxy prefix over an emitted generic construction.</summary>
    internal static MethodBuilder DefinePrefix(
        TypeBuilder cache,
        MethodInfo source,
        Type returnType,
        MockIlParameter[] parameters,
        IReadOnlyDictionary<Type, Type> substitutions,
        Type? callbackType)
    {
        Type[] types = CreatePrefixTypes(returnType, parameters);
        MethodBuilder prefix = cache.DefineMethod(
            "Prefix",
            MethodAttributes.Assembly |
            MethodAttributes.Static |
            MethodAttributes.HideBySig,
            CallingConventions.Standard);
        prefix.SetSignature(
            typeof(bool),
            null,
            null,
            types,
            CreateModifiers(
                source,
                types.Length,
                substitutions,
                required: true),
            CreateModifiers(
                source,
                types.Length,
                substitutions,
                required: false));
        DefinePrefixParameters(prefix, source, returnType, types.Length);
        ImmutableArray<int> carrierIndices =
            MockIlParameter.CreateCarrierIndices(parameters);
        MockTypedTrampolineIl.Emit(
            prefix.GetILGenerator(),
            returnType,
            parameters,
            carrierIndices,
            callbackType,
            MockBackendLabel.ProxyInstance);
        return prefix;
    }

    /// <summary>Defines exact completion for a managed-reference factory.</summary>
    internal static MethodBuilder DefineFinalizer(
        TypeBuilder cache,
        MethodInfo source)
    {
        ParameterInfo[] parameters = source.GetParameters();
        MethodBuilder finalizer = cache.DefineMethod(
            "Finalizer",
            MethodAttributes.Assembly |
            MethodAttributes.Static |
            MethodAttributes.HideBySig,
            CallingConventions.Standard);
        MockTypedFinalizerEmitter.SetSignature(
            finalizer,
            source,
            parameters);
        MockTypedFinalizerEmitter.DefineParameters(
            finalizer,
            source,
            parameters);
        MockTypedFinalizerIl.Emit(
            finalizer.GetILGenerator(),
            source,
            parameters,
            MockIlParameter.CreateCarrierIndices(
                MockIlParameter.Create(parameters)));
        return finalizer;
    }

    private static Type[] CreatePrefixTypes(
        Type returnType,
        MockIlParameter[] parameters)
    {
        bool isVoid = returnType == typeof(void);
        var types = new Type[parameters.Length + (isVoid ? 3 : 4)];
        types[0] = typeof(MethodInfo);
        types[1] = typeof(object);
        int parameterOffset = 2;
        if (!isVoid)
        {
            types[2] = MockManagedReferenceAbi.IsSupported(returnType)
                ? MockManagedReferenceAbi.InjectionType(returnType)
                : returnType.IsByRef
                    ? returnType
                    : returnType.MakeByRefType();
            parameterOffset = 3;
        }

        for (int index = 0; index < parameters.Length; index++)
            types[index + parameterOffset] = parameters[index].Type;
        types[^1] = typeof(MockDispatchContinuation).MakeByRefType();
        return types;
    }

    private static Type[][] CreateModifiers(
        MethodInfo source,
        int count,
        IReadOnlyDictionary<Type, Type> substitutions,
        bool required)
    {
        var result = new Type[count][];
        for (int index = 0; index < count; index++)
            result[index] = [];

        int offset = source.ReturnType == typeof(void) ? 2 : 3;
        if (source.ReturnType != typeof(void) &&
            !MockManagedReferenceAbi.IsSupported(source.ReturnType))
        {
            result[2] = MockGenericTypeSubstitution.Replace(
                required
                    ? source.ReturnParameter.GetRequiredCustomModifiers()
                    : source.ReturnParameter.GetOptionalCustomModifiers(),
                substitutions);
        }

        ParameterInfo[] parameters = source.GetParameters();
        for (int index = 0; index < parameters.Length; index++)
        {
            result[index + offset] = MockGenericTypeSubstitution.Replace(
                required
                    ? parameters[index].GetRequiredCustomModifiers()
                    : parameters[index].GetOptionalCustomModifiers(),
                substitutions);
        }

        return result;
    }

    private static void DefinePrefixParameters(
        MethodBuilder prefix,
        MethodInfo source,
        Type returnType,
        int count)
    {
        prefix.DefineParameter(1, ParameterAttributes.None, "__originalMethod");
        prefix.DefineParameter(2, ParameterAttributes.None, "__instance");
        int offset = 2;
        if (returnType != typeof(void))
        {
            bool managedReference =
                MockManagedReferenceAbi.IsSupported(returnType);
            prefix.DefineParameter(
                3,
                managedReference
                    ? ParameterAttributes.None
                    : source.ReturnParameter.Attributes,
                managedReference ? "__resultRef" : "__result");
            offset = 3;
        }

        ParameterInfo[] parameters = source.GetParameters();
        for (int index = 0; index < parameters.Length; index++)
        {
            ParameterBuilder generated = prefix.DefineParameter(
                index + offset + 1,
                parameters[index].Attributes,
                parameters[index].Name);
            CopyScopedMetadata(parameters[index], generated);
        }

        prefix.DefineParameter(
            count,
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
