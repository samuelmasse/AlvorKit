namespace AlvorKit.Script.LiveCode;

/// <summary>Builds the generated System.CommandLine surface for LiveCode operations.</summary>
internal static class LiveCodeCommandTree
{
    /// <summary>Creates the complete LiveCode command tree.</summary>
    internal static RootCommand Create(
        LiveCodeCli cli,
        LivePatchCli patches,
        LiveCodeWorkspaceCli workspaces)
    {
        var root = new RootCommand(
            "Inspect and interact with explicitly enabled AlvorKit development processes through scoped C# or predefined bridges.");
        root.Subcommands.Add(LiveCodeWorkspaceCommandTree.Create(workspaces));
        root.Subcommands.Add(List(cli));
        root.Subcommands.Add(Graph(cli));
        root.Subcommands.Add(Bridges(cli));
        root.Subcommands.Add(Bridge(cli));
        root.Subcommands.Add(Puppet(cli));
        root.Subcommands.Add(Execute(cli));
        root.Subcommands.Add(Frozen(cli));
        root.Subcommands.Add(LivePatchCommandTree.Create(patches));
        return root;
    }

    private static Command Frozen(LiveCodeCli cli)
    {
        var command = new Command(
            "frozen",
            "Inspect a game whose frame heartbeat stalled through its dedicated out-of-band lane.");
        command.Subcommands.Add(FrozenStatus(cli));
        command.Subcommands.Add(FrozenExecute(cli));
        return command;
    }

    private static Command FrozenStatus(LiveCodeCli cli)
    {
        var session = RequiredOption("--session", "Exact session id or display name.");
        var discovery = DiscoveryOption();
        var workspace = WorkspaceOption();
        var command = new Command(
            "status",
            "Read frame-heartbeat and frozen-inspector state without using the game loop.");
        command.Options.Add(session);
        command.Options.Add(discovery);
        command.Options.Add(workspace);
        command.SetAction(parse => cli.FrozenStatus(
            parse.GetRequiredValue(session),
            parse.GetValue(discovery),
            parse.GetValue(workspace)));
        return command;
    }

    private static Command FrozenExecute(LiveCodeCli cli)
    {
        var session = RequiredOption("--session", "Exact session id or display name.");
        var scope = RequiredOption(
            "--scope",
            "Scope id, exact diagnostic label, or exact scope type name.");
        var file = SourceFileOption();
        var discovery = DiscoveryOption();
        var workspace = WorkspaceOption();
        var command = new Command(
            "exec",
            "Compile an ordinary ILiveCodeCommand and run it out of band only after game frames stall.");
        command.Options.Add(session);
        command.Options.Add(scope);
        command.Options.Add(file);
        command.Options.Add(discovery);
        command.Options.Add(workspace);
        command.SetAction(parse => cli.FrozenExecute(
            parse.GetRequiredValue(session),
            parse.GetRequiredValue(scope),
            parse.GetValue(file),
            parse.GetValue(discovery),
            parse.GetValue(workspace)));
        return command;
    }

    private static Command Bridges(LiveCodeCli cli)
    {
        var session = RequiredOption("--session", "Exact session id or display name.");
        var discovery = DiscoveryOption();
        var workspace = WorkspaceOption();
        var command = new Command(
            "bridges",
            "Discover the target's predefined bridge contracts and JSON schemas.");
        command.Options.Add(session);
        command.Options.Add(discovery);
        command.Options.Add(workspace);
        command.SetAction(parse => cli.Bridges(
            parse.GetRequiredValue(session),
            parse.GetValue(discovery),
            parse.GetValue(workspace)));
        return command;
    }

    private static Command Bridge(LiveCodeCli cli)
    {
        var session = RequiredOption("--session", "Exact session id or display name.");
        var name = RequiredOption("--name", "Exact registered bridge name.");
        var version = new Option<int>("--version")
        {
            Description = "Expected bridge version. Zero accepts the advertised current version.",
            DefaultValueFactory = _ => 0
        };
        var file = new Option<string?>("--file")
        {
            Description = "JSON payload file. When omitted, JSON is read from standard input."
        };
        var discovery = DiscoveryOption();
        var workspace = WorkspaceOption();
        var command = new Command(
            "bridge",
            "Invoke a game-thread predefined bridge with a JSON payload.");
        command.Options.Add(session);
        command.Options.Add(name);
        command.Options.Add(version);
        command.Options.Add(file);
        command.Options.Add(discovery);
        command.Options.Add(workspace);
        command.SetAction(parse => cli.Bridge(
            parse.GetRequiredValue(session),
            parse.GetRequiredValue(name),
            parse.GetValue(version),
            parse.GetValue(file),
            parse.GetValue(discovery),
            parse.GetValue(workspace)));
        return command;
    }

    private static Command Puppet(LiveCodeCli cli)
    {
        var session = RequiredOption("--session", "Exact session id or display name.");
        var file = new Option<string?>("--file")
        {
            Description = "AlvorSense command file. When omitted, commands are read from standard input."
        };
        var discovery = DiscoveryOption();
        var workspace = WorkspaceOption();
        var command = new Command(
            "puppet",
            "Run an atomic AlvorSense command batch under an exclusive input reservation.");
        command.Options.Add(session);
        command.Options.Add(file);
        command.Options.Add(discovery);
        command.Options.Add(workspace);
        command.SetAction(parse => cli.Puppet(
            parse.GetRequiredValue(session),
            parse.GetValue(file),
            parse.GetValue(discovery),
            parse.GetValue(workspace)));
        return command;
    }

    private static Command List(LiveCodeCli cli)
    {
        var discovery = DiscoveryOption();
        var command = new Command("list", "List running LiveCode development sessions.");
        command.Options.Add(discovery);
        command.SetAction(parse => cli.List(parse.GetValue(discovery)));
        return command;
    }

    private static Command Graph(LiveCodeCli cli)
    {
        var session = RequiredOption("--session", "Exact session id or display name.");
        var discovery = DiscoveryOption();
        var workspace = WorkspaceOption();
        var command = new Command("graph", "Read the target's current tracked injector scope graph.");
        command.Options.Add(session);
        command.Options.Add(discovery);
        command.Options.Add(workspace);
        command.SetAction(parse => cli.Graph(
            parse.GetRequiredValue(session),
            parse.GetValue(discovery),
            parse.GetValue(workspace)));
        return command;
    }

    private static Command Execute(LiveCodeCli cli)
    {
        var session = RequiredOption("--session", "Exact session id or display name.");
        var scope = RequiredOption(
            "--scope",
            "Scope id, exact diagnostic label, or exact scope type name.");
        var file = SourceFileOption();
        var discovery = DiscoveryOption();
        var workspace = WorkspaceOption();
        var command = new Command(
            "exec",
            "Compile C# and execute its ILiveCodeCommand in an exact active scope.");
        command.Options.Add(session);
        command.Options.Add(scope);
        command.Options.Add(file);
        command.Options.Add(discovery);
        command.Options.Add(workspace);
        command.SetAction(parse => cli.Execute(
            parse.GetRequiredValue(session),
            parse.GetRequiredValue(scope),
            parse.GetValue(file),
            parse.GetValue(discovery),
            parse.GetValue(workspace)));
        return command;
    }

    private static Option<string> RequiredOption(string name, string description) =>
        new(name) { Description = description, Required = true };

    private static Option<string?> DiscoveryOption() =>
        new("--discovery-dir")
        {
            Description = "Override the default per-user LiveCode discovery directory."
        };

    private static Option<string?> SourceFileOption() =>
        new("--file")
        {
            Description = "C# source file. When omitted, source is read from standard input."
        };

    private static Option<string?> WorkspaceOption() =>
        new("--workspace")
        {
            Description = "Live workspace id beneath tmp/live or explicit path; records exact request and result."
        };
}
