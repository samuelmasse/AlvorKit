namespace AlvorKit.Engine;

/// <summary>
/// Exposes compiled exact LivePatch handlers through a discoverable LiveCode
/// control plane that executes on the game's safe-frame thread.
/// </summary>
public sealed class LivePatchLiveCodeBridge : ILiveCodeBridge, IDisposable
{
    private readonly LivePatchSession session;
    private readonly InjectorScopeGraph graph;
    private readonly LivePatchBridgeProtocol protocol = new();
    private readonly LivePatchSubmissionLoader submissions;
    private readonly LivePatchBridgeRegistrations registrations = new();
    private bool disposed;

    /// <summary>Creates a bridge around the root LivePatch session and scope graph.</summary>
    public LivePatchLiveCodeBridge(
        LivePatchSession session,
        InjectorScopeGraph graph)
    {
        this.session = session;
        this.graph = graph;
        submissions = new(protocol);
    }

    /// <inheritdoc />
    public LiveCodeBridgeDescriptor Descriptor => protocol.Descriptor;

    /// <inheritdoc />
    public void Run(LiveCodeBridgeContext context, JsonElement request)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        Pump();
        var operation = protocol.RequiredString(request, "operation");
        switch (operation)
        {
            case "capabilities":
                Capabilities(context);
                break;
            case "list":
                context.Value("patches", session.List());
                context.Value("contexts", registrations.ContextStates());
                break;
            case "status":
                Status(context, protocol.RequiredUInt64(request, "patchId"));
                break;
            case "install":
                Install(context, request);
                break;
            case "replace":
                Replace(context, request);
                break;
            case "remove":
                Remove(context, protocol.RequiredUInt64(request, "patchId"));
                break;
            default:
                throw new ArgumentException($"Unknown LivePatch operation '{operation}'.");
        }
    }

    /// <summary>Releases collectible contexts whose patch reached a terminal state.</summary>
    public void Pump()
    {
        foreach (var item in registrations.ActiveSnapshot())
        {
            var snapshot = session.Get(item.Lease.PatchId);
            if (snapshot.State is LivePatchState.Removed or LivePatchState.Failed)
                registrations.Release(item);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (disposed)
            return;
        disposed = true;
        foreach (var item in registrations.ActiveSnapshot())
            registrations.Release(item);
    }

    private void Capabilities(LiveCodeBridgeContext context)
    {
        context.Value("bridgeVersion", Descriptor.Version);
        context.Value("profiler", session.Capabilities);
        context.Value(
            "selectors",
            new[] { "exactInstance", "exactScope", "descendants", "all" });
        context.Value("modes", new[] { "replace" });
        context.Value("handlerAbi", "exact receiver + declared arguments + exact return");
        context.Value(
            "unsupported",
            new[]
            {
                "open or constructed generic targets",
                "value-type receivers",
                "managed-reference returns",
                "ref-struct returns"
            });
    }

    private void Status(LiveCodeBridgeContext context, ulong patchId)
    {
        context.Value("patch", session.Get(patchId));
        context.Value("submissionContext", registrations.ContextState(patchId));
    }

    private void Install(LiveCodeBridgeContext output, JsonElement request)
    {
        var executorScopeId = new InjectorScopeId(
            protocol.RequiredInt64(request, "executorScopeId"));
        var executor = RequireScope(executorScopeId);
        var loaded = submissions.Load(request);
        try
        {
            var target = submissions.ResolveTarget(
                request.GetProperty("target"),
                loaded.HandlerMethod);
            var selector = ResolveSelector(
                request.GetProperty("selector"),
                target,
                executorScopeId);
            var handler = submissions.CreateHandler(loaded.HandlerType, executor);
            var lease = session.InstallReplace(
                target,
                selector,
                handler,
                loaded.HandlerMethod,
                protocol.OptionalString(request, "name"));
            registrations.Add(lease, loaded, executorScopeId);
            output.WriteLine(
                $"LivePatch {lease.PatchId} was accepted; activation completes asynchronously at a safe frame.");
            output.Value("patch", lease.Snapshot());
            output.Value("targetMethod", target.ToString());
            output.Value("executorScopeId", executorScopeId.Value);
        }
        catch
        {
            loaded.Context.Unload();
            throw;
        }
    }

    private void Replace(LiveCodeBridgeContext output, JsonElement request)
    {
        var patchId = protocol.RequiredUInt64(request, "patchId");
        if (!registrations.TryGet(patchId, out var existing))
        {
            throw new KeyNotFoundException(
                $"LivePatch {patchId} does not own a submitted handler in this session.");
        }

        var executorId = request.TryGetProperty("executorScopeId", out var executorElement) &&
            executorElement.ValueKind == JsonValueKind.Number
            ? new InjectorScopeId(executorElement.GetInt64())
            : existing.ExecutorScopeId;
        var loaded = submissions.Load(request);
        try
        {
            var handler = submissions.CreateHandler(
                loaded.HandlerType,
                RequireScope(executorId));
            existing.Lease.Replace(handler, loaded.HandlerMethod);
            registrations.Replace(existing, loaded, executorId);
            output.WriteLine(
                $"LivePatch {patchId} published its new handler atomically; no ReJIT was required.");
            output.Value("patch", existing.Lease.Snapshot());
            output.Value("executorScopeId", executorId.Value);
        }
        catch
        {
            loaded.Context.Unload();
            throw;
        }
    }

    private void Remove(LiveCodeBridgeContext output, ulong patchId)
    {
        if (registrations.TryGet(patchId, out var item))
        {
            registrations.Release(item);
        }
        else
        {
            var snapshot = session.Get(patchId);
            if (snapshot.State is not (LivePatchState.Removed or LivePatchState.Failed))
            {
                throw new InvalidOperationException(
                    $"LivePatch {patchId} is active but is not owned by this LiveCode bridge.");
            }
        }

        output.WriteLine(
            $"LivePatch {patchId} stopped dispatching immediately; native original-IL restoration is asynchronous.");
        output.Value("patch", session.Get(patchId));
        output.Value("submissionContext", registrations.ContextState(patchId));
    }

    private LivePatchSelector ResolveSelector(
        JsonElement descriptor,
        MethodInfo target,
        InjectorScopeId executorScopeId)
    {
        var kind = protocol.RequiredString(descriptor, "kind");
        var scopeId = descriptor.TryGetProperty("scopeId", out var element) &&
            element.ValueKind == JsonValueKind.Number
            ? new InjectorScopeId(element.GetInt64())
            : executorScopeId;
        return kind switch
        {
            "exactInstance" => LivePatchSelector.ExactInstance(
                RequireScope(scopeId).Get(target.DeclaringType!)),
            "exactScope" => LivePatchSelector.ExactScope(scopeId),
            "descendants" => LivePatchSelector.Descendants(scopeId),
            "all" => LivePatchSelector.All(),
            _ => throw new ArgumentException($"Unknown LivePatch selector kind '{kind}'.")
        };
    }

    private InjectorScope RequireScope(InjectorScopeId id) =>
        graph.TryGetActiveScope(id, out var scope)
            ? scope
            : throw new InvalidOperationException(
                $"Injector scope '{id}' is not active.");
}
