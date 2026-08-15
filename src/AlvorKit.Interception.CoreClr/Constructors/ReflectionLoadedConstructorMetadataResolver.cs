namespace AlvorKit;

/// <summary>Resolves constructor-initializer metadata in one loaded reflection module.</summary>
public sealed class ReflectionLoadedConstructorMetadataResolver(
    ConstructorInfo constructor) :
    ILoadedConstructorMetadataResolver
{
    private readonly Type declaringType = constructor.DeclaringType ??
        throw new ArgumentException(
            "A constructor must have a declaring type.",
            nameof(constructor));
    private readonly Module module = constructor.Module;

    /// <inheritdoc />
    public bool TryResolveMethod(
        int metadataToken,
        [NotNullWhen(true)] out LoadedMethodOperand? method)
    {
        MethodBase? resolved;
        try
        {
            resolved = module.ResolveMethod(
                metadataToken,
                declaringType.IsGenericType
                    ? declaringType.GetGenericArguments()
                    : null,
                null);
        }
        catch (Exception exception) when (
            exception is ArgumentException or
                BadImageFormatException or
                NotSupportedException)
        {
            method = null;
            return false;
        }

        if (resolved is null || resolved.DeclaringType is null)
        {
            method = null;
            return false;
        }

        Type owner = resolved.DeclaringType;
        method = new(
            resolved.ToString() ??
                $"{owner.FullName}.{resolved.Name}",
            !resolved.IsStatic,
            resolved is ConstructorInfo,
            (resolved.CallingConvention & CallingConventions.VarArgs) != 0,
            resolved.ContainsGenericParameters ||
                owner.ContainsGenericParameters,
            Shape(owner),
            owner.IsByRefLike,
            resolved.GetParameters().Length,
            resolved is MethodInfo methodInfo &&
                methodInfo.ReturnType != typeof(void));
        return true;
    }

    /// <inheritdoc />
    public bool TryResolveField(
        int metadataToken,
        [NotNullWhen(true)] out LoadedFieldOperand? field)
    {
        _ = metadataToken;
        field = null;
        return false;
    }

    /// <inheritdoc />
    public bool TryResolveType(
        int metadataToken,
        [NotNullWhen(true)] out LoadedTypeOperand? type)
    {
        _ = metadataToken;
        type = null;
        return false;
    }

    /// <inheritdoc />
    public bool TryResolveInitializerKind(
        int metadataToken,
        [NotNullWhen(true)] out LoadedConstructorInitializerKind? kind)
    {
        MethodBase? resolved;
        try
        {
            resolved = module.ResolveMethod(
                metadataToken,
                declaringType.IsGenericType
                    ? declaringType.GetGenericArguments()
                    : null,
                null);
        }
        catch (Exception exception) when (
            exception is ArgumentException or
                BadImageFormatException or
                NotSupportedException)
        {
            kind = null;
            return false;
        }

        if (resolved is not ConstructorInfo initializer)
        {
            kind = null;
            return false;
        }
        if (initializer.DeclaringType == declaringType)
        {
            kind = LoadedConstructorInitializerKind.This;
            return true;
        }
        if (initializer.DeclaringType == declaringType.BaseType)
        {
            kind = LoadedConstructorInitializerKind.Base;
            return true;
        }

        kind = null;
        return false;
    }

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
}
