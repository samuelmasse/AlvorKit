namespace AlvorKit;

/// <summary>Records one evaluated project and the configurations in which it participates.</summary>
internal record SolutionProject(string Path, bool Startup, bool Executable, IReadOnlySet<string> Configurations);
