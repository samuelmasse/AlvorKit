namespace AlvorKit;

/// <summary>Implements one discoverable, versioned operation that runs directly on the game thread.</summary>
public interface ILiveCodeBridge
{
    /// <summary>Gets the bridge contract advertised to connected clients.</summary>
    LiveCodeBridgeDescriptor Descriptor { get; }

    /// <summary>Executes a validated request payload and writes structured output.</summary>
    void Run(LiveCodeBridgeContext context, JsonElement request);
}
