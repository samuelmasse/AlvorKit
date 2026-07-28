namespace AlvorKit.Mocking;

/// <summary>Completes returned and thrown dispatch continuations.</summary>
internal static class MockDispatchCompletion
{
    /// <summary>Completes an invocation after the original implementation returns.</summary>
    internal static void CompleteReturned(
        this MockDispatchContinuation continuation,
        object?[] arguments,
        object? result) =>
        _ = CompleteReturned(
            continuation,
            arguments,
            result,
            continuation.IsTypedRefReturnFactory
                ? MockInvocationExecutionSource.Configured
                : OriginalExecutionSource(continuation),
            retainResult: !continuation.IsTypedRefReturnFactory,
            observeAsync: false);

    /// <summary>Completes a configured typed factory and retains its result.</summary>
    internal static object? CompleteTypedReturned(
        this MockDispatchContinuation continuation,
        object?[] arguments,
        object? result)
    {
        try
        {
            ValidateConstructionResult(continuation, result);
            return CompleteReturned(
                continuation,
                arguments,
                result,
                MockInvocationExecutionSource.Configured,
                retainResult: true,
                observeAsync: true);
        }
        catch (Exception exception)
        {
            MockInvocationCapture.CompleteThrown(
                continuation.Mocked,
                continuation.Token,
                MockInvocationExecutionSource.Configured,
                exception,
                MockInvocationFailureStage.ReturnFactory);
            throw;
        }
    }

    /// <summary>Completes a typed return that must not enter the control plane.</summary>
    internal static void CompleteTypedUnretainedReturned(
        this MockDispatchContinuation continuation,
        object?[] arguments) =>
        CompleteReturned(
            continuation,
            arguments,
            null,
            MockInvocationExecutionSource.Configured,
            retainResult: false,
            observeAsync: false);

    /// <summary>Completes the invocation and publishes eligible exit values.</summary>
    private static object? CompleteReturned(
        MockDispatchContinuation continuation,
        object?[] arguments,
        object? result,
        MockInvocationExecutionSource source,
        bool retainResult,
        bool observeAsync)
    {
        MockAsyncReturn? asyncReturn = observeAsync
            ? MockAsyncReturn.Prepare(
                continuation.Method.ReturnType,
                result)
            : null;
        object? normalizedResult =
            asyncReturn?.ReturnValue ?? result;
        ParameterInfo[] parameters =
            continuation.Mocked.Type.GetParameters(
                continuation.Method);
        int[] carrierIndices = Indices.ParameterIndices(
            continuation.Mocked.Type,
            continuation.Method);

        for (var index = 0; index < parameters.Length; index++)
        {
            Type declaredType = parameters[index].ParameterType;
            if (!declaredType.IsByRef ||
                declaredType.GetElementType()!.IsByRefLike ||
                continuation.HasProjector(
                    index,
                    MockSnapshotPhase.Exit))
            {
                continue;
            }

            continuation.Mocked.Invocations.PublishProjection(
                continuation.Token,
                MockInvocationArgumentSnapshot.Shallow(
                    index,
                    declaredType,
                    MockSnapshotPhase.Exit,
                    arguments[carrierIndices[index]]));
        }

        Type returnType = continuation.Method.ReturnType;
        MockInvocationReturn returned = returnType == typeof(void)
            ? MockInvocationReturn.Void()
            : !retainResult ||
                returnType.IsByRef ||
                returnType.IsByRefLike
                ? MockInvocationReturn.Unavailable(returnType)
                : MockInvocationReturn.Shallow(
                    returnType,
                    normalizedResult);
        continuation.Mocked.Invocations.CompleteReturned(
            continuation.Token,
            source,
            returned);
        asyncReturn?.Observe(continuation.Token.Slot);
        return normalizedResult;
    }

    /// <summary>Completes the invocation with its exact original exception.</summary>
    internal static void CompleteThrown(
        this MockDispatchContinuation continuation,
        Exception exception) =>
        MockInvocationCapture.CompleteThrown(
            continuation.Mocked,
            continuation.Token,
            continuation.IsTypedRefReturnFactory
                ? MockInvocationExecutionSource.Configured
                : OriginalExecutionSource(continuation),
            exception,
            continuation.IsTypedRefReturnFactory
                ? MockInvocationFailureStage.ReturnFactory
                : MockInvocationFailureStage.OriginalImplementation);

    /// <summary>Gets the source used when generated code executes the original path.</summary>
    private static MockInvocationExecutionSource OriginalExecutionSource(
        MockDispatchContinuation continuation) =>
        continuation.IsReceiverFreeFieldBehavior ||
        continuation.IsReceiverFreeConstructorBehavior
            ? MockInvocationExecutionSource.Configured
            : continuation.Mocked.ReceiverFree is null
                ? MockInvocationExecutionSource.PartialPassthrough
                : MockInvocationExecutionSource.ReceiverFreeOriginal;

    /// <summary>Completes an exact field observer or transformer failure.</summary>
    internal static void CompleteReceiverFreeBehaviorThrown(
        this MockDispatchContinuation continuation,
        Exception exception) =>
        MockInvocationCapture.CompleteThrown(
            continuation.Mocked,
            continuation.Token,
            MockInvocationExecutionSource.Configured,
            exception,
            MockInvocationFailureStage.Behavior);

    /// <summary>Completes a constructor replacement without its remainder.</summary>
    internal static void CompleteReceiverFreeConstructorReplacement(
        this MockDispatchContinuation continuation)
    {
        if (!continuation.ReplacesReceiverFreeConstructorBody)
        {
            throw new InvalidOperationException(
                "The continuation is not a constructor-body replacement.");
        }

        _ = CompleteReturned(
            continuation,
            continuation.RetainedArguments!,
            null,
            MockInvocationExecutionSource.Configured,
            retainResult: false,
            observeAsync: false);
    }

    /// <summary>Validates a non-null assignable construction substitution.</summary>
    private static void ValidateConstructionResult(
        MockDispatchContinuation continuation,
        object? result)
    {
        if (continuation.Mocked.ReceiverFree?.Operation is not
            ConstructorInfo constructor)
        {
            return;
        }

        Type constructedType = constructor.DeclaringType ??
            throw new MockException(
                "Construction metadata has no declaring type.");
        if (result is null ||
            !constructedType.IsInstanceOfType(result))
        {
            throw new MockException(
                $"Construction substitution for " +
                $"'{constructedType.FullName}' returned " +
                $"{(result is null ? "null" : $"'{result.GetType()}'")}; " +
                "the result must be non-null and assignable to the " +
                "constructed type.");
        }
    }

    /// <summary>Completes a typed factory with its exact exception.</summary>
    internal static void CompleteTypedThrown(
        this MockDispatchContinuation continuation,
        Exception exception) =>
        MockInvocationCapture.CompleteThrown(
            continuation.Mocked,
            continuation.Token,
            MockInvocationExecutionSource.Configured,
            exception,
            MockInvocationFailureStage.ReturnFactory);

    /// <summary>Completes a typed callback with its exact exception.</summary>
    internal static void CompleteTypedCallbackThrown(
        this MockDispatchContinuation continuation,
        Exception exception) =>
        MockInvocationCapture.CompleteThrown(
            continuation.Mocked,
            continuation.Token,
            MockInvocationExecutionSource.Configured,
            exception,
            MockInvocationFailureStage.Behavior);
}
