namespace AlvorKit;

/// <summary>Builds and caches logical argument index maps for intercepted methods.</summary>
internal static class Indices
{
    /// <summary>Returns the declared-order carrier index for each parameter.</summary>
    internal static int[] ParameterIndices(TypeCache type, MethodInfo method)
    {
        if (type.ParameterIndices.TryGetValue(method, out var val))
            return val;

        var indices = new int[type.GetParameters(method).Length];
        for (int index = 0; index < indices.Length; index++)
            indices[index] = index;

        type.ParameterIndices.TryAdd(method, indices);
        return indices;
    }

    /// <summary>Returns declared indices corresponding to heap-safe ref and out parameters.</summary>
    internal static int[] RefParameterIndices(TypeCache type, MethodInfo method)
    {
        if (type.RefParameterIndices.TryGetValue(method, out var val))
            return val;

        ParameterInfo[] parameters = type.GetParameters(method);
        var indices = new List<int>();
        for (int index = 0; index < parameters.Length; index++)
        {
            Type parameterType = parameters[index].ParameterType;
            if (parameterType.IsByRef &&
                !parameterType.GetElementType()!.IsByRefLike)
            {
                indices.Add(index);
            }
        }

        int[] result = [.. indices];
        type.RefParameterIndices.TryAdd(method, result);
        return result;
    }

}
