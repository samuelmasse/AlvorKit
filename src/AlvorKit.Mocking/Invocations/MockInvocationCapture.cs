namespace AlvorKit.Mocking;

/// <summary>Maps the heap-safe dispatch carrier into declared-order history.</summary>
internal static class MockInvocationCapture
{
    /// <summary>Opens one invocation with an immutable instrumentation label.</summary>
    internal static MockInvocationToken Open(
        Mocked mocked,
        MethodInfo method,
        ReadOnlySpan<object?> arguments,
        string backend)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(backend);
        ParameterInfo[] parameters = mocked.Type.GetParameters(method);
        var carrierIndices = Indices.ParameterIndices(mocked.Type, method);
        var snapshots = new MockInvocationArgumentSnapshot[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            var declaredType = parameter.ParameterType;
            var valueType = declaredType.IsByRef
                ? declaredType.GetElementType()!
                : declaredType;

            if (parameter.IsOut)
            {
                snapshots[i] = Unavailable(
                    i,
                    declaredType,
                    MockSnapshotPhase.Entry,
                    MockUnavailableReason.OutHasNoEntryValue);
            }
            else if (valueType.IsByRefLike)
            {
                snapshots[i] = Unavailable(
                    i,
                    declaredType,
                    MockSnapshotPhase.Entry,
                    MockUnavailableReason.ByRefLikeProjectionNotConfigured);
            }
            else
            {
                snapshots[i] = MockInvocationArgumentSnapshot.Shallow(
                    i,
                    declaredType,
                    MockSnapshotPhase.Entry,
                    arguments[carrierIndices[i]]);
            }
        }

        MockReceiverFreeIdentity? receiverFree = mocked.ReceiverFree;
        MockInvocationTarget target = receiverFree is null
            ? MockInvocationTarget.ForMock(
                mocked.Invocations.Id,
                mocked.Type.Type)
            : MockInvocationTarget.ForCallSite(
                receiverFree.SessionId,
                mocked.Type.Type,
                receiverFree.Site.ModuleVersionId,
                receiverFree.Site.ContainingMethodToken,
                receiverFree.Site.OriginalIlOffset,
                receiverFree.Site.OperationKind);
        var identity = new MockInvocationIdentity(
            target,
            receiverFree?.Operation ?? method,
            backend);

        var session = MockSession.Current;
        if (session is null)
            return mocked.Invocations.OpenOwned(
                identity,
                snapshots,
                parameters,
                mocked.Invocations.Timeline);

        session.Register(mocked);
        return mocked.Invocations.OpenOwned(
            identity,
            snapshots,
            parameters,
            session.Timeline);
    }

    /// <summary>Publishes ordinary reference exits and completes a normal call.</summary>
    internal static object? CompleteReturned(
        Mocked mocked,
        MockInvocationToken token,
        MethodInfo method,
        ReadOnlySpan<object?> arguments,
        object? result,
        MockInvocationExecutionSource source) =>
        CompleteReturned(
            mocked,
            token,
            method,
            arguments,
            result,
            source,
            null,
            observeAsync: false);

    /// <summary>Completes a call without replacing explicit exit projections.</summary>
    internal static object? CompleteReturned(
        Mocked mocked,
        MockInvocationToken token,
        MethodInfo method,
        ReadOnlySpan<object?> arguments,
        object? result,
        MockInvocationExecutionSource source,
        MockSetup? projectedSetup,
        bool observeAsync = false)
    {
        MockAsyncReturn? asyncReturn = observeAsync
            ? MockAsyncReturn.Prepare(method.ReturnType, result)
            : null;
        object? normalizedResult =
            asyncReturn?.ReturnValue ?? result;
        ParameterInfo[] parameters = mocked.Type.GetParameters(method);
        var carrierIndices = Indices.ParameterIndices(mocked.Type, method);

        for (var i = 0; i < parameters.Length; i++)
        {
            var declaredType = parameters[i].ParameterType;
            if (!declaredType.IsByRef ||
                declaredType.GetElementType()!.IsByRefLike ||
                projectedSetup?.HasProjector(
                    i,
                    MockSnapshotPhase.Exit) == true)
            {
                continue;
            }

            mocked.Invocations.PublishProjection(
                token,
                MockInvocationArgumentSnapshot.Shallow(
                    i,
                    declaredType,
                    MockSnapshotPhase.Exit,
                    arguments[carrierIndices[i]]));
        }

        mocked.Invocations.CompleteReturned(
            token,
            source,
            CreateReturn(method.ReturnType, normalizedResult));
        asyncReturn?.Observe(token.Slot);
        return normalizedResult;
    }

    /// <summary>Completes a call with the exact observed exception.</summary>
    internal static void CompleteThrown(
        Mocked mocked,
        MockInvocationToken token,
        MockInvocationExecutionSource source,
        Exception exception,
        MockInvocationFailureStage failureStage) =>
        mocked.Invocations.CompleteThrown(
            token,
            source,
            exception,
            failureStage);

    private static MockInvocationReturn CreateReturn(
        Type returnType,
        object? result)
    {
        if (returnType == typeof(void))
            return MockInvocationReturn.Void();

        return returnType.IsByRef || returnType.IsByRefLike
            ? MockInvocationReturn.Unavailable(returnType)
            : MockInvocationReturn.Shallow(returnType, result);
    }

    private static MockInvocationArgumentSnapshot Unavailable(
        int declaredIndex,
        Type declaredType,
        MockSnapshotPhase phase,
        MockUnavailableReason reason) =>
        MockInvocationArgumentSnapshot.UnavailableValue(
            new(declaredIndex, declaredType, phase, reason));
}
