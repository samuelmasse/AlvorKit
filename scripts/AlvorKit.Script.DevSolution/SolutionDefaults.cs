namespace AlvorKit.Script.DevSolution;

/// <summary>Finds default solution paths for local development solution generation.</summary>
internal static class SolutionDefaults
{
    /// <summary>Finds the nearest single non-generated consumer solution above the current directory.</summary>
    public static string FindConsumerSolution()
    {
        for (var directory = Environment.CurrentDirectory; directory is not null; directory = Directory.GetParent(directory)?.FullName)
        {
            var solutions = Directory.GetFiles(directory, "*.slnx", SearchOption.TopDirectoryOnly)
                .Where(path => !Path.GetFileName(path).EndsWith(".Dev.slnx", StringComparison.OrdinalIgnoreCase))
                .Order(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (solutions.Length == 1)
                return solutions[0];
            if (solutions.Length > 1)
                throw new InvalidOperationException($"Multiple .slnx files found in {directory}. Pass --consumer-solution.");
        }

        throw new InvalidOperationException("No consumer .slnx file found above the current directory.");
    }

    /// <summary>Finds the AlvorKit solution for the script checkout.</summary>
    public static string FindEngineSolution() =>
        Path.Combine(ProjectRoot.FindFromCurrentProcess(typeof(SolutionDefaults)), ProjectRoot.SolutionFileName);
}
