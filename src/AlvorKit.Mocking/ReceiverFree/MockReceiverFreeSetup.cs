namespace AlvorKit.Mocking;

/// <summary>Owns one immutable receiver-free match and its behavior state.</summary>
internal sealed class MockReceiverFreeSetup
{
    private readonly MockArgumentPattern[] patterns;
    private readonly MockConfiguredBehavior? configured;
    private readonly ConcurrentDictionary<MethodInfo, Delegate>
        normalizedCallbacks = [];

    /// <summary>Creates one setup, including any setup-local sequence cursor.</summary>
    internal MockReceiverFreeSetup(
        MockReceiverFreeSetupDescriptor descriptor,
        MockReceiverFreeBehavior behavior)
    {
        Descriptor = descriptor;
        Behavior = behavior;
        patterns = descriptor.Patterns.ToArray();
        configured = behavior.Kind switch
        {
            MockReceiverFreeBehaviorKind.Return or
            MockReceiverFreeBehaviorKind.Substitute =>
                new MockConstantBehavior(behavior.Value, []),
            MockReceiverFreeBehaviorKind.ReturnSequence =>
                new MockReturnSequenceBehavior(
                    (object?[])behavior.Value!),
            MockReceiverFreeBehaviorKind.Throw =>
                new MockThrowBehavior(behavior.Exception!),
            _ => null
        };
    }

    /// <summary>Gets the member, receiver, and optional site scope.</summary>
    internal MockReceiverFreeSetupDescriptor Descriptor { get; }

    /// <summary>Gets the receiver-free behavior contract.</summary>
    internal MockReceiverFreeBehavior Behavior { get; }

    /// <summary>Claims ordinary return, sequence, or throw state.</summary>
    internal MockBehaviorExecution? ClaimConfigured() =>
        configured?.Claim();

    /// <summary>
    /// Returns an exact normalized callback for the current logical method.
    /// </summary>
    internal Delegate GetNormalizedCallback(MethodInfo logicalMethod)
    {
        Delegate callback = Behavior.Callback ??
            throw new InvalidOperationException(
                "The receiver-free behavior has no callback.");
        return normalizedCallbacks.GetOrAdd(
            logicalMethod,
            _ => MockTypedCallbackContract.Normalize(
                callback,
                logicalMethod));
    }

    /// <summary>
    /// Returns an exact constructor callback, adapting receiver-only public
    /// callbacks without observing constructor arguments.
    /// </summary>
    internal Delegate GetNormalizedConstructorCallback(
        MethodInfo logicalMethod)
    {
        Delegate callback = Behavior.Callback ??
            throw new InvalidOperationException(
                "The constructor behavior has no callback.");
        return normalizedCallbacks.GetOrAdd(
            logicalMethod,
            _ => MockRuntimeBackendRegistry.Proxy
                .NormalizeConstructorCallback(
                    callback,
                    logicalMethod));
    }

    /// <summary>Returns whether this setup matches one active site and carrier.</summary>
    internal bool Matches(
        MockReceiverFreeIdentity identity,
        ReadOnlySpan<object?> arguments)
        => Matches(
            identity,
            arguments,
            static (pattern, actual) =>
                pattern.Matches(actual));

    /// <summary>
    /// Matches retained positions while deferring live typed predicates.
    /// </summary>
    internal bool MatchesHeapSafe(
        MockReceiverFreeIdentity identity,
        ReadOnlySpan<object?> arguments)
        => Matches(
            identity,
            arguments,
            static (pattern, actual) =>
                pattern.MatchesHeapSafe(actual));

    /// <summary>Evaluates one deferred live argument matcher.</summary>
    internal bool MatchesTyped<T>(
        int declaredIndex,
        scoped in T value)
        where T : allows ref struct
    {
        int patternIndex = declaredIndex -
            DeclaredPatternOffset(Descriptor.OperationKind);
        return patternIndex < 0 ||
            patternIndex >= patterns.Length ||
            patterns[patternIndex].MatchesDeferred(in value);
    }

    /// <summary>Gets whether typed matchers or projectors need the live frame.</summary>
    internal bool RequiresTypedExecution
    {
        get
        {
            if (Behavior.Projectors.Length != 0)
                return true;
            foreach (MockArgumentPattern pattern in patterns)
            {
                if (pattern.RequiresTypedEvaluation)
                    return true;
            }

            return false;
        }
    }

    private bool Matches(
        MockReceiverFreeIdentity identity,
        ReadOnlySpan<object?> arguments,
        Func<MockArgumentPattern, object?, bool> match)
    {
        if (!Equals(Descriptor.Operation, identity.Operation) ||
            Descriptor.OperationKind != identity.Site.OperationKind)
        {
            return false;
        }
        if (Descriptor.Site is not null &&
            Descriptor.Site.Descriptor != identity.Site)
        {
            return false;
        }

        int argumentOffset =
            DeclaredPatternOffset(identity.Site.OperationKind);
        if (identity.Site.OperationKind ==
            MockInvocationOperationKind.ConstructorBody)
        {
            if (arguments.Length == 0)
                return false;
        }
        else if (identity.Operation is FieldInfo { IsStatic: false })
        {
            if (arguments.Length == 0 ||
                !ReferenceEquals(
                    Descriptor.Receiver,
                    arguments[0]))
            {
                return false;
            }

            argumentOffset = 1;
        }

        if (patterns.Length != arguments.Length - argumentOffset)
            return false;
        for (int index = 0; index < patterns.Length; index++)
        {
            if (!match(
                patterns[index],
                arguments[index + argumentOffset]))
            {
                return false;
            }
        }

        return true;
    }

    private static int DeclaredPatternOffset(
        MockInvocationOperationKind operationKind) =>
        operationKind ==
            MockInvocationOperationKind.ConstructorBody
            ? 1
            : 0;
}
