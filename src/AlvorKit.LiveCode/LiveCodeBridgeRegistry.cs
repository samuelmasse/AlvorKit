namespace AlvorKit;

/// <summary>Owns the predefined bridges advertised and executed by one LiveCode session.</summary>
public sealed class LiveCodeBridgeRegistry
{
    private readonly Lock gate = new();
    private readonly Dictionary<string, ILiveCodeBridge> bridges =
        new(StringComparer.Ordinal);

    /// <summary>Registers a uniquely named bridge.</summary>
    public void Register(ILiveCodeBridge bridge)
    {
        ArgumentNullException.ThrowIfNull(bridge);
        Validate(bridge.Descriptor);

        lock (gate)
        {
            if (!bridges.TryAdd(bridge.Descriptor.Name, bridge))
                throw new InvalidOperationException($"LiveCode bridge '{bridge.Descriptor.Name}' is already registered.");
        }
    }

    /// <summary>Returns the current bridge contracts in stable name order.</summary>
    public LiveCodeBridgeDescriptor[] Describe()
    {
        lock (gate)
        {
            return
            [
                .. bridges.Values
                    .Select(static x => x.Descriptor)
                    .OrderBy(static x => x.Name, StringComparer.Ordinal)
            ];
        }
    }

    internal LiveCodeBridgeExecutionResult Run(LiveCodePendingBridge pending)
    {
        ILiveCodeBridge? bridge;
        lock (gate)
            bridges.TryGetValue(pending.Bridge, out bridge);

        if (bridge is null)
            return Failure(pending, LiveCodeBridgeExecutionStatus.NotFound, $"LiveCode bridge '{pending.Bridge}' is not registered.");
        if (pending.Version != 0 && pending.Version != bridge.Descriptor.Version)
        {
            return Failure(
                pending,
                LiveCodeBridgeExecutionStatus.VersionMismatch,
                $"LiveCode bridge '{pending.Bridge}' is version {bridge.Descriptor.Version}, not {pending.Version}.",
                bridge.Descriptor.Version);
        }

        var timer = Stopwatch.StartNew();
        var context = new LiveCodeBridgeContext();
        try
        {
            bridge.Run(context, pending.Payload);
            timer.Stop();
            return new(
                LiveCodeBridgeExecutionStatus.Completed,
                bridge.Descriptor.Name,
                bridge.Descriptor.Version,
                context.Lines(),
                context.Values(),
                context.Artifacts(),
                timer.Elapsed.TotalMilliseconds,
                null,
                null,
                null);
        }
        catch (Exception exception)
        {
            timer.Stop();
            return new(
                LiveCodeBridgeExecutionStatus.Failed,
                bridge.Descriptor.Name,
                bridge.Descriptor.Version,
                context.Lines(),
                context.Values(),
                context.Artifacts(),
                timer.Elapsed.TotalMilliseconds,
                exception.Message,
                exception.GetType().FullName,
                exception.StackTrace);
        }
    }

    internal string? ValidateInvocation(string name, int version)
    {
        ILiveCodeBridge? bridge;
        lock (gate)
            bridges.TryGetValue(name, out bridge);

        if (bridge is null)
            return $"LiveCode bridge '{name}' is not registered.";
        if (version != 0 && version != bridge.Descriptor.Version)
        {
            return $"LiveCode bridge '{name}' is version {bridge.Descriptor.Version}, not {version}.";
        }
        return null;
    }

    private static LiveCodeBridgeExecutionResult Failure(
        LiveCodePendingBridge pending,
        LiveCodeBridgeExecutionStatus status,
        string error,
        int? version = null) =>
        new(
            status,
            pending.Bridge,
            version ?? pending.Version,
            [],
            [],
            [],
            0,
            error,
            null,
            null);

    private static void Validate(LiveCodeBridgeDescriptor descriptor)
    {
        if (string.IsNullOrWhiteSpace(descriptor.Name))
            throw new ArgumentException("LiveCode bridge name cannot be empty.", nameof(descriptor));
        if (descriptor.Version <= 0)
            throw new ArgumentOutOfRangeException(nameof(descriptor), "LiveCode bridge version must be positive.");
        if (string.IsNullOrWhiteSpace(descriptor.Description))
            throw new ArgumentException("LiveCode bridge description cannot be empty.", nameof(descriptor));
        if (descriptor.RequestSchema.ValueKind != JsonValueKind.Object)
            throw new ArgumentException("LiveCode bridge request schema must be a JSON object.", nameof(descriptor));
    }
}
