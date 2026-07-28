namespace AlvorKit.LivePatch;

/// <summary>
/// Owns scope-aware exact handlers, native wrappers, collision policy,
/// safe-frame completion pumping, and scope teardown.
/// </summary>
public sealed class LivePatchSession : IDisposable
{
    private readonly Lock gate = new();
    private readonly IInterceptionBackend backend;
    private readonly InjectorScopeGraph graph;
    private readonly Dictionary<InterceptionTarget, LivePatchMethodSlot> methods = [];
    private readonly LivePatchRegistrationStore registrations = new();
    private readonly LivePatchInstaller installer;
    private bool disposed;

    /// <summary>Creates a session around a prepared interception backend.</summary>
    public LivePatchSession(IInterceptionBackend backend, InjectorScopeGraph graph)
    {
        this.backend = backend;
        this.graph = graph;
        installer = new(backend, graph, methods);
        graph.ScopeEnding += ScopeEnding;
    }

    /// <summary>Gets the negotiated native backend capabilities for discovery and diagnostics.</summary>
    public InterceptionCapabilities Capabilities => backend.Capabilities;

    /// <summary>Installs one submitted exact handler with an explicit receiver selector.</summary>
    public LivePatchLease InstallReplace(
        MethodInfo targetMethod,
        LivePatchSelector selector,
        object? handlerInstance,
        MethodInfo handlerMethod,
        string? name = null)
    {
        ArgumentNullException.ThrowIfNull(targetMethod);
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(handlerMethod);

        lock (gate)
        {
            ThrowIfDisposed();
            var registration = installer.Install(
                targetMethod,
                selector,
                handlerInstance,
                handlerMethod,
                name);
            registrations.Add(registration);
            return new(this, registration.PatchId);
        }
    }

    /// <summary>Pumps native completions at a game safe-frame boundary.</summary>
    public int Pump()
    {
        lock (gate)
        {
            ThrowIfDisposed();
            var changed = 0;
            foreach (var registration in registrations.ActiveSnapshot())
            {
                var failure = registration.Method.Dispatch.GetFailure(
                    registration.PatchId);
                if (failure is null)
                    continue;

                FailRegistration(registration, failure);
                changed++;
            }
            foreach (var method in methods.Values.ToArray())
            {
                if (!method.Pump())
                    continue;

                changed++;
                registrations.RefreshTerminalNativeEvidence(method);
                var methodRegistrations = registrations.ForMethod(method);
                if (method.Completion.State == InterceptionState.Failed)
                {
                    foreach (var registration in methodRegistrations)
                    {
                        var removed = method.Dispatch.Remove(
                            registration.PatchId);
                        removed?.Dispose();
                    }
                }
                foreach (var registration in methodRegistrations)
                {
                    UpdateState(registration);
                }
                if (method.Finished && method.Dispatch.Count == 0)
                    methods.Remove(method.NativePatch.Target);
            }

            return changed;
        }
    }

    /// <summary>Returns active and retained terminal patch evidence in stable ID order.</summary>
    public LivePatchSnapshot[] List()
    {
        lock (gate)
        {
            return registrations.List();
        }
    }

    /// <summary>Reads one active or retained terminal patch.</summary>
    public LivePatchSnapshot Get(ulong patchId)
    {
        lock (gate)
        {
            return registrations.Get(patchId);
        }
    }

    internal void Replace(
        ulong patchId,
        object? handlerInstance,
        MethodInfo handlerMethod)
    {
        ArgumentNullException.ThrowIfNull(handlerMethod);
        lock (gate)
        {
            ThrowIfDisposed();
            var registration = Require(patchId);
            installer.Replace(registration, handlerInstance, handlerMethod);
        }
    }

    internal void Remove(ulong patchId)
    {
        lock (gate)
        {
            if (disposed)
                return;
            if (!registrations.ContainsActive(patchId))
            {
                if (registrations.ContainsHistory(patchId))
                    return;
                throw new KeyNotFoundException($"LivePatch {patchId} does not exist.");
            }
            RemoveLocked(patchId);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        lock (gate)
        {
            if (disposed)
                return;
            disposed = true;
            graph.ScopeEnding -= ScopeEnding;
            foreach (var registration in registrations.ActiveSnapshot())
            {
                var removed = registration.Method.Dispatch.Remove(
                    registration.PatchId);
                removed?.Dispose();
            }
            foreach (var method in methods.Values)
            {
                LivePatchRuntime.Detach(method.SlotId, method.Dispatch);
                if (method.Completion.State == InterceptionState.Active)
                    _ = method.NativePatch.Remove();
                method.ReleaseClaim();
            }
            registrations.ClearActive();
            methods.Clear();
        }
    }

    private void ScopeEnding(InjectorScopeEnding ending)
    {
        lock (gate)
        {
            if (disposed)
                return;
            var selected = registrations.ActiveSnapshot()
                .Where(x => x.Selector.EndsWith(ending, graph))
                .Select(x => x.PatchId)
                .ToArray();
            foreach (var patchId in selected)
                RemoveLocked(patchId);
        }
    }

    private void RemoveLocked(ulong patchId)
    {
        var registration = Require(patchId);
        var removed = registration.Method.Dispatch.Remove(patchId);
        removed?.Dispose();
        registration.State = LivePatchState.Removing;
        if (registration.Method.Dispatch.Count == 0)
            registration.Method.BeginRetire();
        else
        {
            registration.Method.RefreshClaimSelector();
            CompleteRegistration(registration, LivePatchState.Removed);
        }
    }

    private void UpdateState(LivePatchRegistration registration)
    {
        var completion = registration.Method.Completion;
        registration.State = completion.State switch
        {
            InterceptionState.Active => LivePatchState.Active,
            InterceptionState.Removing => LivePatchState.Removing,
            InterceptionState.Removed => LivePatchState.Removed,
            InterceptionState.Failed => LivePatchState.Failed,
            _ => LivePatchState.Installing
        };
        if (registration.State == LivePatchState.Failed)
            registration.Failure = $"Native ReJIT failed with HRESULT 0x{completion.HResult:X8}.";
        if (registration.State is LivePatchState.Removed or LivePatchState.Failed)
            CompleteRegistration(registration, registration.State);
    }

    private void FailRegistration(LivePatchRegistration registration, Exception failure)
    {
        var removed = registration.Method.Dispatch.Remove(
            registration.PatchId);
        removed?.Dispose();
        registration.Failure =
            $"{failure.GetType().FullName}: {failure.Message}";
        if (registration.Method.Dispatch.Count == 0)
            registration.Method.BeginRetire();
        else
            registration.Method.RefreshClaimSelector();
        CompleteRegistration(registration, LivePatchState.Failed);
    }

    private void CompleteRegistration(LivePatchRegistration registration, LivePatchState state)
    {
        registration.State = state;
        registrations.Complete(registration, state);
    }

    private LivePatchRegistration Require(ulong patchId) =>
        registrations.Require(patchId);

    private void ThrowIfDisposed() =>
        ObjectDisposedException.ThrowIf(disposed, this);
}
