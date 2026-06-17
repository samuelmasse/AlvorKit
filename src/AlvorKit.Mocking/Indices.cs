namespace AlvorKit.Mocking;

/// <summary>Builds and caches logical argument index maps for intercepted methods.</summary>
internal static class Indices
{
    /// <summary>Classifies parameters into value, by-reference, by-ref-like, and by-reference by-ref-like groups.</summary>
    internal static (int[], int[], int[], int[]) ClassifyParameters(ParameterInfo[] parameters)
    {
        var value = new List<int>();
        var refs = new List<int>();
        var refStructs = new List<int>();
        var refStructRefs = new List<int>();

        for (int i = 0; i < parameters.Length; i++)
        {
            var type = parameters[i].ParameterType;

            if (type.IsByRef)
            {
                if (type.GetElementType()!.IsByRefLike)
                    refStructRefs.Add(i);
                else refs.Add(i);
            }
            else if (type.IsByRefLike)
                refStructs.Add(i);
            else value.Add(i);
        }

        return ([.. value], [.. refs], [.. refStructs], [.. refStructRefs]);
    }

    /// <summary>Returns the logical matching index for each declared parameter.</summary>
    internal static int[] ParameterIndices(TypeCache type, MethodInfo method)
    {
        if (type.ParameterIndices.TryGetValue(method, out var val))
            return val;

        var parameters = method.GetParameters();

        var (valueParamIndices, refParamIndices, refStructParamIndices, refStructRefParamIndices) = ClassifyParameters(parameters);

        var indices = new int[parameters.Length];
        int index = 0;

        Traverse(valueParamIndices);
        Traverse(refParamIndices);
        Traverse(refStructParamIndices);
        Traverse(refStructRefParamIndices);

        void Traverse(int[] subIndices)
        {
            foreach (int i in subIndices)
                indices[i] = index++;
        }

        type.ParameterIndices.TryAdd(method, indices);
        return indices;
    }

    /// <summary>Returns logical argument indices corresponding to ref and out parameters.</summary>
    internal static int[] RefParameterIndices(TypeCache type, MethodInfo method)
    {
        if (type.RefParameterIndices.TryGetValue(method, out var val))
            return val;

        var parameters = method.GetParameters();
        var (valueParams, refParams, _, _) = ClassifyParameters(parameters);

        var indices = new int[refParams.Length];
        int baseIndex = valueParams.Length;

        for (int i = 0; i < refParams.Length; i++)
            indices[i] = baseIndex + i;

        type.RefParameterIndices.TryAdd(method, indices);
        return indices;
    }

    /// <summary>Returns logical argument indices corresponding only to out parameters.</summary>
    internal static int[] OutParameterIndices(TypeCache type, MethodInfo method)
    {
        if (type.OutParameterIndices.TryGetValue(method, out var val))
            return val;

        var parameters = method.GetParameters();
        var (valueParams, refParams, _, _) = ClassifyParameters(parameters);

        var outParamIndices = refParams.Where(i => parameters[i].IsOut).ToArray();

        var indices = new int[outParamIndices.Length];
        int baseIndex = valueParams.Length;

        int j = 0;
        for (int i = 0; i < refParams.Length; i++)
        {
            int paramIndex = refParams[i];
            if (parameters[paramIndex].IsOut)
                indices[j++] = baseIndex + i;
        }

        type.OutParameterIndices.TryAdd(method, indices);
        return indices;
    }
}
