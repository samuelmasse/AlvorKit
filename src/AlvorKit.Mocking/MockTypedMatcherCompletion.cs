namespace AlvorKit;

/// <summary>Completes projected typed matcher invocations through the shared capture path.</summary>
internal static class MockTypedMatcherCompletion
{
    /// <summary>Completes a returned invocation when exit projection requires final arguments.</summary>
    internal static object? CompleteReturned(
        Mocked mocked,
        MockInvocationToken token,
        MethodInfo method,
        object?[] arguments,
        object? result,
        bool hasExitProjectors,
        MockSetup? setup)
    {
        if (!hasExitProjectors)
            return result;

        return MockInvocationCapture.CompleteReturned(
            mocked,
            token,
            method,
            arguments,
            result,
            MockInvocationExecutionSource.Configured,
            setup,
            observeAsync: setup?.Behavior is MockCallbackBehavior);
    }
}
