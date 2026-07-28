namespace AlvorKit.Mocking;

/// <summary>
/// Binds a method and captured argument pattern to one configured behavior.
/// </summary>
internal sealed class MockSetup
{
    private readonly MockArgumentPattern[] arguments;
    private readonly MockStructThisMutation[] structMutations;

    /// <summary>Creates one immutable setup.</summary>
    internal MockSetup(
        MethodInfo method,
        ReadOnlySpan<MockArgumentPattern> arguments,
        MockConfiguredBehavior behavior)
        : this(method, arguments, behavior, [])
    {
    }

    /// <summary>Creates one immutable setup with typed history projectors.</summary>
    internal MockSetup(
        MethodInfo method,
        ReadOnlySpan<MockArgumentPattern> arguments,
        MockConfiguredBehavior behavior,
        ReadOnlySpan<MockSnapshotProjector> projectors)
        : this(
            method,
            arguments,
            behavior,
            projectors,
            [],
            null)
    {
    }

    /// <summary>
    /// Creates one setup with optional struct hooks and interception-site scope.
    /// </summary>
    internal MockSetup(
        MethodInfo method,
        ReadOnlySpan<MockArgumentPattern> arguments,
        MockConfiguredBehavior behavior,
        ReadOnlySpan<MockSnapshotProjector> projectors,
        ReadOnlySpan<MockStructThisMutation> structMutations,
        MockCallSite? site)
    {
        Method = method;
        this.arguments = arguments.ToArray();
        Behavior = behavior;
        Projectors = projectors.ToArray();
        this.structMutations = structMutations.ToArray();
        Site = site;
    }

    /// <summary>Gets the configured method.</summary>
    internal MethodInfo Method { get; }

    /// <summary>Gets the configured behavior.</summary>
    internal MockConfiguredBehavior Behavior { get; }

    /// <summary>Gets immutable typed history projectors for this setup.</summary>
    internal MockSnapshotProjector[] Projectors { get; }

    /// <summary>Gets an optional exact interception-site scope.</summary>
    internal MockCallSite? Site { get; }

    /// <summary>Returns whether this setup matches the active invocation.</summary>
    internal bool Matches(
        MethodInfo method,
        ReadOnlySpan<object?> actual,
        MockReceiverFreeIdentity? identity = null)
    {
        if (!MatchesOperation(method, actual.Length, identity))
            return false;

        for (var i = 0; i < arguments.Length; i++)
        {
            if (!arguments[i].Matches(actual[i]))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Returns whether heap-safe positions match while deferring live typed
    /// predicates to the generated data plane.
    /// </summary>
    internal bool MatchesHeapSafe(
        MethodInfo method,
        ReadOnlySpan<object?> actual,
        MockReceiverFreeIdentity? identity = null)
    {
        if (!MatchesOperation(method, actual.Length, identity))
            return false;

        for (var index = 0; index < arguments.Length; index++)
        {
            if (!arguments[index].MatchesHeapSafe(actual[index]))
                return false;
        }

        return true;
    }

    /// <summary>Evaluates one declared live position for this candidate.</summary>
    internal bool Matches<T>(
        int declaredIndex,
        scoped in T actual)
        where T : allows ref struct =>
        arguments[declaredIndex].MatchesDeferred(in actual);

    /// <summary>Gets whether any pattern requires live typed evaluation.</summary>
    internal bool RequiresTypedEvaluation
    {
        get
        {
            foreach (MockArgumentPattern argument in arguments)
            {
                if (argument.RequiresTypedEvaluation)
                    return true;
            }

            return false;
        }
    }

    /// <summary>Gets whether generated typed execution is required.</summary>
    internal bool RequiresTypedExecution =>
        Projectors.Length != 0 ||
        structMutations.Length != 0 ||
        RequiresTypedEvaluation;

    /// <summary>Runs registered live-struct mutations for one phase.</summary>
    internal bool MutateStructThis<T>(
        int declaredIndex,
        MockSnapshotPhase phase,
        scoped ref T value)
        where T : struct
    {
        if (declaredIndex != 0)
            return false;

        bool mutated = false;
        foreach (MockStructThisMutation mutation in structMutations)
        {
            if (mutation.Phase != phase)
                continue;

            ((MockStructMutation<T>)mutation.Mutation)(ref value);
            mutated = true;
        }

        return mutated;
    }

    /// <summary>Gets whether one declared argument has an explicit projector.</summary>
    internal bool HasProjector(
        int declaredIndex,
        MockSnapshotPhase phase)
    {
        foreach (MockSnapshotProjector projector in Projectors)
        {
            if (projector.DeclaredIndex == declaredIndex &&
                projector.Phase == phase)
            {
                return true;
            }
        }

        return false;
    }

    private bool MatchesOperation(
        MethodInfo method,
        int argumentCount,
        MockReceiverFreeIdentity? identity)
    {
        if (Method != method ||
            arguments.Length != argumentCount)
        {
            return false;
        }
        if (Site is null)
            return true;

        return identity is not null &&
            identity.Site == Site.Descriptor;
    }
}
