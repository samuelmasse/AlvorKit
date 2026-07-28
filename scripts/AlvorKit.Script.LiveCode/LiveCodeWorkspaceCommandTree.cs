namespace AlvorKit.Script.LiveCode;

/// <summary>Builds the local workspace lifecycle commands shared by LiveCode and future agent façades.</summary>
internal static class LiveCodeWorkspaceCommandTree
{
    internal static Command Create(LiveCodeWorkspaceCli cli)
    {
        var command = new Command(
            "workspace",
            "Create and audit an ignored combined AlvorSense/LiveCode agent workspace.");
        command.Subcommands.Add(Init(cli));
        command.Subcommands.Add(Status(cli));
        command.Subcommands.Add(AssociateSense(cli));
        command.Subcommands.Add(AddIntervention(cli));
        command.Subcommands.Add(ResolveIntervention(cli));
        command.Subcommands.Add(Close(cli));
        return command;
    }

    private static Command Init(LiveCodeWorkspaceCli cli)
    {
        var id = Required("--id", "Safe workspace id created beneath tmp/live.");
        var purpose = Required("--purpose", "Short description of the investigation.");
        var session = Required("--session", "Exact LiveCode session id or display name.");
        var sense = new Option<string?>("--alvorsense")
        {
            Description = "Optional associated AlvorSense session id."
        };
        var discovery = Discovery();
        var command = new Command(
            "init",
            "Resolve one live process, capture its baseline, and create the session workspace.");
        command.Options.Add(id);
        command.Options.Add(purpose);
        command.Options.Add(session);
        command.Options.Add(sense);
        command.Options.Add(discovery);
        command.SetAction(parse => cli.Init(
            parse.GetRequiredValue(id),
            parse.GetRequiredValue(purpose),
            parse.GetRequiredValue(session),
            parse.GetValue(sense),
            parse.GetValue(discovery)));
        return command;
    }

    private static Command Status(LiveCodeWorkspaceCli cli)
    {
        var workspace = Workspace();
        var discovery = Discovery();
        var command = new Command(
            "status",
            "Read local cleanup state and verify the exact LiveCode process is still discoverable.");
        command.Options.Add(workspace);
        command.Options.Add(discovery);
        command.SetAction(parse => cli.Status(
            parse.GetRequiredValue(workspace),
            parse.GetValue(discovery)));
        return command;
    }

    private static Command AssociateSense(LiveCodeWorkspaceCli cli)
    {
        var workspace = Workspace();
        var sense = Required("--alvorsense", "Exact AlvorSense session id.");
        var command = new Command(
            "associate-sense",
            "Associate the user-visible AlvorSense session used to verify this investigation.");
        command.Options.Add(workspace);
        command.Options.Add(sense);
        command.SetAction(parse => cli.AssociateSense(
            parse.GetRequiredValue(workspace),
            parse.GetRequiredValue(sense)));
        return command;
    }

    private static Command AddIntervention(LiveCodeWorkspaceCli cli)
    {
        var workspace = Workspace();
        var id = Required("--id", "Stable intervention id used by cleanup.");
        var kind = Required("--kind", "livecode, livepatch, or bridge.");
        var description = Required("--description", "Observable persistent effect.");
        var state = new Option<string>("--state")
        {
            Description = "active or restart-required.",
            DefaultValueFactory = _ => "active"
        };
        var runtime = new Option<string?>("--runtime-id")
        {
            Description = "Optional runtime patch or operation id."
        };
        var source = new Option<string?>("--source")
        {
            Description = "Optional source file responsible for the effect."
        };
        var cleanup = new Option<string?>("--cleanup")
        {
            Description = "Exact cleanup operation or restart requirement."
        };
        var command = new Command(
            "add-intervention",
            "Track a persistent effect that must be resolved before closing the workspace.");
        command.Options.Add(workspace);
        command.Options.Add(id);
        command.Options.Add(kind);
        command.Options.Add(description);
        command.Options.Add(state);
        command.Options.Add(runtime);
        command.Options.Add(source);
        command.Options.Add(cleanup);
        command.SetAction(parse => cli.AddIntervention(
            parse.GetRequiredValue(workspace),
            parse.GetRequiredValue(id),
            parse.GetRequiredValue(kind),
            parse.GetRequiredValue(description),
            parse.GetValue(state)!,
            parse.GetValue(runtime),
            parse.GetValue(source),
            parse.GetValue(cleanup)));
        return command;
    }

    private static Command ResolveIntervention(LiveCodeWorkspaceCli cli)
    {
        var workspace = Workspace();
        var id = Required("--id", "Tracked intervention id.");
        var command = new Command(
            "resolve-intervention",
            "Mark an intervention resolved after observing its cleanup.");
        command.Options.Add(workspace);
        command.Options.Add(id);
        command.SetAction(parse => cli.ResolveIntervention(
            parse.GetRequiredValue(workspace),
            parse.GetRequiredValue(id)));
        return command;
    }

    private static Command Close(LiveCodeWorkspaceCli cli)
    {
        var workspace = Workspace();
        var command = new Command(
            "close",
            "Close an audited workspace; unresolved interventions reject the operation.");
        command.Options.Add(workspace);
        command.SetAction(parse => cli.Close(parse.GetRequiredValue(workspace)));
        return command;
    }

    private static Option<string> Workspace() =>
        Required("--workspace", "Workspace id beneath tmp/live or explicit workspace path.");

    private static Option<string> Required(string name, string description) =>
        new(name) { Description = description, Required = true };

    private static Option<string?> Discovery() =>
        new("--discovery-dir")
        {
            Description = "Override the default per-user LiveCode discovery directory."
        };
}
