namespace AlvorKit;

internal static partial class ProxyTypeBuilder
{
    /// <summary>Defines proxy method parameters with the original direction and metadata attributes.</summary>
    private static void DefineParameters(MethodBuilder methodBuilder, ParameterInfo[] parameters)
    {
        for (int i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            ParameterBuilder generatedParameter = methodBuilder.DefineParameter(
                i + 1,
                param.Attributes,
                param.Name);
            CopyScopedMetadata(param, generatedParameter);
        }
    }

    /// <summary>Defines the proxy return parameter with the original metadata.</summary>
    private static void DefineReturnParameter(
        MethodBuilder methodBuilder,
        ParameterInfo returnParameter)
    {
        ParameterBuilder generatedParameter = methodBuilder.DefineParameter(
            0,
            returnParameter.Attributes,
            returnParameter.Name);
        CopyScopedMetadata(returnParameter, generatedParameter);
    }

    /// <summary>Copies scoped parameter metadata needed by exact typed dispatch.</summary>
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
