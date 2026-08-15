namespace AlvorKit;

/// <summary>Maps ordinary matcher captures back to declared argument positions.</summary>
internal static class CaptureOrdinaryMatcherProcessing
{
    /// <summary>Replaces disambiguated argument carriers with their captured matchers.</summary>
    internal static void Process(
        CaptureContext context,
        MethodInfo method,
        object?[] arguments,
        List<Matcher> matchers)
    {
        var mocked = Mock.GetMocked(context.Instance!)!;
        int[] indices = Indices.ParameterIndices(mocked.Type, method);
        ParameterInfo[] parameters = method.GetParameters();
        object?[] secondArguments = context.Args!;
        var comparer = new MockOrdinaryArgumentComparer();
        Span<int> differences = stackalloc int[indices.Length];
        var differenceCount = 0;
        int parameterOffset = context.ExpectedOperationKind is
            MockInvocationOperationKind.ConstructorBody or
            MockInvocationOperationKind.StructMethod
            ? 1
            : 0;

        for (var i = parameterOffset; i < indices.Length; i++)
        {
            int carrierIndex = indices[i];
            Type declaredType = parameters[i].ParameterType;
            Type valueType = declaredType.IsByRef
                ? declaredType.GetElementType()!
                : declaredType;
            if (!valueType.IsByRefLike &&
                !comparer.Equals(
                    arguments[carrierIndex],
                    secondArguments[carrierIndex],
                    valueType))
            {
                differences[differenceCount++] = carrierIndex;
            }
        }

        if (differenceCount < matchers.Count)
        {
            for (var i = parameterOffset;
                 i < parameters.Length &&
                 differenceCount < matchers.Count;
                 i++)
            {
                if (!parameters[i].IsOut)
                    continue;

                int carrierIndex = indices[i];
                if (!differences[..differenceCount].Contains(carrierIndex))
                    differences[differenceCount++] = carrierIndex;
            }
        }

        if (differenceCount != matchers.Count)
        {
            throw new MockException(
                $"Matcher capture recorded {matchers.Count} matchers but " +
                $"identified {differenceCount} declared argument positions.");
        }

        for (var i = 0; i < matchers.Count; i++)
            arguments[differences[i]] = matchers[i];
    }
}
