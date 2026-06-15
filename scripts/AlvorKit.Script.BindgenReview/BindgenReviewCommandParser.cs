using AlvorKit.Script.Workspace;

namespace AlvorKit.Script.BindgenReview;

/// <summary>Parses command-line arguments for the bindgen review helper.</summary>
internal static partial class BindgenReviewCommandParser
{
    /// <summary>Usage text printed for <c>help</c> or <c>--help</c>.</summary>
    public const string HelpText = """
        Usage: dotnet run --project scripts/AlvorKit.Script.BindgenReview -- <command> [options]

        Commands:
          start <library>      Create a unique review directory and generate before.
          after <review-root>  Generate after for an existing review directory.
          diff <review-root>   Print git diff --no-index for before and after.
          clean <review-root>  Delete an existing review directory.
          finish <review-root> Generate after, print the diff, and delete the review directory.

        Options:
          --case <name>        Human-readable case name for start. Defaults to the library.
          --keep               Keep the review directory after finish prints the diff.
          --repo-root <path>   Repository root. Defaults to the current repo.
          -h, --help           Show this help text.
        """;

    /// <summary>Parses command-line arguments using repository-root discovery for defaults.</summary>
    /// <param name="args">Command-line arguments passed to the process.</param>
    public static BindgenReviewCommand Parse(IReadOnlyList<string> args)
    {
        if (args.Count == 0 || args[0] is "help" or "-h" or "--help")
            return Parse(args, Directory.GetCurrentDirectory());

        var repoRoot = RepoRootArgument(args) ?? ProjectRoot.FindFromCurrentProcess(typeof(BindgenReviewCommandParser));
        return Parse(args, repoRoot);
    }

    /// <summary>Parses command-line arguments with an explicit default repository root for tests.</summary>
    /// <param name="args">Command-line arguments passed to the process.</param>
    /// <param name="defaultRepoRoot">Repository root to use when <c>--repo-root</c> is omitted.</param>
    internal static BindgenReviewCommand Parse(IReadOnlyList<string> args, string defaultRepoRoot)
    {
        if (args.Count == 0 || args[0] is "help" or "-h" or "--help")
            return Help(defaultRepoRoot);

        var kind = ParseKind(args[0]);
        var repoRoot = defaultRepoRoot;
        var positionals = new List<string>();
        string? caseName = null;
        var keep = false;

        for (var index = 1; index < args.Count; index++)
            ReadOption(args, ref index, ref repoRoot, positionals, ref caseName, ref keep);

        Validate(kind, positionals, caseName, keep);
        var library = kind == BindgenReviewCommandKind.Start ? positionals[0] : null;
        var reviewRoot = kind == BindgenReviewCommandKind.Start ? null : positionals[0];
        return new(kind, Path.GetFullPath(repoRoot), library, reviewRoot, caseName, keep);
    }

    /// <summary>Returns a help command rooted at the supplied path.</summary>
    /// <param name="repoRoot">Repository root or current directory used only to fill the command shape.</param>
    private static BindgenReviewCommand Help(string repoRoot) =>
        new(BindgenReviewCommandKind.Help, Path.GetFullPath(repoRoot), null, null, null, false);

    /// <summary>Parses the command name into the matching command kind.</summary>
    /// <param name="name">Command name from the first argument.</param>
    private static BindgenReviewCommandKind ParseKind(string name) =>
        name switch
        {
            "start" => BindgenReviewCommandKind.Start,
            "after" => BindgenReviewCommandKind.After,
            "diff" => BindgenReviewCommandKind.Diff,
            "clean" => BindgenReviewCommandKind.Clean,
            "finish" => BindgenReviewCommandKind.Finish,
            _ => throw new ArgumentException($"Unknown bindgen review command '{name}'.")
        };
}
