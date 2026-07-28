namespace AlvorKit.Mocking;

/// <summary>
/// Substitutes method generic parameters into emitted method and cache owners.
/// </summary>
internal static class MockGenericTypeSubstitution
{
    /// <summary>Creates an original-to-emitted generic parameter map.</summary>
    internal static Dictionary<Type, Type> CreateMap(
        Type[] original,
        GenericTypeParameterBuilder[] emitted)
    {
        var result = new Dictionary<Type, Type>(original.Length);
        for (int index = 0; index < original.Length; index++)
            result.Add(original[index], emitted[index]);
        return result;
    }

    /// <summary>Copies generic attributes and substituted constraints.</summary>
    internal static void CopyConstraints(
        Type[] original,
        GenericTypeParameterBuilder[] emitted,
        IReadOnlyDictionary<Type, Type> substitutions)
    {
        for (int index = 0; index < emitted.Length; index++)
        {
            Type source = original[index];
            GenericTypeParameterBuilder destination = emitted[index];
            destination.SetGenericParameterAttributes(
                source.GenericParameterAttributes);

            Type[] constraints = source.GetGenericParameterConstraints();
            Type? baseConstraint = null;
            var interfaceConstraints = new List<Type>();
            foreach (Type constraint in constraints)
            {
                Type mapped = Replace(constraint, substitutions);
                if (constraint.IsInterface)
                    interfaceConstraints.Add(mapped);
                else
                    baseConstraint = mapped;
            }

            if (baseConstraint is not null)
                destination.SetBaseTypeConstraint(baseConstraint);
            if (interfaceConstraints.Count > 0)
                destination.SetInterfaceConstraints([.. interfaceConstraints]);
        }
    }

    /// <summary>Substitutes generic parameters through compound CLR type forms.</summary>
    internal static Type Replace(
        Type type,
        IReadOnlyDictionary<Type, Type> substitutions)
    {
        if (substitutions.TryGetValue(type, out Type? replacement))
            return replacement;
        if (type.IsByRef)
            return Replace(type.GetElementType()!, substitutions).MakeByRefType();
        if (type.IsPointer)
            return Replace(type.GetElementType()!, substitutions).MakePointerType();
        if (type.IsArray)
        {
            Type element = Replace(type.GetElementType()!, substitutions);
            return type.GetArrayRank() == 1
                ? element.MakeArrayType()
                : element.MakeArrayType(type.GetArrayRank());
        }
        if (!type.IsGenericType)
            return type;

        Type[] arguments = type.GetGenericArguments();
        for (int index = 0; index < arguments.Length; index++)
            arguments[index] = Replace(arguments[index], substitutions);
        return type.GetGenericTypeDefinition().MakeGenericType(arguments);
    }

    /// <summary>Substitutes every type in a custom-modifier vector.</summary>
    internal static Type[] Replace(
        Type[] types,
        IReadOnlyDictionary<Type, Type> substitutions)
    {
        var result = new Type[types.Length];
        for (int index = 0; index < types.Length; index++)
            result[index] = Replace(types[index], substitutions);
        return result;
    }
}
