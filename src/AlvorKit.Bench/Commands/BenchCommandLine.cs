namespace AlvorKit;

/// <summary>Parses benchmark commands and validates sample counts before invoking execution.</summary>
public class BenchCommandLine(Func<BenchCommand, int> execute)
{
    /// <summary>Default total measurement count, including discarded warmups.</summary>
    private const int DefaultRunCount = 16;
    /// <summary>Default number of retained measurements.</summary>
    private const int DefaultSampleCount = 5;
    /// <summary>Default measurements discarded before collecting retained samples.</summary>
    private const int DefaultWarmupCount = DefaultRunCount - DefaultSampleCount;

    /// <summary>Runs generated help, parse validation, or the selected command.</summary>
    public int Invoke(string[] args)
    {
        RootCommand root = new("Run and inspect benchmarks.");
        root.Subcommands.Add(CreateListCommand());
        root.Subcommands.Add(CreateRunCommand());
        root.SetAction(parseResult => execute(DefaultRun()));
        return root.Parse(args).Invoke();
    }

    /// <summary>Builds selection-only catalog listing options.</summary>
    private Command CreateListCommand()
    {
        var globs = CreateGlobArgument();
        var excludes = CreateExcludeOption();
        Command command = new("list", "List runnable benchmarks, descriptions, and comparison roles.")
        {
            globs,
            excludes,
        };
        command.SetAction(parseResult => execute(new(
            BenchCommandKind.List,
            parseResult.GetValue(globs) ?? [],
            parseResult.GetValue(excludes) ?? [],
            DefaultWarmupCount,
            DefaultSampleCount,
            null,
            new(false, false, false))));
        return command;
    }

    /// <summary>Builds sampling and export options for benchmark execution.</summary>
    private Command CreateRunCommand()
    {
        var globs = CreateGlobArgument();
        var excludes = CreateExcludeOption();
        var warmup = CreateWarmupOption();
        var samples = CreateSamplesOption();
        var json = CreateJsonOption();
        Option<bool> quiet = new("--quiet") { Description = "Suppress human-readable results." };
        Option<bool> showSamples = new("--show-samples")
        {
            Description = "Print every retained sample instead of only the mean.",
        };
        Option<bool> showMemoryTree = new("--memory-tree")
        {
            Description = "Print each captured unmanaged-memory tree and its retained-sample mean.",
        };
        Command command = new("run", "Run benchmarks matching the supplied glob patterns.")
        {
            globs,
            excludes,
            warmup,
            samples,
            json,
            quiet,
            showSamples,
            showMemoryTree,
        };
        command.SetAction(parseResult => execute(
            new(
                BenchCommandKind.Run,
                parseResult.GetValue(globs) ?? [],
                parseResult.GetValue(excludes) ?? [],
                parseResult.GetValue(warmup),
                parseResult.GetValue(samples),
                parseResult.GetValue(json),
                new(parseResult.GetValue(quiet) || parseResult.GetValue(json) == "-",
                    parseResult.GetValue(showSamples), parseResult.GetValue(showMemoryTree)))));
        return command;
    }

    /// <summary>Accepts zero or more include patterns, unioned by the selector.</summary>
    private Argument<string[]> CreateGlobArgument() => new("glob")
    {
        Arity = ArgumentArity.ZeroOrMore,
        Description = "Benchmark ID glob. *, **, and ? match path characters; multiple globs are unioned.",
    };

    /// <summary>Accepts repeatable exclusion patterns that override includes.</summary>
    private Option<string[]> CreateExcludeOption() => new("--exclude")
    {
        AllowMultipleArgumentsPerToken = false,
        Arity = ArgumentArity.OneOrMore,
        Description = "Exclude a benchmark ID glob; repeat the option to exclude more patterns.",
    };

    /// <summary>Allows nonnegative warmups, including an explicit zero.</summary>
    private Option<int> CreateWarmupOption()
    {
        Option<int> option = new("--warmup")
        {
            DefaultValueFactory = _ => DefaultWarmupCount,
            Description = $"Warmup measurements per benchmark. Default: {DefaultWarmupCount}.",
        };
        option.Validators.Add(result =>
        {
            if (result.GetValue(option) < 0)
                result.AddError("Warmup count must be non-negative.");
        });
        return option;
    }

    /// <summary>Requires at least one retained sample so means are defined.</summary>
    private Option<int> CreateSamplesOption()
    {
        Option<int> option = new("--samples")
        {
            DefaultValueFactory = _ => DefaultSampleCount,
            Description = $"Retained measurements per benchmark. Default: {DefaultSampleCount}.",
        };
        option.Validators.Add(result =>
        {
            if (result.GetValue(option) <= 0)
                result.AddError("Sample count must be positive.");
        });
        return option;
    }

    /// <summary>Selects file export or standard output using a dash.</summary>
    private Option<string> CreateJsonOption() => new("--json")
    {
        Description = "Write results as JSON to a path, or use '-' for standard output.",
    };

    /// <summary>Runs the entire suite with the documented sampling defaults.</summary>
    private BenchCommand DefaultRun() => new(
        BenchCommandKind.Run,
        [],
        [],
        DefaultWarmupCount,
        DefaultSampleCount,
        null,
        new(false, false, false));
}
