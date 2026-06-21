namespace AlvorKit.Script.BindgenReview;

/// <summary>Parses command-line arguments for the bindgen review helper.</summary>
internal static partial class BindgenReviewCommandParser
{
    /// <summary>Parses command-line arguments using repository-root discovery for defaults.</summary>
    /// <param name="args">Command-line arguments passed to the process.</param>
    public static BindgenReviewCommand Parse(IReadOnlyList<string> args) =>
        Parse(args, () => ProjectRoot.FindFromCurrentProcess(typeof(BindgenReviewCommandParser)));

    /// <summary>Parses command-line arguments with an explicit default repository root for tests.</summary>
    /// <param name="args">Command-line arguments passed to the process.</param>
    /// <param name="defaultRepoRoot">Repository root to use when <c>--repo-root</c> is omitted.</param>
    internal static BindgenReviewCommand Parse(IReadOnlyList<string> args, string defaultRepoRoot) =>
        Parse(args, () => defaultRepoRoot);

    /// <summary>Creates the command tree for the bindgen review helper.</summary>
    /// <param name="defaultRepoRoot">Repository root provider used when <c>--repo-root</c> is omitted.</param>
    /// <param name="execute">Action that executes the parsed command.</param>
    internal static RootCommand CreateRootCommand(Func<string> defaultRepoRoot, Func<BindgenReviewCommand, Task<int>> execute)
    {
        var root = new RootCommand("Generated binding review helper.");
        root.Subcommands.Add(CreateCommand("start", "Create a review directory and generate before.", BindgenReviewCommandKind.Start, defaultRepoRoot, execute));
        root.Subcommands.Add(CreateCommand("after", "Generate after for an existing review.", BindgenReviewCommandKind.After, defaultRepoRoot, execute));
        root.Subcommands.Add(CreateCommand("diff", "Print a before/after diff.", BindgenReviewCommandKind.Diff, defaultRepoRoot, execute));
        root.Subcommands.Add(CreateCommand("clean", "Delete an existing review directory.", BindgenReviewCommandKind.Clean, defaultRepoRoot, execute));
        root.Subcommands.Add(CreateCommand("finish", "Generate after, print diff, and clean.", BindgenReviewCommandKind.Finish, defaultRepoRoot, execute));
        return root;
    }

    /// <summary>Creates one bindgen review command with shared options and one positional argument.</summary>
    private static Command CreateCommand(
        string name,
        string description,
        BindgenReviewCommandKind kind,
        Func<string> defaultRepoRoot,
        Func<BindgenReviewCommand, Task<int>> execute)
    {
        var positional = new Argument<string>("target") { Description = "Library or review root." };
        var caseName = new Option<string>("--case") { Description = "Human-readable case name." };
        var repoRoot = new Option<string>("--repo-root") { Description = "Repository root." };
        var keep = new Option<bool>("--keep") { Description = "Keep review output after finish." };
        var command = new Command(name, description);
        command.Arguments.Add(positional);
        command.Options.Add(caseName);
        command.Options.Add(repoRoot);
        command.Options.Add(keep);
        command.SetAction(parse =>
        {
            var targetValue = parse.GetRequiredValue(positional);
            var caseValue = parse.GetValue(caseName);
            var keepValue = parse.GetValue(keep);
            Validate(kind, [targetValue], caseValue, keepValue);
            return execute(new(
                kind,
                Path.GetFullPath(parse.GetValue(repoRoot) ?? defaultRepoRoot()),
                kind == BindgenReviewCommandKind.Start ? targetValue : null,
                kind == BindgenReviewCommandKind.Start ? null : targetValue,
                caseValue,
                keepValue));
        });
        return command;
    }

    /// <summary>Parses command-line arguments using the supplied default repository root.</summary>
    private static BindgenReviewCommand Parse(IReadOnlyList<string> args, Func<string> defaultRepoRoot)
    {
        BindgenReviewCommand? command = null;
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

        return command ?? throw new ArgumentException("A bindgen review command is required.");
    }

    /// <summary>Creates an invocation configuration that suppresses generated help output during parse tests.</summary>
    private static InvocationConfiguration SilentInvocation() =>
        new() { Output = TextWriter.Null, Error = TextWriter.Null, EnableDefaultExceptionHandler = false };
}
