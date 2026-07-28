namespace AlvorKit.Mocking;

internal static partial class ProxyTypeBuilder
{
    /// <summary>
    /// Defines a proxy-owned generic method whose exact dispatch is emitted directly in its body.
    /// </summary>
    private static TypeBuilder DefineGenericMethod(
        ModuleBuilder module,
        TypeBuilder typeBuilder,
        MethodInfo method)
    {
        MethodBuilder proxyMethod = typeBuilder.DefineMethod(
            method.Name,
            MethodAttributes.Public | MethodAttributes.Virtual,
            CallingConventions.HasThis);
        Type[] originalArguments = method.GetGenericArguments();
        GenericTypeParameterBuilder[] proxyArguments =
            proxyMethod.DefineGenericParameters(
                [.. originalArguments.Select(static argument => argument.Name)]);
        Dictionary<Type, Type> substitutions =
            MockGenericTypeSubstitution.CreateMap(
                originalArguments,
                proxyArguments);
        MockGenericTypeSubstitution.CopyConstraints(
            originalArguments,
            proxyArguments,
            substitutions);

        ParameterInfo[] sourceParameters = method.GetParameters();
        Type proxyReturnType = MockGenericTypeSubstitution.Replace(
            method.ReturnType,
            substitutions);
        Type[] proxyParameterTypes = ReplaceParameterTypes(
            sourceParameters,
            substitutions);
        proxyMethod.SetSignature(
            proxyReturnType,
            MockGenericTypeSubstitution.Replace(
                method.ReturnParameter.GetRequiredCustomModifiers(),
                substitutions),
            MockGenericTypeSubstitution.Replace(
                method.ReturnParameter.GetOptionalCustomModifiers(),
                substitutions),
            proxyParameterTypes,
            ReplaceModifiers(
                sourceParameters,
                substitutions,
                required: true),
            ReplaceModifiers(
                sourceParameters,
                substitutions,
                required: false));
        DefineReturnParameter(proxyMethod, method.ReturnParameter);
        DefineParameters(proxyMethod, sourceParameters);

        TypeBuilder cache = MockProxyGenericMethodEmitter.Emit(
            module,
            proxyMethod,
            method,
            proxyArguments,
            proxyReturnType,
            CreateParameters(sourceParameters, proxyParameterTypes));
        typeBuilder.DefineMethodOverride(proxyMethod, method);
        return cache;
    }

    private static Type[] ReplaceParameterTypes(
        ParameterInfo[] parameters,
        IReadOnlyDictionary<Type, Type> substitutions)
    {
        var result = new Type[parameters.Length];
        for (int index = 0; index < parameters.Length; index++)
        {
            result[index] = MockGenericTypeSubstitution.Replace(
                parameters[index].ParameterType,
                substitutions);
        }

        return result;
    }

    private static Type[][] ReplaceModifiers(
        ParameterInfo[] parameters,
        IReadOnlyDictionary<Type, Type> substitutions,
        bool required)
    {
        var result = new Type[parameters.Length][];
        for (int index = 0; index < parameters.Length; index++)
        {
            Type[] modifiers = required
                ? parameters[index].GetRequiredCustomModifiers()
                : parameters[index].GetOptionalCustomModifiers();
            result[index] = MockGenericTypeSubstitution.Replace(
                modifiers,
                substitutions);
        }

        return result;
    }

    private static MockIlParameter[] CreateParameters(
        ParameterInfo[] source,
        Type[] emittedTypes)
    {
        var result = new MockIlParameter[source.Length];
        for (int index = 0; index < source.Length; index++)
        {
            result[index] = new(
                emittedTypes[index],
                source[index].IsIn,
                source[index].IsOut);
        }

        return result;
    }
}
