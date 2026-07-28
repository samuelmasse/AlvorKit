namespace AlvorKit.Windowing;

/// <summary>Structured output from one atomic AlvorSense command batch.</summary>
/// <param name="CommandsExecuted">Number of commands successfully executed before stop.</param>
/// <param name="Output">Text emitted by the command protocol.</param>
/// <param name="Time">Deterministic agent time after the batch.</param>
/// <param name="Updates">Cumulative update count after the batch.</param>
/// <param name="Renders">Cumulative render count after the batch.</param>
/// <param name="Mouse">Last synthetic pointer position.</param>
/// <param name="Artifacts">Framebuffer artifacts captured by the batch.</param>
internal sealed record AgentWindowPuppetResult(
    int CommandsExecuted,
    string Output,
    double Time,
    int Updates,
    int Renders,
    Vec2 Mouse,
    AgentWindowPuppetArtifact[] Artifacts);
