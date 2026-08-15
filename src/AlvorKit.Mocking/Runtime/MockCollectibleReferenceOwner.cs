namespace AlvorKit;

/// <summary>
/// Selects a weak cache owner for metadata that can reference collectible
/// runtime constructions.
/// </summary>
internal static class MockCollectibleReferenceOwner
{
    /// <summary>
    /// Returns the first collectible module referenced by a member's exact
    /// construction, or the member's definition module when none is present.
    /// </summary>
    internal static Module Select(MemberInfo member)
    {
        ArgumentNullException.ThrowIfNull(member);
        return CollectibleModule(member) ?? member.Module;
    }

    /// <summary>
    /// Returns the first collectible module referenced by either exact shape,
    /// or the member's definition module when neither contains one.
    /// </summary>
    internal static Module Select(
        MemberInfo member,
        Type additionalType)
    {
        ArgumentNullException.ThrowIfNull(member);
        ArgumentNullException.ThrowIfNull(additionalType);
        return CollectibleModule(member) ??
            CollectibleModule(additionalType) ??
            member.Module;
    }

    private static Module? CollectibleModule(MemberInfo member)
    {
        if (member.Module.Assembly.IsCollectible)
            return member.Module;

        Module? owner = CollectibleModule(member.DeclaringType);
        if (owner is not null)
            return owner;

        return member switch
        {
            MethodInfo method => CollectibleModule(method),
            ConstructorInfo constructor =>
                CollectibleModule(constructor.GetParameters()),
            FieldInfo field => CollectibleModule(field.FieldType),
            _ => null
        };
    }

    private static Module? CollectibleModule(MethodInfo method)
    {
        Module? owner = CollectibleModule(method.GetGenericArguments()) ??
            CollectibleModule(method.ReturnType) ??
            CollectibleModule(method.ReturnParameter);
        return owner ?? CollectibleModule(method.GetParameters());
    }

    private static Module? CollectibleModule(ParameterInfo parameter) =>
        CollectibleModule(parameter.ParameterType) ??
        CollectibleModule(parameter.GetRequiredCustomModifiers()) ??
        CollectibleModule(parameter.GetOptionalCustomModifiers());

    private static Module? CollectibleModule(
        IEnumerable<ParameterInfo> parameters)
    {
        foreach (ParameterInfo parameter in parameters)
        {
            Module? owner = CollectibleModule(parameter);
            if (owner is not null)
                return owner;
        }

        return null;
    }

    private static Module? CollectibleModule(IEnumerable<Type> types)
    {
        foreach (Type type in types)
        {
            Module? owner = CollectibleModule(type);
            if (owner is not null)
                return owner;
        }

        return null;
    }

    private static Module? CollectibleModule(Type? type)
    {
        if (type is null)
            return null;
        if (type.Assembly.IsCollectible)
            return type.Module;

        if (type.HasElementType)
        {
            Module? owner = CollectibleModule(type.GetElementType());
            if (owner is not null)
                return owner;
        }

        if (type.IsFunctionPointer)
        {
            return CollectibleModule(
                    type.GetFunctionPointerReturnType()) ??
                CollectibleModule(
                    type.GetFunctionPointerParameterTypes());
        }

        return CollectibleModule(type.GetGenericArguments());
    }
}
