namespace AlvorKit.Script.ZoneSolution;

/// <summary>A sibling repository selected for the generated zone solution.</summary>
/// <param name="Name">Repository directory name used as the solution folder prefix.</param>
/// <param name="RootPath">Absolute repository root path.</param>
/// <param name="SolutionPath">Absolute primary .slnx path.</param>
internal sealed record ZoneRepository(string Name, string RootPath, string SolutionPath);
