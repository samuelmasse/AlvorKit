namespace AlvorKit.Script.BindgenReview;

/// <summary>Validation and shared helpers for the bindgen review command parser.</summary>
internal static partial class BindgenReviewCommandParser
{
    /// <summary>Reads one command-line option and updates the parse state.</summary>
    private static void ReadOption(
        IReadOnlyList<string> args,
        ref int index,
        ref string repoRoot,
        List<string> positionals,
        ref string? caseName,
        ref bool keep)
    {
        switch (args[index])
        {
            case "--case":
                caseName = ReadValue(args, ref index);
                break;
            case "--repo-root":
                repoRoot = ReadValue(args, ref index);
                break;
            case "--keep":
                keep = true;
                break;
            case "-h":
            case "--help":
                throw new ArgumentException("Use 'help' as the command to show bindgen review usage.");
            default:
                if (args[index].StartsWith("--", StringComparison.Ordinal))
                    throw new ArgumentException($"Unknown bindgen review option '{args[index]}'.");

                positionals.Add(args[index]);
                break;
        }
    }

    /// <summary>Validates parsed options against command-specific requirements.</summary>
    private static void Validate(
        BindgenReviewCommandKind kind,
        IReadOnlyList<string> positionals,
        string? caseName,
        bool keep)
    {
        if (positionals.Count != 1)
            throw new ArgumentException($"{kind.ToString().ToLowerInvariant()} requires exactly one positional argument.");
        if (positionals.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("Positional arguments must not be blank.");
        if (caseName is not null && kind != BindgenReviewCommandKind.Start)
            throw new ArgumentException("--case is only valid for start.");
        if (keep && kind != BindgenReviewCommandKind.Finish)
            throw new ArgumentException("--keep is only valid for finish.");
    }

    /// <summary>Reads the value following an option name.</summary>
    /// <param name="args">Command-line arguments.</param>
    /// <param name="index">Index of the current option, advanced to the value.</param>
    private static string ReadValue(IReadOnlyList<string> args, ref int index)
    {
        if (++index >= args.Count)
            throw new ArgumentException($"Missing value for '{args[index - 1]}'.");

        return args[index];
    }

    /// <summary>Finds an explicit repository root before normal root discovery is needed.</summary>
    /// <param name="args">Command-line arguments.</param>
    private static string? RepoRootArgument(IReadOnlyList<string> args)
    {
        for (var index = 0; index < args.Count - 1; index++)
        {
            if (args[index] == "--repo-root")
                return args[index + 1];
        }

        return null;
    }
}
