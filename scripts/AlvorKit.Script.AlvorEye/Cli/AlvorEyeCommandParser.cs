namespace AlvorKit.Script.AlvorEye;

/// <summary>Parses command-line arguments for the AlvorEye helper.</summary>
internal static class AlvorEyeCommandParser
{
    /// <summary>Parses command-line arguments using repository-root discovery for defaults.</summary>
    /// <param name="args">Command-line arguments passed to the process.</param>
    public static AlvorEyeCommand Parse(IReadOnlyList<string> args) =>
        Parse(args, () => ProjectRoot.FindFromCurrentProcess(typeof(AlvorEyeCommandParser)));

    /// <summary>Parses command-line arguments with an explicit default repository root for tests.</summary>
    /// <param name="args">Command-line arguments passed to the process.</param>
    /// <param name="defaultRepoRoot">Repository root to use when <c>--repo-root</c> is omitted.</param>
    internal static AlvorEyeCommand Parse(IReadOnlyList<string> args, string defaultRepoRoot) =>
        Parse(args, () => defaultRepoRoot);

    /// <summary>Creates the command tree for AlvorEye.</summary>
    /// <param name="defaultRepoRoot">Repository root provider used when <c>--repo-root</c> is omitted.</param>
    /// <param name="execute">Action that executes the parsed command.</param>
    internal static RootCommand CreateRootCommand(Func<string> defaultRepoRoot, Func<AlvorEyeCommand, Task<int>> execute)
    {
        var root = new RootCommand("Desktop visual automation helper.");
        root.Subcommands.Add(CreateCommand("run", "Execute a complete scenario.", AlvorEyeCommandKind.Run, defaultRepoRoot, execute));
        root.Subcommands.Add(CreateCommand("session", "Run JSONL actions from stdin.", AlvorEyeCommandKind.Session, defaultRepoRoot, execute));
        root.Subcommands.Add(CreateCommand("handoff", "Freeze a session target and capture a frame.", AlvorEyeCommandKind.Handoff, defaultRepoRoot, execute));
        root.Subcommands.Add(CreateCommand("resume", "Resume a frozen session target.", AlvorEyeCommandKind.Resume, defaultRepoRoot, execute));
        return root;
    }

    /// <summary>Creates one AlvorEye command with the shared options.</summary>
    private static Command CreateCommand(
        string name,
        string description,
        AlvorEyeCommandKind kind,
        Func<string> defaultRepoRoot,
        Func<AlvorEyeCommand, Task<int>> execute)
    {
        var scenario = new Option<string>("--scenario") { Description = "Scenario JSON file." };
        var session = new Option<string>("--session") { Description = "Persistent session id." };
        var repoRoot = new Option<string>("--repo-root") { Description = "Repository root." };
        var command = new Command(name, description);
        command.Options.Add(scenario);
        command.Options.Add(session);
        command.Options.Add(repoRoot);
        command.SetAction(parse =>
        {
            var scenarioValue = parse.GetValue(scenario);
            var sessionValue = parse.GetValue(session);
            Validate(kind, scenarioValue, sessionValue);
            return execute(new(
                kind,
                Path.GetFullPath(parse.GetValue(repoRoot) ?? defaultRepoRoot()),
                scenarioValue is null ? null : Path.GetFullPath(scenarioValue),
                sessionValue));
        });
        return command;
    }

    /// <summary>Parses command-line arguments using the supplied default repository root.</summary>
    private static AlvorEyeCommand Parse(IReadOnlyList<string> args, Func<string> defaultRepoRoot)
    {
        AlvorEyeCommand? command = null;
        var root = CreateRootCommand(
            defaultRepoRoot,
            parsed =>
            {
                command = parsed;
                return Task.FromResult(0);
            });
        var result = root.Parse(args.ToArray());
        ThrowIfErrors(result);
        var exitCode = result.InvokeAsync(SilentInvocation()).GetAwaiter().GetResult();
        if (exitCode != 0)
            throw new ArgumentException($"Command exited with code {exitCode}.");

        return command ?? throw new ArgumentException("An AlvorEye command is required.");
    }

    /// <summary>Validates required options for the selected command.</summary>
    private static void Validate(AlvorEyeCommandKind kind, string? scenario, string? session)
    {
        if (kind is AlvorEyeCommandKind.Run or AlvorEyeCommandKind.Session && string.IsNullOrWhiteSpace(scenario))
            throw new ArgumentException($"{kind.ToString().ToLowerInvariant()} requires --scenario.");
        if (kind is AlvorEyeCommandKind.Handoff or AlvorEyeCommandKind.Resume && string.IsNullOrWhiteSpace(session))
            throw new ArgumentException($"{kind.ToString().ToLowerInvariant()} requires --session.");
    }

    /// <summary>Creates an invocation configuration that suppresses generated help output during parse tests.</summary>
    private static InvocationConfiguration SilentInvocation() =>
        new() { Output = TextWriter.Null, Error = TextWriter.Null, EnableDefaultExceptionHandler = false };

    /// <summary>Throws an argument exception when System.CommandLine found parse errors.</summary>
    private static void ThrowIfErrors(ParseResult result)
    {
        if (result.Action is System.CommandLine.Help.HelpAction)
            throw new ArgumentException("Help is generated by the command-line app.");
        if (result.Errors.Count > 0)
            throw new ArgumentException(string.Join(" ", result.Errors.Select(error => error.Message)));
    }
}
