namespace AlvorKit.Script.Lint;

/// <summary>Command-line options for the lint coordinator.</summary>
/// <param name="RepoRoot">Absolute or relative repository root to lint.</param>
/// <param name="Fix">True when supported formatters should write changes instead of checking only.</param>
/// <param name="ShowHelp">True when help text should be printed.</param>
/// <param name="IncludePatterns">Repository-relative file, directory, or glob patterns to lint.</param>
internal sealed record LintOptions(string RepoRoot, bool Fix, bool ShowHelp, IReadOnlyList<string> IncludePatterns)
{
    /// <summary>Usage text printed for --help.</summary>
    public const string HelpText = """
        Usage: dotnet run --project scripts/AlvorKit.Script.Lint -- [options]

        Options:
          --fix                 Format supported files instead of checking them.
          --include <pattern>   Lint only matching files. May be repeated.
          --repo-root <path>    Repository root to lint. Defaults to the current repo.
          -h, --help            Show this help text.
        """;

    /// <summary>Parses command-line arguments into validated lint options.</summary>
    public static LintOptions Parse(IReadOnlyList<string> args)
    {
        var repoRoot = RepositoryPaths.FindRoot();
        var fix = false;
        var showHelp = false;
        var includePatterns = new List<string>();

        for (var i = 0; i < args.Count; i++)
        {
            switch (args[i])
            {
                case "--fix":
                    fix = true;
                    break;
                case "-h":
                case "--help":
                    showHelp = true;
                    break;
                case "--repo-root":
                    if (++i >= args.Count)
                        throw new ArgumentException("--repo-root requires a path argument.");
                    repoRoot = Path.GetFullPath(args[i]);
                    break;
                case "--include":
                    if (++i >= args.Count)
                        throw new ArgumentException("--include requires a path or glob argument.");
                    includePatterns.Add(args[i]);
                    break;
                default:
                    throw new ArgumentException($"Unknown lint option '{args[i]}'.");
            }
        }

        return new(repoRoot, fix, showHelp, includePatterns);
    }
}
