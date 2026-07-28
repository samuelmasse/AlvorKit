namespace AlvorKit.LivePatch;

/// <summary>
/// Creates exact handler trampolines and shares one native method slot across
/// compatible receiver-scoped registrations.
/// </summary>
internal sealed class LivePatchInstaller(
    IInterceptionBackend backend,
    InjectorScopeGraph graph,
    Dictionary<InterceptionTarget, LivePatchMethodSlot> methods)
{
    private static readonly InterceptionClaimConsumer ClaimConsumer =
        new("LivePatch");
    private static long nextPatchId;
    private static long nextSlotId;

    /// <summary>Installs a handler and returns its active registration.</summary>
    internal LivePatchRegistration Install(
        MethodInfo targetMethod,
        LivePatchSelector selector,
        object? handlerInstance,
        MethodInfo handlerMethod,
        string? name)
    {
        ValidateSelector(targetMethod, selector);
        var target = InterceptionTarget.FromMethod(targetMethod);
        var trampoline = backend.CreateHandlerTrampoline(
            targetMethod,
            handlerInstance,
            handlerMethod,
            InterceptionHandlerExceptionPolicy.ContainAndDeactivate);

        var patchId = NextId(ref nextPatchId);
        try
        {
            var method = methods.TryGetValue(target, out var existing)
                ? AddToExisting(target, patchId, selector, trampoline, existing)
                : CreateMethod(target, patchId, selector, trampoline);
            return new(
                patchId,
                name ?? handlerMethod.DeclaringType?.Name ?? $"patch-{patchId}",
                target,
                selector,
                method);
        }
        catch
        {
            trampoline.Dispose();
            throw;
        }
    }

    /// <summary>Atomically replaces one active registration's managed handler.</summary>
    internal void Replace(
        LivePatchRegistration registration,
        object? handlerInstance,
        MethodInfo handlerMethod)
    {
        var targetMethod = Resolve(registration.Target);
        var next = backend.CreateHandlerTrampoline(
            targetMethod,
            handlerInstance,
            handlerMethod,
            InterceptionHandlerExceptionPolicy.ContainAndDeactivate);
        try
        {
            var previous = registration.Method.Dispatch.Replace(
                registration.PatchId,
                next);
            previous.Dispose();
        }
        catch
        {
            next.Dispose();
            throw;
        }
    }

    private LivePatchMethodSlot CreateMethod(
        InterceptionTarget target,
        ulong patchId,
        LivePatchSelector selector,
        IInterceptionHandlerTrampoline trampoline)
    {
        var slotId = NextId(ref nextSlotId);
        var dispatch = new LivePatchSlot(graph);
        dispatch.Add(patchId, selector, trampoline);
        var claim = new InterceptionClaim(
            target,
            InterceptionPhysicalRegion.MethodWide,
            new(ClaimConsumer, selector.ToString()),
            InterceptionLogicalOperand.ForMethod(target));
        var claimLease = backend.CollisionRegistry.Acquire(claim);
        try
        {
            LivePatchRuntime.Attach(slotId, dispatch);
            try
            {
                var plan = InterceptionDispatchPlan.ForTarget(
                    target,
                    slotId,
                    LivePatchRuntime.ResolverPointer);
                var method = new LivePatchMethodSlot(
                    slotId,
                    dispatch,
                    backend.Install(plan),
                    claimLease);
                methods.Add(target, method);
                return method;
            }
            catch
            {
                LivePatchRuntime.Detach(slotId, dispatch);
                throw;
            }
        }
        catch
        {
            claimLease.Dispose();
            throw;
        }
    }

    private static LivePatchMethodSlot AddToExisting(
        InterceptionTarget target,
        ulong patchId,
        LivePatchSelector selector,
        IInterceptionHandlerTrampoline trampoline,
        LivePatchMethodSlot method)
    {
        if (method.Finished || method.Dispatch.Count == 0)
        {
            throw new InvalidOperationException(
                $"Method '{target.DisplayName}' is still retiring its previous native wrapper.");
        }

        method.Dispatch.Add(patchId, selector, trampoline);
        method.RefreshClaimSelector();
        return method;
    }

    private void ValidateSelector(
        MethodInfo target,
        LivePatchSelector selector)
    {
        if (target.IsStatic && selector.Kind != LivePatchSelectorKind.All)
        {
            throw new NotSupportedException(
                "Static methods have no receiver ownership; select All explicitly.");
        }
        if (selector.Kind is LivePatchSelectorKind.ExactScope or
            LivePatchSelectorKind.ScopeAndDescendants)
        {
            if (!graph.TryGetActiveScope(selector.ScopeId, out _))
                throw new InvalidOperationException($"Scope '{selector.ScopeId}' is not active.");
        }
    }

    private static MethodInfo Resolve(InterceptionTarget target)
    {
        foreach (var module in AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(static assembly => assembly.Modules))
        {
            if (module.ModuleVersionId == target.ModuleMvid)
                return (MethodInfo)module.ResolveMethod(target.MethodToken)!;
        }

        throw new InvalidOperationException(
            $"Target module '{target.ModuleMvid}' is no longer loaded.");
    }

    private static ulong NextId(ref long value) =>
        checked((ulong)Interlocked.Increment(ref value));
}
