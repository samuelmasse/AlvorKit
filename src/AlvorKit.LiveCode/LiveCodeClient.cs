namespace AlvorKit.LiveCode;

/// <summary>Connects to one discovered LiveCode host over its private loopback protocol.</summary>
public sealed class LiveCodeClient(LiveCodeSessionManifest session)
{
    private readonly LiveCodeWire wire = new();

    /// <summary>Gets the selected session.</summary>
    public LiveCodeSessionManifest Session { get; } = session;

    /// <summary>Gets the target's current tracked scope graph.</summary>
    public async Task<LiveCodeScopeGraph> Graph(CancellationToken cancellationToken = default)
    {
        var response = await Send(
            new(Session.Token, LiveCodeWireRequestKind.Graph),
            cancellationToken);
        return response.Graph
            ?? throw new LiveCodeClientException("LiveCode host returned no scope graph.");
    }

    /// <summary>Gets the exact assembly references and imports used to compile a command for the target.</summary>
    public async Task<LiveCodeReferenceManifest> References(CancellationToken cancellationToken = default)
    {
        var response = await Send(
            new(Session.Token, LiveCodeWireRequestKind.References),
            cancellationToken);
        return response.References
            ?? throw new LiveCodeClientException("LiveCode host returned no compilation references.");
    }

    /// <summary>Gets the predefined bridge contracts currently advertised by the target.</summary>
    public async Task<LiveCodeBridgeDescriptor[]> Bridges(CancellationToken cancellationToken = default)
    {
        var response = await Send(
            new(Session.Token, LiveCodeWireRequestKind.Bridges),
            cancellationToken);
        return response.Bridges
            ?? throw new LiveCodeClientException("LiveCode host returned no bridge descriptors.");
    }

    /// <summary>Submits one compiled command to an exact tracked scope and waits for game-thread execution.</summary>
    public async Task<LiveCodeExecutionResult> Execute(
        long scopeId,
        string entryType,
        byte[] assembly,
        byte[]? symbols = null,
        CancellationToken cancellationToken = default)
    {
        var response = await Send(
            new(Session.Token, LiveCodeWireRequestKind.Execute, scopeId, entryType, assembly, symbols),
            cancellationToken);
        return response.Execution
            ?? throw new LiveCodeClientException("LiveCode host returned no execution result.");
    }

    /// <summary>Reads the out-of-band lane's current game-frame heartbeat and execution state.</summary>
    public async Task<LiveCodeFrozenInspectionSnapshot> FrozenInspectionStatus(
        CancellationToken cancellationToken = default)
    {
        var response = await Send(
            new(Session.Token, LiveCodeWireRequestKind.FrozenInspectionStatus),
            cancellationToken);
        return response.FrozenInspection
            ?? throw new LiveCodeClientException("LiveCode host returned no frozen-inspection status.");
    }

    /// <summary>
    /// Submits an ordinary compiled command to an exact scope and runs it on the dedicated thread only if frames stalled.
    /// </summary>
    public async Task<LiveCodeFrozenInspectionExecutionResult> ExecuteFrozen(
        long scopeId,
        string entryType,
        byte[] assembly,
        byte[]? symbols = null,
        CancellationToken cancellationToken = default)
    {
        var response = await Send(
            new(
                Session.Token,
                LiveCodeWireRequestKind.FrozenInspectionExecute,
                scopeId,
                entryType,
                assembly,
                symbols),
            cancellationToken);
        return response.FrozenExecution
            ?? throw new LiveCodeClientException("LiveCode host returned no frozen execution result.");
    }

    /// <summary>Invokes one predefined bridge and waits for its game-thread result.</summary>
    public async Task<LiveCodeBridgeExecutionResult> Bridge(
        string name,
        JsonElement payload,
        int version = 0,
        CancellationToken cancellationToken = default)
    {
        var response = await Send(
            new(
                Session.Token,
                LiveCodeWireRequestKind.Bridge,
                Bridge: name,
                BridgeVersion: version,
                Payload: payload),
            cancellationToken);
        return response.BridgeExecution
            ?? throw new LiveCodeClientException("LiveCode host returned no bridge execution result.");
    }

    /// <summary>Reserves and queues one bridge invocation without waiting for the game-thread pump.</summary>
    public async Task<LiveCodeBridgeEnqueueResponse> EnqueueBridge(
        string operationId,
        string name,
        JsonElement payload,
        int version = 0,
        CancellationToken cancellationToken = default)
    {
        var response = await Send(
            new(
                Session.Token,
                LiveCodeWireRequestKind.BridgeEnqueue,
                Bridge: name,
                BridgeVersion: version,
                Payload: payload,
                OperationId: operationId),
            cancellationToken);
        return response.BridgeEnqueue
            ?? throw new LiveCodeClientException("LiveCode host returned no bridge enqueue acknowledgment.");
    }

    /// <summary>Reads accepted bridge-operation state without entering the game-thread queue.</summary>
    public async Task<LiveCodeBridgeOperationStatusResponse> BridgeOperationStatus(
        string operationId,
        CancellationToken cancellationToken = default)
    {
        var response = await Send(
            new(
                Session.Token,
                LiveCodeWireRequestKind.BridgeOperationStatus,
                OperationId: operationId),
            cancellationToken);
        return response.BridgeOperation
            ?? throw new LiveCodeClientException("LiveCode host returned no bridge operation status.");
    }

    private async Task<LiveCodeWireResponse> Send(
        LiveCodeWireRequest request,
        CancellationToken cancellationToken)
    {
        if (Session.ProtocolVersion != LiveCodeHost.ProtocolVersion)
        {
            throw new LiveCodeClientException(
                $"LiveCode protocol {Session.ProtocolVersion} is incompatible with client protocol {LiveCodeHost.ProtocolVersion}.");
        }

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, Session.Port, cancellationToken);
        await using var stream = client.GetStream();
        await wire.Write(stream, request, cancellationToken);
        var response = await wire.Read<LiveCodeWireResponse>(stream, cancellationToken);
        if (!response.Ok)
            throw new LiveCodeClientException(response.Error ?? "LiveCode host rejected the request.");
        return response;
    }
}
