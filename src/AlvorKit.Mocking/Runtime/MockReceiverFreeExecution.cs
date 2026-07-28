namespace AlvorKit.Mocking;

/// <summary>Executes configured receiver-free operations or prepares their continuation.</summary>
internal static class MockReceiverFreeExecution
{
    /// <summary>Executes one selected receiver-free setup.</summary>
    internal static bool Execute(
        MockReceiverFreeSetup setup,
        object instance,
        Mocked mocked,
        MethodInfo method,
        object?[] arguments,
        MockInvocationToken token,
        ref MockInvocationExecutionSource source,
        MockTypedMatcherEvaluation? matcherEvaluation,
        out object? result,
        out MockDispatchContinuation? continuation)
    {
        result = null;
        continuation = null;
        MockReceiverFreeBehavior behavior = setup.Behavior;

        if (behavior.Kind ==
            MockReceiverFreeBehaviorKind.Passthrough)
        {
            continuation = new(
                mocked,
                token,
                method,
                arguments,
                projectedReceiverFreeSetup: setup);
            return false;
        }
        if (behavior.Kind ==
            MockReceiverFreeBehaviorKind.Strict)
        {
            source =
                MockInvocationExecutionSource.StrictFailure;
            throw new MockException(
                $"Unexpected receiver-free invocation of " +
                $"'{mocked.ReceiverFree!.Operation.DeclaringType?.FullName}." +
                $"{mocked.ReceiverFree.Operation.Name}' at interception site " +
                $"'{mocked.ReceiverFree.Site}'.");
        }

        MockBehaviorExecution? configured =
            setup.ClaimConfigured();
        if (configured is not null)
        {
            MockBehaviorExecution execution = configured.Value;
            MockBehaviorClaimExecution.Execute(
                execution,
                instance,
                mocked,
                method,
                arguments,
                out result);
            if (matcherEvaluation?.HasExitProjectors != true)
            {
                result = MockInvocationCapture.CompleteReturned(
                    mocked,
                    token,
                    method,
                    arguments,
                    result,
                    MockInvocationExecutionSource.Configured);
            }
            return true;
        }

        if (behavior.Kind is
            MockReceiverFreeBehaviorKind.ReturnFactory or
            MockReceiverFreeBehaviorKind.SubstituteFactory)
        {
            Delegate callback = behavior.Callback!;
            bool exactArguments =
                callback.Method.GetParameters().Length != 0;
            Delegate normalized = exactArguments
                ? setup.GetNormalizedCallback(method)
                : callback;
            continuation = new(
                mocked,
                token,
                method,
                normalized,
                exactArguments
                    ? MockBehaviorExecutionKind.TypedCallback
                    : MockBehaviorExecutionKind.TypedReturnFactory,
                originalArguments: arguments);
            return true;
        }

        if (behavior.Kind ==
            MockReceiverFreeBehaviorKind.Callback)
        {
            if (behavior.Callback is
                Func<MockCall, object?> answer)
            {
                result = answer(new(
                    instance,
                    mocked,
                    method,
                    arguments));
                result = MockInvocationCapture.CompleteReturned(
                    mocked,
                    token,
                    method,
                    arguments,
                    result,
                    MockInvocationExecutionSource.Configured,
                    null,
                    observeAsync: true);
                return true;
            }
            if (behavior.Callback is Action<MockCall> action)
            {
                action(new(
                    instance,
                    mocked,
                    method,
                    arguments));
                _ = MockInvocationCapture.CompleteReturned(
                    mocked,
                    token,
                    method,
                    arguments,
                    null,
                    MockInvocationExecutionSource.Configured);
                return true;
            }

            continuation = new(
                mocked,
                token,
                method,
                setup.GetNormalizedCallback(method),
                MockBehaviorExecutionKind.TypedCallback,
                originalArguments: arguments);
            return true;
        }

        if (mocked.ReceiverFree!.Operation is ConstructorInfo &&
            mocked.ReceiverFree.Site.OperationKind ==
                MockInvocationOperationKind.ConstructorBody &&
            behavior.Kind is
                MockReceiverFreeBehaviorKind.Observe or
                MockReceiverFreeBehaviorKind.Replace)
        {
            continuation = new(
                mocked,
                token,
                method,
                setup.GetNormalizedConstructorCallback(method),
                behavior.Kind,
                arguments,
                constructorBody: true);
            return false;
        }

        if (behavior.Kind is
            MockReceiverFreeBehaviorKind.Observe or
            MockReceiverFreeBehaviorKind.Transform)
        {
            if (mocked.ReceiverFree!.Operation is not FieldInfo)
            {
                throw new MockException(
                    $"Receiver-free behavior '{behavior.Kind}' is valid " +
                    "only for a field read or write.");
            }

            continuation = new(
                mocked,
                token,
                method,
                behavior.Callback!,
                behavior.Kind,
                arguments);
            return false;
        }

        throw new MockException(
            $"Receiver-free behavior '{behavior.Kind}' requires a runtime " +
            "execution mode that is not valid for this operation.");
    }
}
