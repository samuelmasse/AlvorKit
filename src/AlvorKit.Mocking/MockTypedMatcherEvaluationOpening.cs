namespace AlvorKit.Mocking;

/// <summary>Opens typed matcher evaluations from immutable setup generations.</summary>
internal static class MockTypedMatcherEvaluationOpening
{
    /// <summary>Opens an evaluation when the captured method needs typed execution.</summary>
    internal static MockTypedMatcherEvaluation? Open(
        Mocked mocked,
        MethodInfo method,
        object?[] arguments,
        string backend)
    {
        if (Capture.Context.IsActive ||
            !mocked.HasTypedExecution(method))
        {
            return null;
        }

        MockInvocationToken token = MockInvocationCapture.Open(
            mocked,
            method,
            arguments,
            backend);
        MockSetup[] candidates = mocked.SnapshotSetups();
        var matches = new bool[candidates.Length];
        MockReceiverFreeSetup[] receiverFreeCandidates =
            mocked.SnapshotReceiverFreeSetups();
        var receiverFreeMatches =
            new bool[receiverFreeCandidates.Length];
        var evaluation = new MockTypedMatcherEvaluation(
            mocked,
            method,
            token,
            candidates,
            matches,
            receiverFreeCandidates,
            receiverFreeMatches);

        try
        {
            for (var index = 0; index < candidates.Length; index++)
            {
                matches[index] =
                    candidates[index].MatchesHeapSafe(
                        method,
                        arguments,
                        mocked.ReceiverFree);
            }
            if (mocked.ReceiverFree is { } identity)
            {
                for (var index = 0;
                    index < receiverFreeCandidates.Length;
                    index++)
                {
                    receiverFreeMatches[index] =
                        receiverFreeCandidates[index]
                            .MatchesHeapSafe(
                                identity,
                                arguments);
                }
            }

            return evaluation;
        }
        catch (Exception exception)
        {
            evaluation.Fail(
                exception,
                MockInvocationFailureStage.Matcher);
            throw;
        }
    }
}
