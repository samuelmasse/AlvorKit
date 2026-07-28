namespace AlvorKit.LivePatch.Test;

[TestClass]
public sealed class LivePatchCollisionRegistryTest
{
    private static readonly InterceptionClaimConsumer Mocking =
        new("Mocking");

    /// <summary>Verifies disjoint LivePatch selectors share one method claim while overlap policy remains unchanged.</summary>
    [TestMethod]
    public void MethodClaim_ComposesDisjointSelectorsButNotOverlappingSelectors()
    {
        var registry = new InterceptionCollisionRegistry();
        var backend = new LivePatchFakeBackend(registry);
        var injector = new Injector();
        var graph = new InjectorScopeGraph(injector);
        var firstReceiver = new ScopedService();
        var secondReceiver = new ScopedService();
        var firstSelector = LivePatchSelector.ExactInstance(firstReceiver);
        var secondSelector = LivePatchSelector.ExactInstance(secondReceiver);
        using var session = new LivePatchSession(backend, graph);
        using var first = session.InstallReplace(
            Method<ScopedService>(nameof(ScopedService.Calculate)),
            firstSelector,
            new ScopedHandler(),
            Method<ScopedHandler>(nameof(ScopedHandler.Run)));
        using var second = session.InstallReplace(
            Method<ScopedService>(nameof(ScopedService.Calculate)),
            secondSelector,
            new ScopedHandler(),
            Method<ScopedHandler>(nameof(ScopedHandler.Run)));

        var exception = Assert.ThrowsExactly<InvalidOperationException>(
            () => session.InstallReplace(
                Method<ScopedService>(nameof(ScopedService.Calculate)),
                LivePatchSelector.ExactInstance(firstReceiver),
                new ScopedHandler(),
                Method<ScopedHandler>(nameof(ScopedHandler.Run))));

        Assert.AreEqual(1, backend.InstallCount);
        Assert.AreEqual(1, registry.Count);
        StringAssert.Contains(exception.Message, "explicit composition");
        var selectorDescription = registry
            .Snapshot()
            .Single()
            .Owner
            .Selector;
        StringAssert.Contains(
            selectorDescription,
            firstSelector.ToString());
        StringAssert.Contains(
            selectorDescription,
            secondSelector.ToString());

        first.Dispose();

        Assert.AreEqual(
            secondSelector.ToString(),
            registry.Snapshot().Single().Owner.Selector);
    }

    /// <summary>Verifies a LivePatch method-wide claim remains held until native retirement completes.</summary>
    [TestMethod]
    public void MethodClaim_IsReleasedAfterNativeRetirement()
    {
        var registry = new InterceptionCollisionRegistry();
        var backend = new LivePatchFakeBackend(registry);
        var injector = new Injector();
        var graph = new InjectorScopeGraph(injector);
        using var session = new LivePatchSession(backend, graph);
        using var lease = session.InstallReplace(
            Method<ScopedService>(nameof(ScopedService.Calculate)),
            LivePatchSelector.All(),
            new ScopedHandler(),
            Method<ScopedHandler>(nameof(ScopedHandler.Run)));

        Assert.AreEqual(1, registry.Count);
        var claim = registry.Snapshot().Single();
        Assert.AreEqual(
            InterceptionPhysicalRegion.MethodWide,
            claim.Region);
        Assert.AreEqual("LivePatch", claim.Owner.Consumer.Name);
        Assert.AreEqual("all", claim.Owner.Selector);

        backend.Patch!.CompleteActive();
        Assert.AreEqual(1, session.Pump());
        lease.Dispose();

        Assert.AreEqual(1, backend.Patch.RemoveCount);
        Assert.AreEqual(1, registry.Count);

        backend.Patch.CompleteRemoved();
        Assert.AreEqual(1, session.Pump());
        Assert.AreEqual(0, registry.Count);
    }

    /// <summary>Verifies a caller-site logical claim blocks LivePatch before native installation.</summary>
    [TestMethod]
    public void LogicalOperandCollision_RejectsLivePatchBeforeInstall()
    {
        var registry = new InterceptionCollisionRegistry();
        var backend = new LivePatchFakeBackend(registry);
        var injector = new Injector();
        var graph = new InjectorScopeGraph(injector);
        using var session = new LivePatchSession(backend, graph);
        var callee = InterceptionTarget.FromMethod(
            Method<ScopedService>(nameof(ScopedService.Calculate)));
        using var mocking = registry.Acquire(
            new(
                InterceptionTarget.FromMethod(
                    Method<CollisionCaller>(nameof(CollisionCaller.Run))),
                InterceptionPhysicalRegion.IlRange(0),
                new(Mocking, "site:calculate"),
                InterceptionLogicalOperand.ForMethod(callee)));

        var exception = Assert.ThrowsExactly<InterceptionCollisionException>(
            () => session.InstallReplace(
                Method<ScopedService>(nameof(ScopedService.Calculate)),
                LivePatchSelector.All(),
                new ScopedHandler(),
                Method<ScopedHandler>(nameof(ScopedHandler.Run))));

        Assert.AreEqual(
            InterceptionCollisionReason.LogicalOperand,
            exception.Collision.Reason);
        StringAssert.Contains(exception.Message, "LivePatch");
        StringAssert.Contains(exception.Message, "Mocking");
        Assert.AreEqual(0, backend.InstallCount);
        Assert.AreEqual(1, registry.Count);
    }

    private static MethodInfo Method<T>(string name) =>
        typeof(T).GetMethod(name)
        ?? throw new InvalidOperationException(
            $"Method '{typeof(T).FullName}.{name}' was not found.");
}

/// <summary>Ordinary caller used to give logical and physical collision claims different methods.</summary>
public sealed class CollisionCaller
{
    public int Run(
        ScopedService receiver,
        int value) =>
        receiver.Calculate(value);
}

internal sealed class LivePatchFakeBackend(
    InterceptionCollisionRegistry collisionRegistry)
    : IInterceptionBackend
{
    public InterceptionCapabilities Capabilities { get; } =
        new(
            InterceptionCapability.Rejit |
                InterceptionCapability.Revert |
                InterceptionCapability.ExactDispatch,
            1024,
            16,
            16);

    public InterceptionCollisionRegistry CollisionRegistry { get; } =
        collisionRegistry;

    internal int InstallCount { get; private set; }

    internal LivePatchFakePatchHandle? Patch { get; private set; }

    public IInterceptionPatchHandle Install(InterceptionPlan plan) =>
        throw new NotSupportedException();

    public IInterceptionPatchHandle Install(InterceptionDispatchPlan plan)
    {
        InstallCount++;
        Patch = new(
            1,
            plan.Target);
        return Patch;
    }

    public IInterceptionHandlerTrampoline CreateHandlerTrampoline(
        MethodInfo target,
        object? handlerInstance,
        MethodInfo handlerMethod,
        InterceptionHandlerExceptionPolicy exceptionPolicy)
    {
        _ = target;
        _ = handlerInstance;
        _ = handlerMethod;
        Assert.AreEqual(
            InterceptionHandlerExceptionPolicy.ContainAndDeactivate,
            exceptionPolicy);
        return new LivePatchFakeTrampoline();
    }

    public InterceptionBackendState GetState() =>
        new(true, false, 0, checked((uint)InstallCount), 0, 0);

    public InterceptionCompletion GetCompletion(ulong requestId)
    {
        if (Patch is null || Patch.LastRequestId != requestId)
            throw new KeyNotFoundException();
        return Patch.GetCompletion();
    }

    public InterceptionCompletion WaitFor(
        ulong requestId,
        TimeSpan timeout,
        TimeSpan? pollInterval = null)
    {
        _ = timeout;
        _ = pollInterval;
        return GetCompletion(requestId);
    }

    public ValueTask<InterceptionCompletion> WaitForAsync(
        ulong requestId,
        TimeSpan timeout,
        TimeSpan? pollInterval = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(
            WaitFor(requestId, timeout, pollInterval));
    }
}

internal sealed class LivePatchFakePatchHandle(
    ulong patchId,
    InterceptionTarget target)
    : IInterceptionPatchHandle
{
    private InterceptionCompletion completion = CreateCompletion(
        1,
        patchId,
        target,
        InterceptionOperation.Install,
        InterceptionState.Requested);

    public ulong PatchId { get; } = patchId;

    public InterceptionTarget Target { get; } = target;

    public ulong LastRequestId => completion.RequestId;

    internal int RemoveCount { get; private set; }

    public ulong Replace(InterceptionPlan plan) =>
        throw new NotSupportedException();

    public ulong Replace(InterceptionDispatchPlan plan) =>
        throw new NotSupportedException();

    public ulong Remove()
    {
        RemoveCount++;
        completion = CreateCompletion(
            2,
            PatchId,
            Target,
            InterceptionOperation.Remove,
            InterceptionState.Removing);
        return LastRequestId;
    }

    public InterceptionCompletion GetCompletion() => completion;

    public InterceptionCompletion WaitFor(
        TimeSpan timeout,
        TimeSpan? pollInterval = null)
    {
        _ = timeout;
        _ = pollInterval;
        return completion;
    }

    public void Dispose() => _ = Remove();

    internal void CompleteActive()
    {
        completion = CreateCompletion(
            1,
            PatchId,
            Target,
            InterceptionOperation.Install,
            InterceptionState.Active);
    }

    internal void CompleteRemoved()
    {
        completion = CreateCompletion(
            2,
            PatchId,
            Target,
            InterceptionOperation.Remove,
            InterceptionState.Removed);
    }

    private static InterceptionCompletion CreateCompletion(
        ulong requestId,
        ulong patchId,
        InterceptionTarget target,
        InterceptionOperation operation,
        InterceptionState state) =>
        new(
            requestId,
            patchId,
            operation,
            state,
            0,
            InterceptionPatchFlags.DisableInlining,
            target,
            0,
            0,
            0,
            0,
            TimeSpan.Zero);
}

internal sealed class LivePatchFakeTrampoline : IInterceptionHandlerTrampoline
{
    private bool active = true;

    public Exception? Failure => null;

    public bool TryAcquire(out nint entryPoint)
    {
        entryPoint = active ? 1 : 0;
        return active;
    }

    public Exception? ConsumeFailure() => null;

    public void Dispose() => active = false;
}
