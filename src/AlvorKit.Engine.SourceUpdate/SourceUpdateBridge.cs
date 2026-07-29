namespace AlvorKit.Engine.SourceUpdate;

/// <summary>Applies compiler-produced source deltas at the LiveCode safe-frame boundary.</summary>
public sealed class SourceUpdateBridge(SourceUpdateModuleLedger ledger) : ILiveCodeBridge
{
    /// <inheritdoc />
    public LiveCodeBridgeDescriptor Descriptor { get; } = new(
        "source-update",
        1,
        "Inspect or apply a verified existing-method source update.",
        true,
        LiveCodeBridgeLease.None,
        JsonSerializer.SerializeToElement(
            new
            {
                type = "object",
                properties = new
                {
                    operation = new { type = "string", values = new[] { "capabilities", "apply" } },
                    apply = new { type = "object" }
                },
                required = new[] { "operation" }
            },
            SourceUpdateJson.Options));

    /// <inheritdoc />
    public void Run(LiveCodeBridgeContext context, JsonElement request)
    {
        var envelope = request.Deserialize<SourceUpdateBridgeRequest>(SourceUpdateJson.Options)
            ?? throw new InvalidDataException("Source Update request is invalid.");
        var response = envelope.Operation switch
        {
            "capabilities" => new SourceUpdateBridgeResponse(Capabilities: ledger.Capabilities()),
            "apply" when envelope.Apply is not null =>
                new SourceUpdateBridgeResponse(Apply: ledger.Apply(envelope.Apply)),
            "apply" => throw new InvalidDataException("Source Update apply request is missing its payload."),
            _ => throw new InvalidDataException($"Unknown Source Update operation '{envelope.Operation}'.")
        };
        context.Value("response", response);
    }
}
