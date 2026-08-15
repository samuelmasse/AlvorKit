namespace AlvorKit;

/// <summary>Owns loopback transport, discovery, and the queue crossing into the game thread.</summary>
internal sealed class LiveCodeHostServer(
    InjectorScopeGraph graph,
    LiveCodeHostOptions options,
    LiveCodeBridgeRegistry bridges,
    LiveCodeFrozenInspectionLane? frozenInspection) : IDisposable
{
    private readonly CancellationTokenSource stopping = new();
    private readonly ConcurrentQueue<LiveCodePendingWork> pending = new();
    private readonly LiveCodeReferenceCatalog references = new(options);
    private readonly LiveCodeBridgeOperationStore bridgeOperations =
        new(options.MaximumBridgeOperations);
    private readonly LiveCodeWire wire = new();
    private TcpListener? listener;
    private Task? acceptLoop;
    private int disposed;

    internal LiveCodeSessionManifest? Session { get; private set; }

    internal LiveCodeSessionManifest Start()
    {
        if (listener is not null)
            throw new InvalidOperationException("The LiveCode host is already running.");
        LiveCodeHostRequestGuard.Validate(options);

        System.IO.Directory.CreateDirectory(options.DiscoveryDirectory);
        listener = new(IPAddress.Loopback, options.Port);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var sessionId = Guid.NewGuid().ToString("N");
        var path = Path.Join(options.DiscoveryDirectory, sessionId + ".json");
        Session = new(
            LiveCodeHost.ProtocolVersion,
            sessionId,
            options.Name,
            Environment.ProcessId,
            port,
            Convert.ToHexString(RandomNumberGenerator.GetBytes(32)),
            DateTimeOffset.UtcNow,
            path,
            frozenInspection is not null);
        WriteManifest(Session);
        acceptLoop = Task.Run(Accept);
        return Session;
    }

    internal bool TryDequeue([NotNullWhen(true)] out LiveCodePendingWork? execution) =>
        pending.TryDequeue(out execution);

    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) != 0)
            return;

        stopping.Cancel();
        listener?.Stop();
        try
        {
            acceptLoop?.GetAwaiter().GetResult();
        }
        catch (Exception exception) when (
            exception is OperationCanceledException
            or ObjectDisposedException
            or SocketException)
        {
        }

        while (pending.TryDequeue(out var execution))
            execution.Cancel("The LiveCode host stopped before execution.");
        bridgeOperations.CancelPending("The LiveCode host stopped before execution.");

        if (Session is not null && File.Exists(Session.ManifestPath))
            File.Delete(Session.ManifestPath);
        stopping.Dispose();
    }

    private async Task Accept()
    {
        while (!stopping.IsCancellationRequested)
        {
            TcpClient client;
            try
            {
                client = await listener!.AcceptTcpClientAsync(stopping.Token);
            }
            catch (Exception exception) when (
                exception is OperationCanceledException
                or ObjectDisposedException
                or SocketException && stopping.IsCancellationRequested)
            {
                break;
            }

            _ = Handle(client);
        }
    }

    private async Task Handle(TcpClient client)
    {
        await using var stream = client.GetStream();
        try
        {
            var request = await wire.Read<LiveCodeWireRequest>(stream, stopping.Token);
            if (!LiveCodeHostRequestGuard.IsAuthorized(Session!, request.Token))
            {
                await wire.Write(
                    stream,
                    new LiveCodeWireResponse(false, "LiveCode authentication failed."),
                    stopping.Token);
                return;
            }

            var response = request.Kind switch
            {
                LiveCodeWireRequestKind.Graph => GraphResponse(),
                LiveCodeWireRequestKind.References => new(true, References: references.Create()),
                LiveCodeWireRequestKind.Execute => await Execute(request),
                LiveCodeWireRequestKind.FrozenInspectionStatus => new(
                    true,
                    FrozenInspection: frozenInspection?.Snapshot()
                        ?? LiveCodeFrozenInspectionLane.DisabledSnapshot()),
                LiveCodeWireRequestKind.FrozenInspectionExecute => await ExecuteFrozen(request),
                LiveCodeWireRequestKind.Bridges => BridgeDescriptors(),
                LiveCodeWireRequestKind.Bridge => await Bridge(request),
                LiveCodeWireRequestKind.BridgeEnqueue => BridgeEnqueue(request),
                LiveCodeWireRequestKind.BridgeOperationStatus => BridgeOperationStatus(request),
                _ => new(false, "Unknown LiveCode request.")
            };
            await wire.Write(stream, response, stopping.Token);
        }
        catch (Exception exception) when (
            exception is IOException
            or InvalidDataException
            or JsonException
            or OperationCanceledException
            or SocketException)
        {
            if (!stopping.IsCancellationRequested)
            {
                try
                {
                    await wire.Write(
                        stream,
                        new LiveCodeWireResponse(false, exception.Message),
                        CancellationToken.None);
                }
                catch (Exception writeException) when (
                    writeException is IOException
                    or ObjectDisposedException
                    or SocketException)
                {
                }
            }
        }
        finally
        {
            client.Dispose();
        }
    }

    private LiveCodeWireResponse GraphResponse()
    {
        var snapshot = graph.Snapshot(includeEnded: true);
        var nodes = snapshot.Nodes.Select(x => new LiveCodeScopeNode(
            x.Id.Value,
            x.ParentId?.Value,
            x.ScopeType,
            x.AttributeType,
            x.Label,
            x.Lifecycle.ToString(),
            x.CreatedRevision,
            x.ChangedRevision)).ToArray();
        return new(true, Graph: new(snapshot.Revision, snapshot.RootId.Value, nodes));
    }

    private async Task<LiveCodeWireResponse> Execute(LiveCodeWireRequest request)
    {
        if (LiveCodeHostRequestGuard.ValidateExecution(options, request) is { } error)
            return new(false, error);

        var execution = new LiveCodePendingExecution(
            request.ScopeId,
            request.EntryType!,
            request.Assembly!,
            request.Symbols);
        pending.Enqueue(execution);
        var result = await execution.Completion.Task.WaitAsync(stopping.Token);
        return new(true, Execution: result);
    }

    private async Task<LiveCodeWireResponse> ExecuteFrozen(LiveCodeWireRequest request)
    {
        if (frozenInspection is null)
            return new(false, "Frozen inspection is disabled for this LiveCode session.");
        if (LiveCodeHostRequestGuard.ValidateExecution(options, request) is { } error)
            return new(false, error);

        var execution = new LiveCodePendingFrozenExecution(
            request.ScopeId,
            request.EntryType!,
            request.Assembly!,
            request.Symbols);
        frozenInspection.Enqueue(execution);
        var result = await execution.Completion.Task.WaitAsync(stopping.Token);
        return new(true, FrozenExecution: result);
    }

    private LiveCodeWireResponse BridgeDescriptors() =>
        options.EnableBridges
            ? new(true, Bridges: bridges.Describe())
            : new(false, "Predefined bridges are disabled for this LiveCode session.");

    private async Task<LiveCodeWireResponse> Bridge(LiveCodeWireRequest request)
    {
        if (!options.EnableBridges)
            return new(false, "Predefined bridges are disabled for this LiveCode session.");
        if (string.IsNullOrWhiteSpace(request.Bridge))
            return new(false, "LiveCode bridge invocation requires a bridge name.");
        if (request.BridgeVersion < 0)
            return new(false, "LiveCode bridge version cannot be negative.");

        var payload = request.Payload
            ?? JsonSerializer.SerializeToElement(new { }, LiveCodeJson.Options);
        var payloadBytes = Encoding.UTF8.GetByteCount(payload.GetRawText());
        if (payloadBytes > options.MaximumBridgePayloadBytes)
            return new(false, $"LiveCode bridge payload exceeds {options.MaximumBridgePayloadBytes} bytes.");

        var execution = new LiveCodePendingBridge(
            request.Bridge,
            request.BridgeVersion,
            payload);
        pending.Enqueue(execution);
        var result = await execution.Completion.Task.WaitAsync(stopping.Token);
        return new(true, BridgeExecution: result);
    }

    private LiveCodeWireResponse BridgeEnqueue(LiveCodeWireRequest request)
    {
        if (ValidateBridge(request) is { } error)
            return new(false, error);
        if (string.IsNullOrWhiteSpace(request.OperationId))
            return new(false, "Two-phase bridge invocation requires an operation id.");
        if (bridges.ValidateInvocation(request.Bridge!, request.BridgeVersion) is { } bridgeError)
            return new(false, bridgeError);

        LiveCodeBridgeOperation operation;
        try
        {
            operation = bridgeOperations.Reserve(request.OperationId);
        }
        catch (InvalidOperationException exception)
        {
            return new(false, exception.Message);
        }

        var payload = request.Payload
            ?? JsonSerializer.SerializeToElement(new { }, LiveCodeJson.Options);
        var execution = new LiveCodePendingBridge(
            request.Bridge!,
            request.BridgeVersion,
            payload,
            operation);
        try
        {
            pending.Enqueue(execution);
        }
        catch (Exception exception)
        {
            execution.Cancel($"Bridge enqueue failed: {exception.Message}");
            return new(
                true,
                BridgeEnqueue: new(
                    operation.Id,
                    LiveCodeBridgeOperationState.Completed,
                    "enqueue-failed"));
        }

        return new(
            true,
            BridgeEnqueue: new(
                operation.Id,
                LiveCodeBridgeOperationState.Pending,
                "queued-for-safe-frame"));
    }

    private LiveCodeWireResponse BridgeOperationStatus(LiveCodeWireRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.OperationId))
            return new(false, "Bridge operation status requires an operation id.");
        try
        {
            return new(
                true,
                BridgeOperation: bridgeOperations.Read(request.OperationId));
        }
        catch (InvalidOperationException exception)
        {
            return new(false, exception.Message);
        }
    }

    private string? ValidateBridge(LiveCodeWireRequest request)
    {
        if (!options.EnableBridges)
            return "Predefined bridges are disabled for this LiveCode session.";
        if (string.IsNullOrWhiteSpace(request.Bridge))
            return "LiveCode bridge invocation requires a bridge name.";
        if (request.BridgeVersion < 0)
            return "LiveCode bridge version cannot be negative.";

        var payload = request.Payload
            ?? JsonSerializer.SerializeToElement(new { }, LiveCodeJson.Options);
        return Encoding.UTF8.GetByteCount(payload.GetRawText()) > options.MaximumBridgePayloadBytes
            ? $"LiveCode bridge payload exceeds {options.MaximumBridgePayloadBytes} bytes."
            : null;
    }

    private static void WriteManifest(LiveCodeSessionManifest session)
    {
        var temporary = session.ManifestPath + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(session, LiveCodeJson.Options));
        File.Move(temporary, session.ManifestPath, overwrite: true);
    }
}
