namespace AlvorKit.Script.LiveCode;

/// <summary>Builds the source-file update command surface.</summary>
internal static class SourceUpdateCommandTree
{
    internal static Command Create(SourceUpdateCli cli)
    {
        var command = new Command(
            "source",
            "Compile an immutable diff to the original C# file and apply its method body to the running module.");
        command.Subcommands.Add(Start(cli));
        command.Subcommands.Add(Apply(cli));
        command.Subcommands.Add(Status(cli));
        command.Subcommands.Add(Stop(cli));
        command.Subcommands.Add(Coordinator(cli));
        return command;
    }

    private static Command Start(SourceUpdateCli cli)
    {
        var workspace = Required("--workspace", "Live workspace id or path.");
        var launch = new Option<string?>("--launch")
        {
            Description = "Editable launch manifest; inferred from the associated AlvorSense session."
        };
        var discovery = Discovery();
        var command = new Command("start", "Start the retained compiler generation owner.");
        command.Options.Add(workspace);
        command.Options.Add(launch);
        command.Options.Add(discovery);
        command.SetAction(parse => cli.Start(
            parse.GetRequiredValue(workspace),
            parse.GetValue(launch),
            parse.GetValue(discovery)));
        return command;
    }

    private static Command Apply(SourceUpdateCli cli)
    {
        var workspace = Required("--workspace", "Live workspace id or path.");
        var source = Required("--source", "Original project .cs file already edited to its desired contents.");
        var diff = Required("--diff", "Unified diff from the acknowledged source to the current file.");
        var updateId = new Option<string?>("--update-id")
        {
            Description = "Optional stable update id; a timestamped id is generated when omitted."
        };
        var command = new Command(
            "apply",
            "Validate, compile, immutably record, and queue exactly one existing method-body edit.");
        command.Options.Add(workspace);
        command.Options.Add(source);
        command.Options.Add(diff);
        command.Options.Add(updateId);
        command.SetAction(parse => cli.Apply(
            parse.GetRequiredValue(workspace),
            parse.GetRequiredValue(source),
            parse.GetRequiredValue(diff),
            parse.GetValue(updateId)));
        return command;
    }

    private static Command Status(SourceUpdateCli cli)
    {
        var workspace = Required("--workspace", "Live workspace id or path.");
        var command = new Command("status", "Read queued, applied, rejected, or restart-required state.");
        command.Options.Add(workspace);
        command.SetAction(parse => cli.Status(parse.GetRequiredValue(workspace)));
        return command;
    }

    private static Command Stop(SourceUpdateCli cli)
    {
        var workspace = Required("--workspace", "Live workspace id or path.");
        var command = new Command("stop", "Stop an idle Source Update coordinator.");
        command.Options.Add(workspace);
        command.SetAction(parse => cli.Stop(parse.GetRequiredValue(workspace)));
        return command;
    }

    private static Command Coordinator(SourceUpdateCli cli)
    {
        var workspace = Required("--workspace-path", "Absolute live workspace path.");
        var launch = Required("--launch", "Absolute editable launch manifest.");
        var session = Required("--session", "Exact LiveCode session id.");
        var discovery = Discovery();
        var command = new Command("coordinator")
        {
            Hidden = true
        };
        command.Options.Add(workspace);
        command.Options.Add(launch);
        command.Options.Add(session);
        command.Options.Add(discovery);
        command.SetAction(parse => cli.Coordinator(
            parse.GetRequiredValue(workspace),
            parse.GetRequiredValue(launch),
            parse.GetRequiredValue(session),
            parse.GetValue(discovery)));
        return command;
    }

    private static Option<string> Required(string name, string description) =>
        new(name) { Description = description, Required = true };

    private static Option<string?> Discovery() =>
        new("--discovery-dir")
        {
            Description = "Override the default per-user LiveCode discovery directory."
        };
}
