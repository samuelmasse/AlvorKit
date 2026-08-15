namespace AlvorKit;

/// <summary>Dispatches intercepted calls into capture, event, and behavior handling.</summary>
internal static class Rewire
{
    /// <summary>Runs dispatch with an immutable instrumentation label.</summary>
    internal static bool Method(
        MethodInfo method,
        object instance,
        Mocked mocked,
        object?[] args,
        MockTypedMatcherEvaluation? matcherEvaluation,
        out object? result,
        out MockDispatchContinuation? continuation,
        string backend)
    {
        result = null;
        continuation = null;

        object?[] entryArguments = args;

        if (Capture.Context.IsActive)
        {
            if (!Capture.TryWrite(
                instance,
                method,
                entryArguments))
            {
                return false;
            }
            result = mocked.GetDefault(method);
            return true;
        }

        var token = matcherEvaluation?.Token ??
            MockInvocationCapture.Open(
                mocked,
                method,
                entryArguments,
                backend);
        var source = MockInvocationExecutionSource.Configured;
        var failureStage = MockInvocationFailureStage.Matcher;

        try
        {
            var capturedEvent = Events.Get(mocked, method);
            if (capturedEvent is not null && entryArguments[0] is not null)
            {
                source = MockInvocationExecutionSource.EventAccessor;
                Events.HandleAddAndRemove(
                    mocked,
                    method,
                    capturedEvent,
                    (Delegate)entryArguments[0]!);
                _ = MockInvocationCapture.CompleteReturned(
                    mocked,
                    token,
                    method,
                    args,
                    null,
                    source);
                return true;
            }

            MockReceiverFreeSetup? receiverFreeSetup =
                mocked.ReceiverFree is null
                    ? null
                    : matcherEvaluation?
                        .SelectedReceiverFreeSetup ??
                    mocked.ReceiverFreeSetups!.Find(
                        mocked.ReceiverFree,
                        entryArguments);
            if (receiverFreeSetup is not null)
            {
                failureStage = MockInvocationFailureStage.Behavior;
                return MockReceiverFreeExecution.Execute(
                    receiverFreeSetup,
                    instance,
                    mocked,
                    method,
                    args,
                    token,
                    ref source,
                    matcherEvaluation,
                    out result,
                    out continuation);
            }

            var behavior = matcherEvaluation is null
                ? mocked.FindBehavior(method, entryArguments)
                : matcherEvaluation.SelectedBehavior;
            if (behavior is not null)
            {
                failureStage = MockInvocationFailureStage.Behavior;
                MockBehaviorExecution execution = behavior.Claim();
                if (execution.Kind ==
                    MockBehaviorExecutionKind.Passthrough)
                {
                    continuation = new(
                        mocked,
                        token,
                        method,
                        args,
                        matcherEvaluation?.SelectedSetup);
                    return false;
                }
                if (execution.Kind ==
                    MockBehaviorExecutionKind.Strict)
                {
                    source =
                        MockInvocationExecutionSource.StrictFailure;
                    throw new MockException(
                        MockDiagnostics.UnexpectedInvocation(
                            mocked,
                            method,
                            entryArguments));
                }
                if (execution.Kind is
                    MockBehaviorExecutionKind.TypedCallback or
                    MockBehaviorExecutionKind.TypedReturnFactory or
                    MockBehaviorExecutionKind.TypedRefReturnFactory)
                {
                    continuation = new MockDispatchContinuation(
                        mocked,
                        token,
                        method,
                        execution.Callback!,
                        execution.Kind,
                        matcherEvaluation?.SelectedSetup,
                        args);
                    return true;
                }

                MockBehaviorClaimExecution.Execute(
                    execution,
                    instance,
                    mocked,
                    method,
                    args,
                    out result);
                if (matcherEvaluation?.HasExitProjectors == true)
                    return true;

                result = MockInvocationCapture.CompleteReturned(
                    mocked,
                    token,
                    method,
                    args,
                    result,
                    source,
                    null,
                    observeAsync:
                        execution.Kind ==
                        MockBehaviorExecutionKind.Callback);
                return true;
            }

            if (mocked.Fallback == MockFallbackBehavior.Partial)
            {
                continuation = new MockDispatchContinuation(
                    mocked,
                    token,
                    method,
                    args);
                return false;
            }

            if (mocked.Fallback == MockFallbackBehavior.Strict)
            {
                source = MockInvocationExecutionSource.StrictFailure;
                failureStage = MockInvocationFailureStage.Behavior;
                throw new MockException(
                    MockDiagnostics.UnexpectedInvocation(
                        mocked,
                        method,
                        entryArguments));
            }

            source = MockInvocationExecutionSource.LooseFallback;
            result = mocked.GetDefault(method);
            result = MockInvocationCapture.CompleteReturned(
                mocked,
                token,
                method,
                args,
                result,
                source);
            return true;
        }
        catch (Exception exception)
        {
            MockInvocationCapture.CompleteThrown(
                mocked,
                token,
                source,
                exception,
                failureStage);
            throw;
        }
    }

}
