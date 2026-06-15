namespace AlvorKit.Script.AgentLease;

/// <summary>Parses command-line arguments for the advisory lease helper.</summary>
internal static partial class AgentLeaseCommandParser
{
    /// <summary>Usage text printed for <c>help</c> or <c>--help</c>.</summary>
    public const string HelpText = """
        Usage: dotnet run --project scripts/AlvorKit.Script.AgentLease -- <command> [options]

        Commands:
          start      Create or replace a lease for current write-oriented work.
          touch      Refresh an existing lease and optionally update its fields.
          list       Show active leases. Add --include-stale to show expired leases.
          check      Check proposed paths for active overlapping leases.
          done       Remove a completed lease.
          conflict   Write a short markdown note for unavoidable overlapping work.

        Options:
          --agent <id>             Agent identifier. Falls back to ALVORKIT_AGENT_ID when set.
          --task <text>            Human-readable task summary.
          --mode <mode>            write, generate, format, test, cleanup, or review.
          --path|--paths <glob>    Repository-relative path, directory, glob, *, or repo-wide. Repeatable.
          --notes <text>           Optional short note stored in the lease.
          --reason <text>          Conflict-note reason.
          --timeout-minutes <n>    Lease lifetime from now. Defaults to 5.
          --repo-root <path>       Repository root. Defaults to the current repo.
          --include-stale          Include expired leases in list output.
          -h, --help               Show this help text.
        """;

    /// <summary>Parses command-line arguments using repository-root discovery for defaults.</summary>
    /// <param name="args">Command-line arguments passed to the process.</param>
    public static AgentLeaseCommand Parse(IReadOnlyList<string> args)
    {
        if (args.Count == 0 || args[0] is "help" or "-h" or "--help")
            return Parse(args, Directory.GetCurrentDirectory());

        var repoRoot = RepoRootArgument(args) ?? ProjectRoot.FindFromCurrentProcess(typeof(AgentLeaseCommandParser));
        return Parse(args, repoRoot);
    }

    /// <summary>Parses command-line arguments with an explicit default repository root for tests.</summary>
    /// <param name="args">Command-line arguments passed to the process.</param>
    /// <param name="defaultRepoRoot">Repository root to use when <c>--repo-root</c> is omitted.</param>
    internal static AgentLeaseCommand Parse(IReadOnlyList<string> args, string defaultRepoRoot)
    {
        if (args.Count == 0 || args[0] is "help" or "-h" or "--help")
            return Help(defaultRepoRoot);

        var kind = ParseKind(args[0]);
        var repoRoot = defaultRepoRoot;
        string? agent = null;
        string? task = null;
        string? mode = null;
        string? notes = null;
        string? reason = null;
        var paths = new List<string>();
        var timeout = AgentLeaseCommand.DefaultTimeout;
        var includeStale = false;

        for (var index = 1; index < args.Count; index++)
            ReadOption(args, ref index, ref repoRoot, ref agent, ref task, ref mode, ref notes, ref reason, paths, ref timeout, ref includeStale);

        Validate(kind, task, mode, paths, reason);
        return new(kind, Path.GetFullPath(repoRoot), agent, task, mode, paths, notes, reason, timeout, includeStale);
    }

    /// <summary>Returns a help command rooted at the supplied path.</summary>
    /// <param name="repoRoot">Repository root or current directory used only to fill the command shape.</param>
    private static AgentLeaseCommand Help(string repoRoot) =>
        new(AgentLeaseCommandKind.Help, Path.GetFullPath(repoRoot), null, null, null, [], null, null, AgentLeaseCommand.DefaultTimeout, false);

    /// <summary>Parses the command name into the matching command kind.</summary>
    /// <param name="name">Command name from the first argument.</param>
    private static AgentLeaseCommandKind ParseKind(string name) =>
        name switch
        {
            "start" => AgentLeaseCommandKind.Start,
            "touch" => AgentLeaseCommandKind.Touch,
            "list" => AgentLeaseCommandKind.List,
            "check" => AgentLeaseCommandKind.Check,
            "done" => AgentLeaseCommandKind.Done,
            "conflict" => AgentLeaseCommandKind.Conflict,
            _ => throw new ArgumentException($"Unknown lease command '{name}'.")
        };

    /// <summary>Reads one command-line option and updates the parse state.</summary>
    private static void ReadOption(
        IReadOnlyList<string> args,
        ref int index,
        ref string repoRoot,
        ref string? agent,
        ref string? task,
        ref string? mode,
        ref string? notes,
        ref string? reason,
        List<string> paths,
        ref TimeSpan timeout,
        ref bool includeStale)
    {
        switch (args[index])
        {
            case "--agent":
                agent = ReadValue(args, ref index);
                break;
            case "--task":
                task = ReadValue(args, ref index);
                break;
            case "--mode":
                mode = ReadValue(args, ref index);
                break;
            case "--path":
            case "--paths":
                paths.Add(ReadValue(args, ref index));
                break;
            case "--notes":
                notes = ReadValue(args, ref index);
                break;
            case "--reason":
                reason = ReadValue(args, ref index);
                break;
            case "--timeout-minutes":
                timeout = TimeSpan.FromMinutes(double.Parse(ReadValue(args, ref index), CultureInfo.InvariantCulture));
                break;
            case "--repo-root":
                repoRoot = ReadValue(args, ref index);
                break;
            case "--include-stale":
                includeStale = true;
                break;
            case "-h":
            case "--help":
                throw new ArgumentException("Use 'help' as the command to show lease helper usage.");
            default:
                throw new ArgumentException($"Unknown lease option '{args[index]}'.");
        }
    }

}
