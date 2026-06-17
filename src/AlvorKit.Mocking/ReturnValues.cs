namespace AlvorKit.Mocking;

/// <summary>Stores configured return values and creates defaults for unmatched calls.</summary>
internal static partial class ReturnValues
{
    /// <summary>Adds a configured return value for one mocked method and argument signature.</summary>
    internal static void Add(Mocked mocked, MethodInfo method, object? arg, object?[] args, object?[] refs)
    {
        lock (mocked)
        {
            if (!mocked.ReturnValues.TryGetValue(method, out var list))
            {
                list = [];
                mocked.ReturnValues.TryAdd(method, list);
            }

            var indices = Indices.RefParameterIndices(mocked.Type, method).Length;
            if (refs.Length != indices && refs.Length != 0)
            {
                throw new MockException(
                    $"Reference parameter count mismatch for method '{method.Name}': expected {indices} or 0, but got {refs.Length}.");
            }

            list.Add((arg, args, refs));
        }
    }

    /// <summary>Returns the stable default value for one method on one mock.</summary>
    internal static object? GetDefault(Mocked mocked, MethodInfo method)
    {
        if (mocked.DefaultValues.TryGetValue(method, out var val))
            return val;

        lock (mocked)
        {
            if (mocked.DefaultValues.TryGetValue(method, out val))
                return val;

            val = CreateDefaultValue(method.ReturnType);

            mocked.DefaultValues.TryAdd(method, val);
            return val;
        }
    }

    /// <summary>Creates the default value returned by an unconfigured full mock method.</summary>
    internal static object? CreateDefaultValue(Type returnType)
    {
        if (returnType == typeof(void) || returnType.IsFunctionPointer || returnType.IsPointer)
            return null;

        if (typeof(Delegate).IsAssignableFrom(returnType) || returnType.IsByRefLike)
            return null;

        if (returnType.IsByRef)
            return CreateDefaultValue(returnType.GetElementType()!);

        if (returnType == typeof(string))
            return string.Empty;

        if (returnType.IsValueType)
            return Activator.CreateInstance(returnType);

        if (returnType.IsArray)
            return Array.CreateInstance(returnType.GetElementType()!, 0);

        if (typeof(ICollection).IsAssignableFrom(returnType))
            return Activator.CreateInstance(returnType);

        if (returnType.IsClass || returnType.IsInterface)
            return Mock.Create(returnType);

        return null;
    }
}
