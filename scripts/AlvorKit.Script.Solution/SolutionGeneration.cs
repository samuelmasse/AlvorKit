namespace AlvorKit;

/// <summary>Holds a complete evaluated document before the watcher verifies its checkout is still current.</summary>
internal record SolutionGeneration(string Root, string Content, int ProjectCount);
