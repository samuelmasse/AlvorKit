namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Resolves operation operands in one loaded reflection method context.</summary>
public sealed class ReflectionLoadedOperationMetadataResolver :
    ILoadedOperationMetadataResolver
{
    private readonly MethodBase containingMethod;
    private readonly Type[] typeArguments;
    private readonly Type[] methodArguments;

    /// <summary>Creates a resolver for the exact constructed caller context.</summary>
    public ReflectionLoadedOperationMetadataResolver(
        MethodBase containingMethod)
    {
        ArgumentNullException.ThrowIfNull(containingMethod);
        this.containingMethod = containingMethod;
        typeArguments =
            containingMethod.DeclaringType?.GetGenericArguments() ?? [];
        methodArguments = containingMethod is MethodInfo method
            ? method.GetGenericArguments()
            : [];
        ConstructedContext = Context(typeArguments, methodArguments);
    }

    /// <summary>Gets the deterministic exact generic construction context.</summary>
    public string ConstructedContext { get; }

    /// <inheritdoc />
    public bool TryResolveMethod(
        int metadataToken,
        [NotNullWhen(true)] out LoadedMethodOperand? method)
    {
        MethodBase? resolved;
        try
        {
            resolved = containingMethod.Module.ResolveMethod(
                metadataToken,
                typeArguments,
                methodArguments);
        }
        catch (Exception exception) when (IsResolutionFailure(exception))
        {
            method = null;
            return false;
        }

        if (resolved?.DeclaringType is not { } declaringType)
        {
            method = null;
            return false;
        }

        method = new(
            MethodSignature(resolved),
            !resolved.IsStatic,
            resolved is ConstructorInfo,
            (resolved.CallingConvention & CallingConventions.VarArgs) != 0,
            resolved.ContainsGenericParameters ||
                declaringType.ContainsGenericParameters,
            Shape(declaringType),
            declaringType.IsByRefLike,
            resolved.GetParameters().Length,
            resolved is MethodInfo info &&
                info.ReturnType != typeof(void));
        return true;
    }

    /// <inheritdoc />
    public bool TryResolveField(
        int metadataToken,
        [NotNullWhen(true)] out LoadedFieldOperand? field)
    {
        FieldInfo? resolved;
        try
        {
            resolved = containingMethod.Module.ResolveField(
                metadataToken,
                typeArguments,
                methodArguments);
        }
        catch (Exception exception) when (IsResolutionFailure(exception))
        {
            field = null;
            return false;
        }

        if (resolved?.DeclaringType is not { } declaringType)
        {
            field = null;
            return false;
        }

        field = new(
            FieldSignature(resolved),
            resolved.IsStatic,
            declaringType.ContainsGenericParameters ||
                resolved.FieldType.ContainsGenericParameters,
            Shape(declaringType),
            declaringType.IsByRefLike);
        return true;
    }

    /// <inheritdoc />
    public bool TryResolveType(
        int metadataToken,
        [NotNullWhen(true)] out LoadedTypeOperand? type)
    {
        Type? resolved;
        try
        {
            resolved = containingMethod.Module.ResolveType(
                metadataToken,
                typeArguments,
                methodArguments);
        }
        catch (Exception exception) when (IsResolutionFailure(exception))
        {
            type = null;
            return false;
        }

        if (resolved is null)
        {
            type = null;
            return false;
        }

        type = new(TypeName(resolved), Shape(resolved), resolved.IsByRefLike);
        return true;
    }

    private static string Context(
        Type[] typeArguments,
        Type[] methodArguments)
    {
        if (typeArguments.Length == 0 && methodArguments.Length == 0)
            return "";

        return $"rc1|type={string.Join(',', typeArguments.Select(TypeName))}" +
            $"|method={string.Join(',', methodArguments.Select(TypeName))}";
    }

    private static string MethodSignature(MethodBase method)
    {
        var signature = new StringBuilder()
            .Append("method|")
            .Append(TypeName(method.DeclaringType!))
            .Append('|')
            .Append(method.Name)
            .Append('|')
            .Append((int)method.CallingConvention);
        if (method is MethodInfo info)
            Append(signature, info.ReturnParameter);
        else
            _ = signature.Append("|void");
        if (method is MethodInfo genericMethod)
        {
            foreach (Type argument in genericMethod.GetGenericArguments())
                _ = signature.Append("|generic:").Append(TypeName(argument));
        }
        foreach (ParameterInfo parameter in method.GetParameters())
            Append(signature, parameter);
        return signature.ToString();
    }

    private static string FieldSignature(FieldInfo field)
    {
        var signature = new StringBuilder()
            .Append("field|")
            .Append(TypeName(field.DeclaringType!))
            .Append('|')
            .Append(field.Name)
            .Append('|')
            .Append(TypeName(field.FieldType));
        Append(signature, field.GetRequiredCustomModifiers(), "req");
        Append(signature, field.GetOptionalCustomModifiers(), "opt");
        return signature.ToString();
    }

    private static void Append(
        StringBuilder signature,
        ParameterInfo parameter)
    {
        _ = signature
            .Append("|parameter:")
            .Append(TypeName(parameter.ParameterType))
            .Append(':')
            .Append((int)(parameter.Attributes &
                (ParameterAttributes.In | ParameterAttributes.Out)));
        Append(signature, parameter.GetRequiredCustomModifiers(), "req");
        Append(signature, parameter.GetOptionalCustomModifiers(), "opt");
    }

    private static void Append(
        StringBuilder signature,
        Type[] modifiers,
        string label)
    {
        foreach (Type modifier in modifiers)
        {
            _ = signature
                .Append(':')
                .Append(label)
                .Append('=')
                .Append(TypeName(modifier));
        }
    }

    private static string TypeName(Type type) =>
        type.AssemblyQualifiedName ??
        type.FullName ??
        type.ToString();

    private static LoadedTypeShape Shape(Type type)
    {
        if (type.IsGenericParameter)
            return LoadedTypeShape.GenericParameter;
        if (type.IsInterface)
            return LoadedTypeShape.Interface;
        return type.IsValueType
            ? LoadedTypeShape.ValueType
            : LoadedTypeShape.ReferenceType;
    }

    private static bool IsResolutionFailure(Exception exception) =>
        exception is
            ArgumentException or
            BadImageFormatException or
            NotSupportedException;
}
