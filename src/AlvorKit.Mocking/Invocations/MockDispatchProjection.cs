namespace AlvorKit;

/// <summary>Projects and mutates live values carried by a dispatch continuation.</summary>
internal static class MockDispatchProjection
{
    /// <summary>Projects one live original-path argument through the selected setup.</summary>
    internal static void Project<T>(
        this MockDispatchContinuation continuation,
        int declaredIndex,
        MockSnapshotPhase phase,
        scoped in T value)
        where T : allows ref struct
    {
        ReadOnlySpan<MockSnapshotProjector> projectors =
            continuation.SelectedProjectors();
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

                object? projected =
                    ((MockSnapshotProjector<T>)projector)
                    .Project(in value);
                continuation.Mocked.Invocations.PublishProjection(
                    continuation.Token,
                    MockInvocationArgumentSnapshot.Projected(
                        declaredIndex,
                        projector.DeclaredType,
                        phase,
                        projected));
            }
        }
        catch (Exception exception)
        {
            MockInvocationCapture.CompleteThrown(
                continuation.Mocked,
                continuation.Token,
                MockInvocationExecutionSource.Configured,
                exception,
                phase == MockSnapshotPhase.Entry
                    ? MockInvocationFailureStage.EntryProjector
                    : MockInvocationFailureStage.ExitProjector);
            throw;
        }
    }

    /// <summary>Runs selected original-path live receiver mutations.</summary>
    internal static bool MutateStructThis<T>(
        this MockDispatchContinuation continuation,
        int declaredIndex,
        MockSnapshotPhase phase,
        scoped ref T value)
        where T : struct
    {
        if (continuation.ProjectedSetup is not { } setup)
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
            MockInvocationCapture.CompleteThrown(
                continuation.Mocked,
                continuation.Token,
                MockInvocationExecutionSource.Configured,
                exception,
                phase == MockSnapshotPhase.Entry
                    ? MockInvocationFailureStage.EntryMutation
                    : MockInvocationFailureStage.ExitMutation);
            throw;
        }
    }

    /// <summary>Gets immutable projectors selected for this continuation.</summary>
    internal static ReadOnlySpan<MockSnapshotProjector> SelectedProjectors(
        this MockDispatchContinuation continuation) =>
        continuation.ProjectedReceiverFreeSetup is { } receiverFree
            ? receiverFree.Behavior.Projectors
            : continuation.ProjectedSetup?.Projectors ?? [];

    /// <summary>Gets whether the selected setup contains one exact projector.</summary>
    internal static bool HasProjector(
        this MockDispatchContinuation continuation,
        int declaredIndex,
        MockSnapshotPhase phase)
    {
        foreach (MockSnapshotProjector projector in
            continuation.SelectedProjectors())
        {
            if (projector.DeclaredIndex == declaredIndex &&
                projector.Phase == phase)
            {
                return true;
            }
        }

        return false;
    }
}
