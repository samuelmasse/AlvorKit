namespace AlvorKit.Mocking;

/// <summary>
/// Owns one invocation token and immutable candidate evaluation while exact
/// typed values remain on the intercepted stack.
/// </summary>
internal sealed class MockTypedMatcherEvaluation
{
    private readonly Mocked mocked;
    private readonly MethodInfo method;
    private readonly MockSetup[] candidates;
    private readonly bool[] matches;
    private readonly MockReceiverFreeSetup[] receiverFreeCandidates;
    private readonly bool[] receiverFreeMatches;
    private bool failed;

    internal MockTypedMatcherEvaluation(
        Mocked mocked,
        MethodInfo method,
        MockInvocationToken token,
        MockSetup[] candidates,
        bool[] matches,
        MockReceiverFreeSetup[] receiverFreeCandidates,
        bool[] receiverFreeMatches)
    {
        this.mocked = mocked;
        this.method = method;
        Token = token;
        this.candidates = candidates;
        this.matches = matches;
        this.receiverFreeCandidates = receiverFreeCandidates;
        this.receiverFreeMatches = receiverFreeMatches;
    }

    /// <summary>Gets the already-open invocation token.</summary>
    internal MockInvocationToken Token { get; }

    /// <summary>Opens one typed evaluation with its immutable backend label.</summary>
    internal static MockTypedMatcherEvaluation? Open(
        Mocked mocked,
        MethodInfo method,
        object?[] arguments,
        string backend) =>
        MockTypedMatcherEvaluationOpening.Open(
            mocked,
            method,
            arguments,
            backend);

    /// <summary>Evaluates one declared live input against every viable candidate.</summary>
    internal void Match<T>(
        int declaredIndex,
        scoped in T value)
        where T : allows ref struct
    {
        try
        {
            for (var index = 0; index < candidates.Length; index++)
            {
                if (matches[index] &&
                    !candidates[index].Matches(
                        declaredIndex,
                        in value))
                {
                    matches[index] = false;
                }
            }
            for (int index = 0;
                index < receiverFreeCandidates.Length;
                index++)
            {
                if (receiverFreeMatches[index] &&
                    !receiverFreeCandidates[index].MatchesTyped(
                        declaredIndex,
                        in value))
                {
                    receiverFreeMatches[index] = false;
                }
            }
        }
        catch (Exception exception)
        {
            Fail(
                exception,
                MockInvocationFailureStage.Matcher);
            throw;
        }
    }

    /// <summary>Projects one selected setup argument without boxing its live value.</summary>
    internal void Project<T>(
        int declaredIndex,
        MockSnapshotPhase phase,
        scoped in T value)
        where T : allows ref struct
    {
        ReadOnlySpan<MockSnapshotProjector> projectors =
            SelectedProjectors;
        if (projectors.Length == 0)
            return;

        try
        {
            foreach (MockSnapshotProjector projector in projectors)
            {
                if (projector.DeclaredIndex != declaredIndex ||
                    projector.Phase != phase)
                {
                    continue;
                }

                var typed = (MockSnapshotProjector<T>)projector;
                object? projected = typed.Project(in value);
                mocked.Invocations.PublishProjection(
                    Token,
                    MockInvocationArgumentSnapshot.Projected(
                        declaredIndex,
                        projector.DeclaredType,
                        phase,
                        projected));
            }
        }
        catch (Exception exception)
        {
            Fail(
                exception,
                phase == MockSnapshotPhase.Entry
                    ? MockInvocationFailureStage.EntryProjector
                    : MockInvocationFailureStage.ExitProjector);
            throw;
        }
    }

    /// <summary>
    /// Runs selected synchronous struct mutations against live receiver
    /// storage and reports whether it changed.
    /// </summary>
    internal bool MutateStructThis<T>(
        int declaredIndex,
        MockSnapshotPhase phase,
        scoped ref T value)
        where T : struct
    {
        MockSetup? setup = SelectedSetup;
        if (setup is null)
            return false;

        try
        {
            return setup.MutateStructThis(
                declaredIndex,
                phase,
                ref value);
        }
        catch (Exception exception)
        {
            Fail(
                exception,
                phase == MockSnapshotPhase.Entry
                    ? MockInvocationFailureStage.EntryMutation
                    : MockInvocationFailureStage.ExitMutation);
            throw;
        }
    }

    /// <summary>Gets the newest fully matching immutable setup.</summary>
    internal MockSetup? SelectedSetup
    {
        get
        {
            for (var index = 0; index < candidates.Length; index++)
            {
                if (matches[index])
                    return candidates[index];
            }

            return null;
        }
    }

    /// <summary>Gets the selected site-specific or member-wide receiver-free setup.</summary>
    internal MockReceiverFreeSetup? SelectedReceiverFreeSetup
    {
        get
        {
            for (int index = 0;
                index < receiverFreeCandidates.Length;
                index++)
            {
                if (receiverFreeMatches[index] &&
                    receiverFreeCandidates[index].Descriptor.Site is not null)
                {
                    return receiverFreeCandidates[index];
                }
            }
            for (int index = 0;
                index < receiverFreeCandidates.Length;
                index++)
            {
                if (receiverFreeMatches[index])
                    return receiverFreeCandidates[index];
            }

            return null;
        }
    }

    /// <summary>Gets the newest fully matching behavior without invoking it.</summary>
    internal MockConfiguredBehavior? SelectedBehavior =>
        SelectedSetup?.Behavior;

    /// <summary>Gets whether the selected setup requires final-slot projection.</summary>
    internal bool HasExitProjectors =>
        MockTypedMatcherProjectors.HasExit(SelectedProjectors);

    /// <summary>Completes an ordinary configured behavior after its projected exits.</summary>
    internal object? CompleteReturned(
        object?[] arguments,
        object? result) =>
        MockTypedMatcherCompletion.CompleteReturned(
            mocked,
            Token,
            method,
            arguments,
            result,
            HasExitProjectors,
            SelectedSetup);

    private ReadOnlySpan<MockSnapshotProjector> SelectedProjectors =>
        SelectedReceiverFreeSetup is { } receiverFree
            ? receiverFree.Behavior.Projectors
            : SelectedSetup?.Projectors ?? [];

    /// <summary>Completes this evaluation once with the reported failure stage.</summary>
    internal void Fail(
        Exception exception,
        MockInvocationFailureStage failureStage)
    {
        if (failed)
            return;

        failed = true;
        MockInvocationCapture.CompleteThrown(
            mocked,
            Token,
            MockInvocationExecutionSource.Configured,
            exception,
            failureStage);
    }
}
