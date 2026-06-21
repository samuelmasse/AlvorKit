namespace AlvorKit.Script.AgentLease;

/// <summary>Parses command-line arguments for the advisory lease helper.</summary>
internal static partial class AgentLeaseCommandParser
{
    /// <summary>Parses command-line arguments using repository-root discovery for defaults.</summary>
    /// <param name="args">Command-line arguments passed to the process.</param>
    public static AgentLeaseCommand Parse(IReadOnlyList<string> args) =>
        Parse(args, () => ProjectRoot.FindFromCurrentProcess(typeof(AgentLeaseCommandParser)));

    /// <summary>Parses command-line arguments with an explicit default repository root for tests.</summary>
    /// <param name="args">Command-line arguments passed to the process.</param>
    /// <param name="defaultRepoRoot">Repository root to use when <c>--repo-root</c> is omitted.</param>
    internal static AgentLeaseCommand Parse(IReadOnlyList<string> args, string defaultRepoRoot) =>
        Parse(args, () => defaultRepoRoot);

    /// <summary>Creates the command tree for the lease helper.</summary>
    /// <param name="defaultRepoRoot">Repository root provider used when <c>--repo-root</c> is omitted.</param>
    /// <param name="execute">Action that executes the parsed command.</param>
    internal static RootCommand CreateRootCommand(Func<string> defaultRepoRoot, Func<AgentLeaseCommand, Task<int>> execute)
    {
        var root = new RootCommand("Advisory agent lease helper.");
        root.Subcommands.Add(CreateCommand("start", "Create or replace a lease.", AgentLeaseCommandKind.Start, defaultRepoRoot, execute));
        root.Subcommands.Add(CreateCommand("touch", "Refresh or update a lease.", AgentLeaseCommandKind.Touch, defaultRepoRoot, execute));
        root.Subcommands.Add(CreateCommand("list", "Show active leases.", AgentLeaseCommandKind.List, defaultRepoRoot, execute));
        root.Subcommands.Add(CreateCommand("check", "Check proposed paths for overlaps.", AgentLeaseCommandKind.Check, defaultRepoRoot, execute));
        root.Subcommands.Add(CreateCommand("done", "Remove a completed lease.", AgentLeaseCommandKind.Done, defaultRepoRoot, execute));
        root.Subcommands.Add(CreateCommand("conflict", "Write an overlap conflict note.", AgentLeaseCommandKind.Conflict, defaultRepoRoot, execute));
        return root;
    }

    /// <summary>Creates a lease command with the shared option surface.</summary>
    private static Command CreateCommand(
        string name,
        string description,
        AgentLeaseCommandKind kind,
        Func<string> defaultRepoRoot,
        Func<AgentLeaseCommand, Task<int>> execute)
    {
        var agent = new Option<string>("--agent") { Description = "Agent identifier." };
        var task = new Option<string>("--task") { Description = "Human-readable task summary." };
        var mode = new Option<string>("--mode") { Description = "Lease coordination mode." };
        var paths = new Option<string[]>("--path", "--paths") { Description = "Repository-relative path or glob." };
        var notes = new Option<string>("--notes") { Description = "Optional short lease note." };
        var reason = new Option<string>("--reason") { Description = "Conflict-note reason." };
        var timeoutMinutes = new Option<string>("--timeout-minutes") { Description = "Lease lifetime in minutes." };
        var repoRoot = new Option<string>("--repo-root") { Description = "Repository root." };
        var includeStale = new Option<bool>("--include-stale") { Description = "Include expired leases in list output." };
        var command = new Command(name, description);
        command.Options.Add(agent);
        command.Options.Add(task);
        command.Options.Add(mode);
        command.Options.Add(paths);
        command.Options.Add(notes);
        command.Options.Add(reason);
        command.Options.Add(timeoutMinutes);
        command.Options.Add(repoRoot);
        command.Options.Add(includeStale);
        command.SetAction(parse =>
        {
            var parsed = new AgentLeaseCommand(
                kind,
                Path.GetFullPath(parse.GetValue(repoRoot) ?? defaultRepoRoot()),
                parse.GetValue(agent),
                parse.GetValue(task),
                parse.GetValue(mode),
                Paths(parse.GetValue(paths)),
                parse.GetValue(notes),
                parse.GetValue(reason),
                Timeout(parse.GetValue(timeoutMinutes)),
                parse.GetValue(includeStale));
            Validate(parsed.Kind, parsed.TaskDescription, parsed.Mode, parsed.Paths, parsed.Reason);
            return execute(parsed);
        });
        return command;
    }

    /// <summary>Parses command-line arguments using the supplied default repository root.</summary>
    private static AgentLeaseCommand Parse(IReadOnlyList<string> args, Func<string> defaultRepoRoot)
    {
        AgentLeaseCommand? command = null;
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

        return command ?? throw new ArgumentException("A lease command is required.");
    }

    /// <summary>Normalizes optional path values from the command line.</summary>
    private static IReadOnlyList<string> Paths(string[]? values) =>
        values ?? [];

    /// <summary>Parses the optional timeout override.</summary>
    private static TimeSpan Timeout(string? value)
    {
        if (value is null)
            return AgentLeaseCommand.DefaultTimeout;

        if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var minutes))
            throw new ArgumentException("--timeout-minutes must be a number.");

        return TimeSpan.FromMinutes(minutes);
    }

    /// <summary>Creates an invocation configuration that suppresses generated help output during parse tests.</summary>
    private static InvocationConfiguration SilentInvocation() =>
        new() { Output = TextWriter.Null, Error = TextWriter.Null, EnableDefaultExceptionHandler = false };
}
