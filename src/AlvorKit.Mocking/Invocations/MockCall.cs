namespace AlvorKit.Mocking;

/// <summary>Provides declared-order ordinary arguments and validated reference writeback.</summary>
public sealed class MockCall
{
    private readonly Mocked mocked;
    private readonly object?[] arguments;

    /// <summary>Creates a call context over one invocation-owned argument buffer.</summary>
    internal MockCall(
        object instance,
        Mocked mocked,
        MethodInfo method,
        object?[] arguments)
    {
        Instance = instance;
        this.mocked = mocked;
        Method = method;
        this.arguments = arguments;
    }

    /// <summary>Gets the intercepted mock instance.</summary>
    public object Instance { get; }

    /// <summary>Gets the intercepted method.</summary>
    public MethodInfo Method { get; }

    /// <summary>Gets an ordinary entry argument by declared parameter index.</summary>
    public T Argument<T>(int index)
    {
        var parameter = GetParameter(index);
        var valueType = GetValueType(parameter);

        if (valueType.IsByRefLike)
        {
            throw Error(
                index,
                "is byref-like and cannot be read through the ordinary MockCall context");
        }

        ValidateType<T>(index, valueType);

        if (parameter.IsOut)
        {
            throw Error(
                index,
                "is an out parameter and has no entry value");
        }

        var carrierIndex = Indices.ParameterIndices(mocked.Type, Method)[index];
        return (T)arguments[carrierIndex]!;
    }

    /// <summary>Sets a normal-exit value for an ordinary ref or out parameter.</summary>
    public void SetReference<T>(int index, T value)
    {
        var parameter = GetParameter(index);
        if (!parameter.ParameterType.IsByRef)
            throw Error(index, "is not a ref or out parameter");

        var valueType = parameter.ParameterType.GetElementType()!;
        if (valueType.IsByRefLike)
        {
            throw Error(
                index,
                "is byref-like and requires an exact-signature typed callback");
        }

        ValidateType<T>(index, valueType);

        var carrierIndex = Indices.ParameterIndices(mocked.Type, Method)[index];
        arguments[carrierIndex] = value;
    }

    private ParameterInfo GetParameter(int index)
    {
        var parameters = Method.GetParameters();
        if ((uint)index >= (uint)parameters.Length)
        {
            throw new MockException(
                $"Declared parameter index {index} is outside the range for " +
                $"'{Method.DeclaringType?.FullName}.{Method.Name}'.");
        }

        return parameters[index];
    }

    private void ValidateType<T>(int index, Type valueType)
    {
        var contextType = valueType.IsPointer
            ? typeof(nint)
            : valueType;
        if (typeof(T) != contextType)
        {
            throw Error(
                index,
                $"has type '{valueType.FullName}', not '{typeof(T).FullName}'");
        }
    }

    private MockException Error(int index, string detail) =>
        new(
            $"Declared parameter {index} on " +
            $"'{Method.DeclaringType?.FullName}.{Method.Name}' {detail}.");

    private static Type GetValueType(ParameterInfo parameter) =>
        parameter.ParameterType.IsByRef
            ? parameter.ParameterType.GetElementType()!
            : parameter.ParameterType;
}
