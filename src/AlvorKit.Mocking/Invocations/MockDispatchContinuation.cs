namespace AlvorKit.Mocking;

/// <summary>
/// Carries one already-open invocation through exact typed factory or original completion.
/// </summary>
internal sealed class MockDispatchContinuation
{
    private readonly Mocked mocked;
    private readonly MockInvocationToken token;
    private readonly MethodInfo method;
    private readonly Delegate? typedReturnFactory;
    private readonly MockBehaviorExecutionKind executionKind;
    private readonly MockSetup? projectedSetup;
    private readonly MockReceiverFreeSetup? projectedReceiverFreeSetup;
    private readonly object?[]? originalArguments;
    private readonly MockReceiverFreeBehaviorKind? receiverFreeBehaviorKind;

    /// <summary>
    /// Creates a continuation for the exact invocation opened by control-plane selection.
    /// </summary>
    internal MockDispatchContinuation(
        Mocked mocked,
        MockInvocationToken token,
        MethodInfo method,
        object?[]? originalArguments = null,
        MockSetup? projectedSetup = null,
        MockReceiverFreeSetup? projectedReceiverFreeSetup = null)
    {
        this.mocked = mocked;
        this.token = token;
        this.method = method;
        this.originalArguments = originalArguments;
        this.projectedSetup = projectedSetup;
        this.projectedReceiverFreeSetup =
            projectedReceiverFreeSetup;
        executionKind = MockBehaviorExecutionKind.Return;
    }

    /// <summary>Creates a pending exact typed return-factory execution.</summary>
    internal MockDispatchContinuation(
        Mocked mocked,
        MockInvocationToken token,
        MethodInfo method,
        Delegate typedReturnFactory,
        MockBehaviorExecutionKind executionKind =
            MockBehaviorExecutionKind.TypedReturnFactory,
        MockSetup? projectedSetup = null,
        object?[]? originalArguments = null)
        : this(mocked, token, method, originalArguments)
    {
        ArgumentNullException.ThrowIfNull(typedReturnFactory);
        this.typedReturnFactory = typedReturnFactory;
        this.executionKind = executionKind;
        this.projectedSetup = projectedSetup;
    }

    /// <summary>Creates a pending exact field observer or transformer.</summary>
    internal MockDispatchContinuation(
        Mocked mocked,
        MockInvocationToken token,
        MethodInfo method,
        Delegate callback,
        MockReceiverFreeBehaviorKind behaviorKind,
        object?[] originalArguments)
        : this(
            mocked,
            token,
            method,
            callback,
            MockBehaviorExecutionKind.ReceiverFreeFieldBehavior,
            originalArguments: originalArguments)
    {
        if (behaviorKind is not (
            MockReceiverFreeBehaviorKind.Observe or
            MockReceiverFreeBehaviorKind.Transform))
        {
            throw new ArgumentOutOfRangeException(
                nameof(behaviorKind));
        }

        receiverFreeBehaviorKind = behaviorKind;
    }

    /// <summary>Creates a pending constructor observer or replacement.</summary>
    internal MockDispatchContinuation(
        Mocked mocked,
        MockInvocationToken token,
        MethodInfo method,
        Delegate callback,
        MockReceiverFreeBehaviorKind behaviorKind,
        object?[] originalArguments,
        bool constructorBody)
        : this(
            mocked,
            token,
            method,
            callback,
            MockBehaviorExecutionKind.ReceiverFreeConstructorBehavior,
            originalArguments: originalArguments)
    {
        if (!constructorBody ||
            behaviorKind is not (
                MockReceiverFreeBehaviorKind.Observe or
                MockReceiverFreeBehaviorKind.Replace))
        {
            throw new ArgumentOutOfRangeException(
                nameof(behaviorKind));
        }

        receiverFreeBehaviorKind = behaviorKind;
    }

    /// <summary>Gets whether generated code must invoke an exact return factory.</summary>
    internal bool IsTypedReturnFactory =>
        executionKind == MockBehaviorExecutionKind.TypedReturnFactory;

    /// <summary>Gets whether generated code must invoke an exact typed callback.</summary>
    internal bool IsTypedCallback =>
        executionKind == MockBehaviorExecutionKind.TypedCallback;

    /// <summary>Gets whether generated code must publish an exact managed-reference factory.</summary>
    internal bool IsTypedRefReturnFactory =>
        executionKind == MockBehaviorExecutionKind.TypedRefReturnFactory;

    /// <summary>Gets whether generated code must run a field observer or transformer.</summary>
    internal bool IsReceiverFreeFieldBehavior =>
        executionKind ==
        MockBehaviorExecutionKind.ReceiverFreeFieldBehavior;

    /// <summary>Gets the selected field behavior kind.</summary>
    internal MockReceiverFreeBehaviorKind ReceiverFreeFieldBehaviorKind =>
        receiverFreeBehaviorKind ??
        throw new InvalidOperationException(
            "The continuation does not carry a field behavior.");

    /// <summary>Gets the exact field observer or transformer.</summary>
    internal Delegate ReceiverFreeFieldCallback =>
        IsReceiverFreeFieldBehavior
            ? typedReturnFactory!
            : throw new InvalidOperationException(
                "The continuation does not carry a field callback.");

    /// <summary>Gets whether generated code must run a constructor callback.</summary>
    internal bool IsReceiverFreeConstructorBehavior =>
        executionKind ==
        MockBehaviorExecutionKind.ReceiverFreeConstructorBehavior;

    /// <summary>Gets whether the constructor remainder must be replaced.</summary>
    internal bool ReplacesReceiverFreeConstructorBody =>
        IsReceiverFreeConstructorBehavior &&
        receiverFreeBehaviorKind ==
            MockReceiverFreeBehaviorKind.Replace;

    /// <summary>Gets the exact constructor observer or replacement.</summary>
    internal Delegate ReceiverFreeConstructorCallback =>
        IsReceiverFreeConstructorBehavior
            ? typedReturnFactory!
            : throw new InvalidOperationException(
                "The continuation does not carry a constructor callback.");

    /// <summary>Gets the exact zero-argument factory selected for this invocation.</summary>
    internal Delegate TypedReturnFactory =>
        typedReturnFactory ??
        throw new InvalidOperationException(
            "The continuation does not carry a typed return factory.");

    /// <summary>Gets the exact callback selected for this invocation.</summary>
    internal Delegate TypedCallback =>
        IsTypedCallback
            ? typedReturnFactory!
            : throw new InvalidOperationException(
                "The continuation does not carry a typed callback.");

    /// <summary>Gets the exact stable managed-reference factory selected for this invocation.</summary>
    internal Delegate TypedRefReturnFactory =>
        IsTypedRefReturnFactory
            ? typedReturnFactory!
            : throw new InvalidOperationException(
                "The continuation does not carry a typed managed-reference factory.");

    /// <summary>Gets the entry carrier retained for a partial-original completion.</summary>
    internal object?[] OriginalArguments =>
        originalArguments ??
        throw new InvalidOperationException(
            "The continuation does not carry partial-original arguments.");

    /// <summary>Projects one live argument through the selected setup.</summary>
    internal void Project<T>(
        int declaredIndex,
        MockSnapshotPhase phase,
        scoped in T value)
        where T : allows ref struct =>
        MockDispatchProjection.Project(this, declaredIndex, phase, in value);
    /// <summary>Runs selected original-path receiver mutations.</summary>
    internal bool MutateStructThis<T>(
        int declaredIndex,
        MockSnapshotPhase phase,
        scoped ref T value)
        where T : struct =>
        MockDispatchProjection.MutateStructThis(
            this,
            declaredIndex,
            phase,
            ref value);
    /// <summary>Completes a normally returned original invocation.</summary>
    internal void CompleteReturned(object?[] arguments, object? result) =>
        MockDispatchCompletion.CompleteReturned(this, arguments, result);

    /// <summary>Completes a configured typed factory result.</summary>
    internal object? CompleteTypedReturned(
        object?[] arguments,
        object? result) =>
        MockDispatchCompletion.CompleteTypedReturned(this, arguments, result);
    /// <summary>Completes a typed result that cannot enter the control plane.</summary>
    internal void CompleteTypedUnretainedReturned(object?[] arguments) =>
        MockDispatchCompletion.CompleteTypedUnretainedReturned(this, arguments);
    /// <summary>Completes an original invocation with its exact exception.</summary>
    internal void CompleteThrown(Exception exception) =>
        MockDispatchCompletion.CompleteThrown(this, exception);
    /// <summary>Completes a field or constructor callback failure.</summary>
    internal void CompleteReceiverFreeBehaviorThrown(Exception exception) =>
        MockDispatchCompletion.CompleteReceiverFreeBehaviorThrown(this, exception);
    /// <summary>Completes a constructor replacement without its remainder.</summary>
    internal void CompleteReceiverFreeConstructorReplacement() =>
        MockDispatchCompletion.CompleteReceiverFreeConstructorReplacement(this);
    /// <summary>Completes a typed factory with its exact exception.</summary>
    internal void CompleteTypedThrown(Exception exception) =>
        MockDispatchCompletion.CompleteTypedThrown(this, exception);
    /// <summary>Completes a typed callback with its exact exception.</summary>
    internal void CompleteTypedCallbackThrown(Exception exception) =>
        MockDispatchCompletion.CompleteTypedCallbackThrown(this, exception);
    /// <summary>Gets the mock state owning this continuation.</summary>
    internal Mocked Mocked => mocked;

    /// <summary>Gets the already-open invocation token.</summary>
    internal MockInvocationToken Token => token;

    /// <summary>Gets the logical method being completed.</summary>
    internal MethodInfo Method => method;

    /// <summary>Gets the selected ordinary setup, when any.</summary>
    internal MockSetup? ProjectedSetup => projectedSetup;

    /// <summary>Gets the selected receiver-free setup, when any.</summary>
    internal MockReceiverFreeSetup? ProjectedReceiverFreeSetup =>
        projectedReceiverFreeSetup;

    /// <summary>Gets retained entry arguments when the continuation owns them.</summary>
    internal object?[]? RetainedArguments => originalArguments;
}
