namespace AlvorKit.Script.AlvorSense;

/// <summary>Creates the System.CommandLine surface for starting AlvorSense sessions.</summary>
internal static class AlvorSenseStartCommandLine
{
    /// <summary>Creates the start command and its session manifest options.</summary>
    /// <param name="context">Mutable parse context that receives the parsed command.</param>
    /// <returns>The configured command.</returns>
    internal static Command Create(AlvorSenseParseContext context)
    {
        var id = new Option<string>("--id") { Description = "Stable session id used by later commands." };
        var project = new Option<string?>("--project") { Description = "Target game project file to run." };
        var assembly = new Option<string?>("--assembly")
        {
            Description = "Prebuilt managed game assembly to run instead of a project."
        };
        var editableProject = new Option<string?>("--editable-project")
        {
            Description = "Debug project to build, copy immutably, and launch with Source Update enabled."
        };
        var workdir = new Option<string>("--workdir") { Description = "Target working directory. Defaults to the current directory." };
        var environment = AlvorSenseCliOptions.RepeatableTextOption("--env", "Extra NAME=VALUE environment variable.");
        var timeout = AlvorSenseCliOptions.TimeoutOption();
        var command = new Command("start", "Start a persistent AlvorSense session.");
        command.Options.Add(id);
        command.Options.Add(project);
        command.Options.Add(assembly);
        command.Options.Add(editableProject);
        command.Options.Add(workdir);
        command.Options.Add(environment);
        command.Options.Add(timeout);
        command.SetAction(parse =>
        {
            var target = Target(
                parse.GetValue(project),
                parse.GetValue(assembly),
                parse.GetValue(editableProject));
            context.Command = new AlvorSenseStartCommand(
                AlvorSenseCliOptions.ValidateSessionId(
                    parse.GetValue(id)
                    ?? DefaultSessionId(target.Path)),
                target.Project,
                AlvorSenseCliOptions.RequiredText(
                    parse.GetValue(workdir)
                    ?? context.CurrentDirectory,
                    "--workdir"),
                Environment(context.Args),
                AlvorSenseCliOptions.Timeout(
                    parse.GetValue(timeout)),
                target.Assembly,
                target.EditableProject);
        });
        return command;
    }

    /// <summary>Requires exactly one supported target form and validates its path text.</summary>
    private static (
        string? Project,
        string? Assembly,
        string? EditableProject,
        string Path) Target(
        string? project,
        string? assembly,
        string? editableProject)
    {
        if ((project is not null ? 1 : 0) +
            (assembly is not null ? 1 : 0) +
            (editableProject is not null ? 1 : 0) != 1)
        {
            throw new ArgumentException(
                "start requires exactly one of --project, --assembly, or --editable-project.");
        }

        if (project is not null)
        {
            project = AlvorSenseCliOptions.RequiredText(
                project,
                "--project");
            return (project, null, null, project);
        }

        if (assembly is not null)
        {
            assembly = AlvorSenseCliOptions.RequiredText(
                assembly,
                "--assembly");
            return (null, assembly, null, assembly);
        }

        editableProject = AlvorSenseCliOptions.RequiredText(
            editableProject!,
            "--editable-project");
        return (null, null, editableProject, editableProject);
    }

    /// <summary>Builds a stable default session id from the project file name and current UTC time.</summary>
    /// <param name="target">Project or assembly path supplied to the start command.</param>
    /// <returns>A generated session id.</returns>
    private static string DefaultSessionId(string target) =>
        Path.GetFileNameWithoutExtension(target) + "-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

    /// <summary>Parses repeated environment-variable assignments while preserving command-line order.</summary>
    /// <param name="args">Original command-line arguments for the start command.</param>
    /// <returns>Environment variables passed to the hosted game process.</returns>
    private static Dictionary<string, string> Environment(string[] args)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [AlvorSenseEnvironment.AudioSilentVariable] = AlvorSenseEnvironment.EnabledValue
        };
        for (var i = 1; i < args.Length; i++)
        {
            if (args[i] != "--env")
                continue;

            var pair = AlvorSenseCliOptions.OptionValue(args, ref i, "--env");
            var split = pair.IndexOf('=');
            if (split <= 0)
                throw new ArgumentException("--env values must be NAME=VALUE.");

            var name = pair[..split];
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("--env variable names must not be empty.");

            values[name] = pair[(split + 1)..];
        }

        return values;
    }
}
