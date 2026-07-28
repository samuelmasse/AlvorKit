namespace AlvorKit.Mocking;

/// <summary>Appends declared and canonical signatures without runtime value formatting.</summary>
internal static class MockDiagnosticSignatureFormatter
{
    /// <summary>Appends a method using its declared type and exact parameter shapes.</summary>
    internal static void AppendSignature(
        StringBuilder message,
        Type targetType,
        MethodInfo method)
    {
        MockDiagnosticValueFormatter.AppendType(
            message,
            targetType);
        message.Append('.').Append(method.Name);

        if (method.IsGenericMethod)
        {
            message.Append('<');
            var genericArguments = method.GetGenericArguments();
            for (var i = 0; i < genericArguments.Length; i++)
            {
                if (i > 0)
                    message.Append(", ");

                MockDiagnosticValueFormatter.AppendType(
                    message,
                    genericArguments[i]);
            }

            message.Append('>');
        }

        message.Append('(');
        var parameters = method.GetParameters();
        for (var i = 0; i < parameters.Length; i++)
        {
            if (i > 0)
                message.Append(", ");

            var parameter = parameters[i];
            if (parameter.IsOut)
                message.Append("out ");
            else if (parameter.ParameterType.IsByRef)
                message.Append(parameter.IsIn ? "in " : "ref ");

            var parameterType = parameter.ParameterType.IsByRef
                ? parameter.ParameterType.GetElementType()!
                : parameter.ParameterType;
            MockDiagnosticValueFormatter.AppendType(
                message,
                parameterType);
        }

        message.Append(')');
    }

    /// <summary>Appends a stable backend-validation signature.</summary>
    internal static void AppendCanonicalSignature(
        StringBuilder message,
        MockCanonicalSignature signature)
    {
        message
            .Append(signature.CallingConvention)
            .Append(' ')
            .Append(signature.Return.Kind)
            .Append(':');
        MockDiagnosticValueFormatter.AppendType(
            message,
            signature.Return.Type.RuntimeType);
        message.Append(" (");
        var parameters = signature.Parameters;
        for (var i = 0; i < parameters.Length; i++)
        {
            if (i > 0)
                message.Append(", ");

            var parameter = parameters[i];
            message
                .Append(parameter.DeclaredIndex)
                .Append(':')
                .Append(parameter.Passing)
                .Append(':');
            MockDiagnosticValueFormatter.AppendType(
                message,
                parameter.Type.RuntimeType);
            message
                .Append(":in=")
                .Append(parameter.IsIn)
                .Append(":out=")
                .Append(parameter.IsOut)
                .Append(":scoped=")
                .Append(parameter.IsScoped);
        }

        message.Append(')');
    }
}
