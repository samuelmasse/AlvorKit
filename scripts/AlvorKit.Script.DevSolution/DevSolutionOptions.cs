namespace AlvorKit.Script.DevSolution;

/// <summary>Command-line options for generating a combined local development solution.</summary>
/// <param name="ConsumerSolutionPath">Solution owned by the consumer repository.</param>
/// <param name="EngineSolutionPath">AlvorKit solution to project into the generated solution.</param>
/// <param name="OutputPath">Generated development solution path.</param>
/// <param name="EngineFolderSegments">Solution-folder path that should contain AlvorKit projects.</param>
internal sealed record DevSolutionOptions(
    string ConsumerSolutionPath,
    string EngineSolutionPath,
    string OutputPath,
    IReadOnlyList<string> EngineFolderSegments)
{
    /// <summary>Creates the command-line surface for the generator.</summary>
    /// <param name="defaultConsumerSolution">Consumer solution provider used when <c>--consumer-solution</c> is omitted.</param>
    /// <param name="defaultEngineSolution">AlvorKit solution provider used when <c>--engine-solution</c> is omitted.</param>
    /// <param name="execute">Action that executes the generator with parsed options.</param>
    internal static RootCommand CreateRootCommand(
        Func<string> defaultConsumerSolution,
        Func<string> defaultEngineSolution,
        Func<DevSolutionOptions, Task<int>> execute)
    {
        var consumerSolution = new Option<string?>("--consumer-solution") { Description = "Consumer repository .slnx file." };
        var engineSolution = new Option<string?>("--engine-solution") { Description = "AlvorKit .slnx file to include." };
        var output = new Option<string?>("--output") { Description = "Generated .slnx output path." };
        var engineFolder = new Option<string?>("--engine-folder") { Description = "Solution folder for AlvorKit projects." };
        var command = new RootCommand("Generate a local Visual Studio solution that includes a sibling AlvorKit checkout.");
        command.Options.Add(consumerSolution);
        command.Options.Add(engineSolution);
        command.Options.Add(output);
        command.Options.Add(engineFolder);
        command.SetAction(parse => execute(Options(parse, defaultConsumerSolution, defaultEngineSolution, consumerSolution, engineSolution, output, engineFolder)));
        return command;
    }

    /// <summary>Parses command-line arguments into validated generator options.</summary>
    /// <param name="args">Command-line arguments supplied by the caller.</param>
    public static DevSolutionOptions Parse(IReadOnlyList<string> args) =>
        Parse(args, SolutionDefaults.FindConsumerSolution, SolutionDefaults.FindEngineSolution);

    /// <summary>Parses command-line arguments using explicit defaults for tests.</summary>
    /// <param name="args">Command-line arguments supplied by the caller.</param>
    /// <param name="defaultConsumerSolution">Consumer solution provider used when <c>--consumer-solution</c> is omitted.</param>
    /// <param name="defaultEngineSolution">AlvorKit solution provider used when <c>--engine-solution</c> is omitted.</param>
    internal static DevSolutionOptions Parse(
        IReadOnlyList<string> args,
        Func<string> defaultConsumerSolution,
        Func<string> defaultEngineSolution)
    {
        var consumerSolution = new Option<string?>("--consumer-solution") { Description = "Consumer repository .slnx file." };
        var engineSolution = new Option<string?>("--engine-solution") { Description = "AlvorKit .slnx file to include." };
        var output = new Option<string?>("--output") { Description = "Generated .slnx output path." };
        var engineFolder = new Option<string?>("--engine-folder") { Description = "Solution folder for AlvorKit projects." };
        var command = new RootCommand("Generate a local Visual Studio solution that includes a sibling AlvorKit checkout.");
        command.Options.Add(consumerSolution);
        command.Options.Add(engineSolution);
        command.Options.Add(output);
        command.Options.Add(engineFolder);
        var result = command.Parse(args.ToArray());
        ThrowIfErrors(result);

        return Options(result, defaultConsumerSolution, defaultEngineSolution, consumerSolution, engineSolution, output, engineFolder);
    }

    /// <summary>Creates generator options from parsed command-line values.</summary>
    private static DevSolutionOptions Options(
        ParseResult parse,
        Func<string> defaultConsumerSolution,
        Func<string> defaultEngineSolution,
        Option<string?> consumerSolution,
        Option<string?> engineSolution,
        Option<string?> output,
        Option<string?> engineFolder)
    {
        var consumerPath = Path.GetFullPath(parse.GetValue(consumerSolution) ?? defaultConsumerSolution());
        var enginePath = Path.GetFullPath(parse.GetValue(engineSolution) ?? defaultEngineSolution());
        var outputPath = Path.GetFullPath(parse.GetValue(output) ?? DefaultOutputPath(consumerPath));
        var engineFolderSegments = SolutionFolderPath.ParseSegments(parse.GetValue(engineFolder) ?? "Engine");

        return new(consumerPath, enginePath, outputPath, engineFolderSegments);
    }

    /// <summary>Returns the default generated solution path for a consumer solution.</summary>
    internal static string DefaultOutputPath(string consumerSolutionPath)
    {
        var directory = Path.GetDirectoryName(consumerSolutionPath) ?? Environment.CurrentDirectory;
        var name = Path.GetFileNameWithoutExtension(consumerSolutionPath);
        return Path.Combine(directory, name + ".Dev.slnx");
    }

    /// <summary>Throws an argument exception when System.CommandLine found parse errors.</summary>
    private static void ThrowIfErrors(ParseResult result)
    {
        if (result.Action is System.CommandLine.Help.HelpAction)
            throw new ArgumentException("Help is generated by the command-line app.");
        if (result.Errors.Count > 0)
            throw new ArgumentException(string.Join(" ", result.Errors.Select(error => error.Message)));
    }
}
