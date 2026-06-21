namespace AlvorKit.Script.TestTiming;

/// <summary>Separates timing guard options from arbitrary trailing <c>dotnet test</c> arguments.</summary>
/// <param name="TimingArguments">Arguments parsed by System.CommandLine for the timing guard.</param>
/// <param name="ForwardedArguments">Arguments forwarded after <c>dotnet test</c>.</param>
internal sealed record TestTimingCommandLineSplit(string[] TimingArguments, string[] ForwardedArguments)
{
    private static readonly HashSet<string> Flags = ["--warn-only"];
    private static readonly HashSet<string> HelpFlags = ["-h", "-?", "--help"];
    private static readonly HashSet<string> ValuedOptions = ["--max-duration-ms", "--results-directory", "--repo-root", "--trx"];

    /// <summary>Splits a command line at the first forwarded test argument or explicit separator.</summary>
    /// <param name="args">Command-line arguments supplied by the caller.</param>
    /// <returns>The timing and forwarded argument slices.</returns>
    internal static TestTimingCommandLineSplit Create(IReadOnlyList<string> args)
    {
        var timing = new List<string>();
        for (var index = 0; index < args.Count; index++)
        {
            if (args[index] == "--")
                return new([.. timing], [.. args.Skip(index + 1)]);
            if (Flags.Contains(args[index]))
            {
                timing.Add(args[index]);
                continue;
            }
            if (HelpFlags.Contains(args[index]))
            {
                timing.Add(args[index]);
                continue;
            }
            if (IsValuedOption(args[index]))
            {
                timing.Add(args[index]);
                if (!args[index].Contains('=', StringComparison.Ordinal) && index + 1 < args.Count)
                    timing.Add(args[++index]);
                continue;
            }

            return new([.. timing], [.. args.Skip(index)]);
        }

        return new([.. timing], []);
    }

    /// <summary>Returns whether the argument is a timing option that expects a value.</summary>
    private static bool IsValuedOption(string arg)
    {
        if (ValuedOptions.Contains(arg))
            return true;

        var split = arg.IndexOf('=');
        return split > 0 && ValuedOptions.Contains(arg[..split]);
    }
}
