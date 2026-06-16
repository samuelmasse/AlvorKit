namespace AlvorKit.Script.AlvorEye;

/// <summary>Parses command-line arguments for the AlvorEye helper.</summary>
internal static class AlvorEyeCommandParser
{
    /// <summary>Usage text printed for <c>help</c> or <c>--help</c>.</summary>
    public const string HelpText = """
        Usage: dotnet run --project scripts/AlvorKit.Script.AlvorEye -- <command> [options]

        Commands:
          run --scenario <file>      Execute a complete scenario.
          session --scenario <file>  Launch/attach, then read JSONL actions from stdin.
          handoff --session <id>     Freeze a session target and capture the current frame.
          resume --session <id>      Resume a frozen session target.
          help                       Show this help text.

        Options:
          --repo-root <path>         Repository root. Defaults to the current repo.
          -h, --help                 Show this help text.
        """;

    /// <summary>Parses command-line arguments using repository-root discovery for defaults.</summary>
    /// <param name="args">Command-line arguments passed to the process.</param>
    public static AlvorEyeCommand Parse(IReadOnlyList<string> args)
    {
        if (args.Count == 0 || args[0] is "help" or "-h" or "--help")
            return Parse(args, Directory.GetCurrentDirectory());

        var repoRoot = RepoRootArgument(args) ?? ProjectRoot.FindFromCurrentProcess(typeof(AlvorEyeCommandParser));
        return Parse(args, repoRoot);
    }

    /// <summary>Parses command-line arguments with an explicit default repository root for tests.</summary>
    /// <param name="args">Command-line arguments passed to the process.</param>
    /// <param name="defaultRepoRoot">Repository root to use when <c>--repo-root</c> is omitted.</param>
    internal static AlvorEyeCommand Parse(IReadOnlyList<string> args, string defaultRepoRoot)
    {
        if (args.Count == 0 || args[0] is "help" or "-h" or "--help")
            return new(AlvorEyeCommandKind.Help, Path.GetFullPath(defaultRepoRoot), null, null);

        var kind = ParseKind(args[0]);
        var repoRoot = defaultRepoRoot;
        string? scenario = null;
        string? session = null;
        for (var index = 1; index < args.Count; index++)
            ReadOption(args, ref index, ref repoRoot, ref scenario, ref session);

        Validate(kind, scenario, session);
        return new(kind, Path.GetFullPath(repoRoot), scenario is null ? null : Path.GetFullPath(scenario), session);
    }

    /// <summary>Parses the command name into the matching command kind.</summary>
    private static AlvorEyeCommandKind ParseKind(string name) =>
        name switch
        {
            "run" => AlvorEyeCommandKind.Run,
            "session" => AlvorEyeCommandKind.Session,
            "handoff" => AlvorEyeCommandKind.Handoff,
            "resume" => AlvorEyeCommandKind.Resume,
            _ => throw new ArgumentException($"Unknown AlvorEye command '{name}'.")
        };

    /// <summary>Finds an explicit repository root argument without requiring a full parse.</summary>
    private static string? RepoRootArgument(IReadOnlyList<string> args)
    {
        for (var index = 0; index < args.Count - 1; index++)
            if (args[index] == "--repo-root")
                return args[index + 1];
        return null;
    }

    /// <summary>Reads one command-line option and updates the parse state.</summary>
    private static void ReadOption(
        IReadOnlyList<string> args,
        ref int index,
        ref string repoRoot,
        ref string? scenario,
        ref string? session)
    {
        var option = args[index];
        var value = option is "--repo-root" or "--scenario" or "--session" ? ReadValue(args, ref index, option) : null;
        switch (option)
        {
            case "--repo-root":
                repoRoot = value!;
                break;
            case "--scenario":
                scenario = value;
                break;
            case "--session":
                session = value;
                break;
            default:
                throw new ArgumentException($"Unknown AlvorEye option '{option}'.");
        }
    }

    /// <summary>Reads the required value for an option.</summary>
    private static string ReadValue(IReadOnlyList<string> args, ref int index, string option)
    {
        if (++index >= args.Count)
            throw new ArgumentException($"Missing value for {option}.");
        return args[index];
    }

    /// <summary>Validates required options for the selected command.</summary>
    private static void Validate(AlvorEyeCommandKind kind, string? scenario, string? session)
    {
        if (kind is AlvorEyeCommandKind.Run or AlvorEyeCommandKind.Session && string.IsNullOrWhiteSpace(scenario))
            throw new ArgumentException($"{kind.ToString().ToLowerInvariant()} requires --scenario.");
        if (kind is AlvorEyeCommandKind.Handoff or AlvorEyeCommandKind.Resume && string.IsNullOrWhiteSpace(session))
            throw new ArgumentException($"{kind.ToString().ToLowerInvariant()} requires --session.");
    }
}
