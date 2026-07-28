namespace AlvorKit.Mocking;

/// <summary>
/// Defines shared exact metadata for generated dispatch completion methods.
/// </summary>
internal static class MockTypedFinalizerEmitter
{
    /// <summary>
    /// Applies the exception, state, result, and original argument signature.
    /// </summary>
    internal static void SetSignature(
        MethodBuilder finalizer,
        MethodInfo target,
        ParameterInfo[] parameters)
    {
        bool hasResult = target.ReturnType != typeof(void);
        int parameterOffset = hasResult ? 3 : 2;
        var types = new Type[parameters.Length + parameterOffset];
        var requiredModifiers = new Type[types.Length][];
        var optionalModifiers = new Type[types.Length][];
        types[0] = typeof(Exception);
        types[1] = typeof(MockDispatchContinuation);

        for (int index = 0; index < types.Length; index++)
        {
            requiredModifiers[index] = [];
            optionalModifiers[index] = [];
        }

        if (hasResult)
        {
            types[2] = target.ReturnType.IsByRef
                ? target.ReturnType
                : target.ReturnType.MakeByRefType();
            requiredModifiers[2] = target.ReturnParameter.GetRequiredCustomModifiers();
            optionalModifiers[2] = target.ReturnParameter.GetOptionalCustomModifiers();
        }

        for (int index = 0; index < parameters.Length; index++)
        {
            ParameterInfo parameter = parameters[index];
            types[index + parameterOffset] = parameter.ParameterType;
            requiredModifiers[index + parameterOffset] = parameter.GetRequiredCustomModifiers();
            optionalModifiers[index + parameterOffset] = parameter.GetOptionalCustomModifiers();
        }

        finalizer.SetSignature(
            typeof(Exception),
            null,
            null,
            types,
            requiredModifiers,
            optionalModifiers);
    }

    /// <summary>
    /// Applies dispatch special names and original parameter metadata.
    /// </summary>
    internal static void DefineParameters(
        MethodBuilder finalizer,
        MethodInfo target,
        ParameterInfo[] parameters)
    {
        finalizer.DefineParameter(1, ParameterAttributes.None, "__exception");
        finalizer.DefineParameter(2, ParameterAttributes.None, "__state");
        int parameterOffset = 2;

        if (target.ReturnType != typeof(void))
        {
            finalizer.DefineParameter(3, target.ReturnParameter.Attributes, "__result");
            parameterOffset = 3;
        }

        for (int index = 0; index < parameters.Length; index++)
        {
            ParameterInfo parameter = parameters[index];
            ParameterBuilder generatedParameter = finalizer.DefineParameter(
                index + parameterOffset + 1,
                parameter.Attributes,
                parameter.Name);
            CopyScopedMetadata(parameter, generatedParameter);
        }
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
}
