namespace AlvorKit.Mocking;

/// <summary>Executes one heap-safe configured behavior claim.</summary>
internal static class MockBehaviorClaimExecution
{
    /// <summary>Executes a throw, callback, or constant return claim.</summary>
    internal static void Execute(
        MockBehaviorExecution execution,
        object instance,
        Mocked mocked,
        MethodInfo method,
        object?[] arguments,
        out object? result)
    {
        if (execution.Kind == MockBehaviorExecutionKind.Throw)
            throw (Exception)execution.Value!;

        if (execution.Kind == MockBehaviorExecutionKind.Callback)
        {
            var callback = (Func<MockCall, object?>)execution.Callback!;
            result = callback(new(instance, mocked, method, arguments));
            return;
        }

        if (execution.Kind != MockBehaviorExecutionKind.Return)
        {
            throw new MockException(
                $"Configured behavior '{execution.Kind}' for " +
                $"'{mocked.Type.Type.FullName}.{method.Name}' requires a typed dispatch backend.");
        }

        int[] referenceIndices =
            Indices.RefParameterIndices(mocked.Type, method);
        if (execution.ReferenceValues.Length != 0)
        {
            for (var i = 0; i < referenceIndices.Length; i++)
                arguments[referenceIndices[i]] = execution.ReferenceValues[i];
        }

        result = execution.Value;
    }
}
