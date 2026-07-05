namespace AlvorKit.Script.DevSolution;

/// <summary>Result of a local development solution generation run.</summary>
/// <param name="OutputPath">Generated solution path.</param>
/// <param name="Changed">Whether the output file was written.</param>
/// <param name="ConsumerProjectCount">Number of projects read from the consumer solution.</param>
/// <param name="EngineProjectCount">Number of projects read from the engine solution.</param>
internal sealed record DevSolutionResult(string OutputPath, bool Changed, int ConsumerProjectCount, int EngineProjectCount);
