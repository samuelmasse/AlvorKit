namespace AlvorKit;

/// <summary>One client-named framebuffer capture produced by a puppet command batch.</summary>
/// <param name="Name">Client-selected artifact name.</param>
/// <param name="Png">RGBA PNG bytes captured before buffer swap.</param>
internal sealed record AgentWindowPuppetArtifact(string Name, byte[] Png);
