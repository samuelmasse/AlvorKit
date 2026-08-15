namespace AlvorKit;

/// <summary>Binary output returned to the LiveCode client instead of being written inside the game process.</summary>
public sealed record LiveCodeBridgeArtifact(
    string Name,
    string ContentType,
    byte[] Data);
