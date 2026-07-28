namespace AlvorKit.Script.TestInterception;

/// <summary>Creates and parses the interception child-launcher command line.</summary>
internal sealed class InterceptionOptionsParser
{
    /// <summary>Creates the executable command surface.</summary>
    internal static RootCommand CreateRootCommand(
        IReadOnlyList<string> childArguments,
        Func<InterceptionLaunchOptions, Task<int>> execute)
    {
        var options = CreateOptions();
        var command = new RootCommand(
            "Launches one child with the AlvorKit CoreCLR interception profiler.");
        AddOptions(command, options);
        command.SetAction(parse => execute(ToOptions(parse, options, childArguments)));
        return command;
    }

    /// <summary>Parses launcher arguments for focused tests.</summary>
    internal InterceptionLaunchOptions Parse(
        IReadOnlyList<string> arguments,
        IReadOnlyList<string>? childArguments = null)
    {
        var options = CreateOptions();
        var command = new RootCommand();
        AddOptions(command, options);
        var result = command.Parse([.. arguments]);
        if (result.Errors.Count > 0)
        {
            throw new ArgumentException(
                string.Join(" ", result.Errors.Select(error => error.Message)));
        }

        return ToOptions(result, options, childArguments ?? []);
    }

    private static (
        Option<string?> TestProject,
        Option<string?> ExecutableProject,
        Option<string?> Configuration,
        Option<string?> Filter,
        Option<string?> ProfilerPath,
        Option<string[]> Modules,
        Option<string?> TimeoutSeconds,
        Option<string?> RepositoryRoot) CreateOptions() =>
        (
            new("--test-project") { Description = "Project launched by dotnet test." },
            new("--exec-project") { Description = "Project launched by dotnet run." },
            new("--configuration", "-c") { Description = "Child build configuration." },
            new("--filter") { Description = "Optional dotnet test filter." },
            new("--profiler-path") { Description = "Exact first-party profiler library path." },
            new("--module") { Description = "Allowed managed module name." },
            new("--timeout-seconds") { Description = "Hard child-process timeout." },
            new("--repo-root") { Description = "Repository root used as the child working directory." });

    private static void AddOptions(
        RootCommand command,
        (
            Option<string?> TestProject,
            Option<string?> ExecutableProject,
            Option<string?> Configuration,
            Option<string?> Filter,
            Option<string?> ProfilerPath,
            Option<string[]> Modules,
            Option<string?> TimeoutSeconds,
            Option<string?> RepositoryRoot) options)
    {
        command.Options.Add(options.TestProject);
        command.Options.Add(options.ExecutableProject);
        command.Options.Add(options.Configuration);
        command.Options.Add(options.Filter);
        command.Options.Add(options.ProfilerPath);
        command.Options.Add(options.Modules);
        command.Options.Add(options.TimeoutSeconds);
        command.Options.Add(options.RepositoryRoot);
    }

    private static InterceptionLaunchOptions ToOptions(
        ParseResult parse,
        (
            Option<string?> TestProject,
            Option<string?> ExecutableProject,
            Option<string?> Configuration,
            Option<string?> Filter,
            Option<string?> ProfilerPath,
            Option<string[]> Modules,
            Option<string?> TimeoutSeconds,
            Option<string?> RepositoryRoot) options,
        IReadOnlyList<string> childArguments)
    {
        var testProject = parse.GetValue(options.TestProject);
        var executableProject = parse.GetValue(options.ExecutableProject);
        if ((testProject is null) == (executableProject is null))
        {
            throw new ArgumentException(
                "Specify exactly one of --test-project or --exec-project.");
        }

        var filter = parse.GetValue(options.Filter);
        if (filter is not null && testProject is null)
            throw new ArgumentException("--filter requires --test-project.");

        var timeout = parse.GetValue(options.TimeoutSeconds) is { } timeoutText
            ? TimeSpan.FromSeconds(
                double.Parse(timeoutText, CultureInfo.InvariantCulture))
            : TimeSpan.FromMinutes(5);
        if (timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout));

        var root = Path.GetFullPath(
            parse.GetValue(options.RepositoryRoot) ??
            Environment.CurrentDirectory);
        return new(
            root,
            testProject,
            executableProject,
            parse.GetValue(options.Configuration) ?? "Debug",
            filter,
            parse.GetValue(options.ProfilerPath),
            parse.GetValue(options.Modules) ?? [],
            timeout,
            childArguments);
    }
}
