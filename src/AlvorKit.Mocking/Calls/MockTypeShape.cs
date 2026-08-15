namespace AlvorKit;

/// <summary>Classifies runtime and emitted generic types that may carry borrowed values.</summary>
internal static class MockTypeShape
{
    /// <summary>Returns whether a type is or may close over a byref-like value.</summary>
    internal static bool MayBeByRefLike(Type type)
    {
        if (type.IsByRefLike)
            return true;
        if (type.IsGenericParameter)
        {
            return (type.GenericParameterAttributes
                    & GenericParameterAttributes.AllowByRefLike) != 0;
        }

        if (!type.IsValueType || !type.IsGenericType)
            return false;

        foreach (Type argument in type.GetGenericArguments())
        {
            if (MayBeByRefLike(argument))
                return true;
        }

        return false;
    }
}
