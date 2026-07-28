namespace AlvorKit.Mocking;

/// <summary>Publishes heap-safe constant, sequence, throw, and callback setups.</summary>
internal static class MockedOrdinarySetupPublishing
{
    /// <summary>Adds a configured constant behavior for a captured call.</summary>
    internal static void AddConstant(
        this Mocked mocked,
        MethodInfo method,
        object?[] arguments,
        object? value,
        object?[] referenceValues) =>
        mocked.AddConstant(
            method,
            arguments,
            value,
            referenceValues,
            []);

    /// <summary>Adds a constant behavior with typed history projectors.</summary>
    internal static void AddConstant(
        this Mocked mocked,
        MethodInfo method,
        object?[] arguments,
        object? value,
        object?[] referenceValues,
        ReadOnlySpan<MockSnapshotProjector> projectors)
    {
        int referenceCount =
            Indices.RefParameterIndices(mocked.Type, method).Length;
        if (referenceValues.Length != 0 &&
            referenceValues.Length != referenceCount)
        {
            throw new MockException(
                $"Reference parameter count mismatch for method '{method.Name}': " +
                $"expected {referenceCount} or 0, but got {referenceValues.Length}.");
        }

        MockedSetupPublication.Publish(
            mocked,
            method,
            arguments,
            new MockConstantBehavior(value, referenceValues),
            projectors);
    }

    /// <summary>Adds configured throw behavior for a captured call.</summary>
    internal static void AddThrow(
        this Mocked mocked,
        MethodInfo method,
        object?[] arguments,
        Exception exception) =>
        mocked.AddThrow(method, arguments, exception, []);

    /// <summary>Adds a configured throw behavior with typed history projectors.</summary>
    internal static void AddThrow(
        this Mocked mocked,
        MethodInfo method,
        object?[] arguments,
        Exception exception,
        ReadOnlySpan<MockSnapshotProjector> projectors) =>
        MockedSetupPublication.Publish(
            mocked,
            method,
            arguments,
            new MockThrowBehavior(exception),
            projectors);

    /// <summary>Adds configured return-sequence behavior for a captured call.</summary>
    internal static void AddReturnSequence(
        this Mocked mocked,
        MethodInfo method,
        object?[] arguments,
        ReadOnlySpan<object?> values) =>
        mocked.AddReturnSequence(method, arguments, values, []);

    /// <summary>Adds a return sequence with typed history projectors.</summary>
    internal static void AddReturnSequence(
        this Mocked mocked,
        MethodInfo method,
        object?[] arguments,
        ReadOnlySpan<object?> values,
        ReadOnlySpan<MockSnapshotProjector> projectors) =>
        MockedSetupPublication.Publish(
            mocked,
            method,
            arguments,
            new MockReturnSequenceBehavior(values),
            projectors);

    /// <summary>Adds ordinary callback behavior for a captured call.</summary>
    internal static void AddCallback(
        this Mocked mocked,
        MethodInfo method,
        object?[] arguments,
        Func<MockCall, object?> callback) =>
        mocked.AddCallback(method, arguments, callback, []);

    /// <summary>Adds an ordinary callback with typed history projectors.</summary>
    internal static void AddCallback(
        this Mocked mocked,
        MethodInfo method,
        object?[] arguments,
        Func<MockCall, object?> callback,
        ReadOnlySpan<MockSnapshotProjector> projectors) =>
        MockedSetupPublication.Publish(
            mocked,
            method,
            arguments,
            new MockCallbackBehavior(callback),
            projectors);

    /// <summary>Normalizes and adds one exact typed callback.</summary>
    internal static void AddTypedCallback(
        this Mocked mocked,
        MethodInfo method,
        object?[] arguments,
        Delegate callback,
        ReadOnlySpan<MockSnapshotProjector> projectors)
    {
        if (method.ReturnType.IsByRef)
        {
            throw new MockException(
                $"Managed-reference return '{method.Name}' cannot use a typed " +
                "callback. Configure it with Mock.WhenRef or " +
                "Mock.WhenRefReadonly.");
        }

        if (method.ReturnType.IsPointer ||
            method.ReturnType.IsFunctionPointer)
        {
            throw new MockException(
                $"Pointer-shaped return '{method.Name}' cannot use a typed " +
                "callback.");
        }

        Delegate normalized =
            MockTypedCallbackContract.Normalize(callback, method);
        MockedSetupPublication.Publish(
            mocked,
            method,
            arguments,
            new MockTypedCallbackBehavior(normalized),
            projectors);
    }
}
