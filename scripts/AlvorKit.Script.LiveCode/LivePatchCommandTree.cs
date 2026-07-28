namespace AlvorKit.Script.LiveCode;

/// <summary>Builds the LivePatch branch of the generated command line.</summary>
internal static class LivePatchCommandTree
{
    /// <summary>Creates all LivePatch discovery and mutation commands.</summary>
    internal static Command Create(LivePatchCli cli)
    {
        var command = new Command(
            "patch",
            "Compile and control exact-signature live method replacements through the target's LivePatch bridge.");
        command.Subcommands.Add(Capabilities(cli));
        command.Subcommands.Add(List(cli));
        command.Subcommands.Add(Status(cli));
        command.Subcommands.Add(Install(cli));
        command.Subcommands.Add(Replace(cli));
        command.Subcommands.Add(Remove(cli));
        return command;
    }

    private static Command Capabilities(LivePatchCli cli)
    {
        var session = RequiredOption("--session", "Exact session id or display name.");
        var discovery = DiscoveryOption();
        var workspace = WorkspaceOption();
        var command = new Command(
            "capabilities",
            "Read negotiated profiler, selector, handler ABI, and eligibility capabilities.");
        command.Options.Add(session);
        command.Options.Add(discovery);
        command.Options.Add(workspace);
        command.SetAction(parse => cli.PatchCapabilities(
            parse.GetRequiredValue(session),
            parse.GetValue(discovery),
            parse.GetValue(workspace)));
        return command;
    }

    private static Command List(LivePatchCli cli)
    {
        var session = RequiredOption("--session", "Exact session id or display name.");
        var discovery = DiscoveryOption();
        var workspace = WorkspaceOption();
        var command = new Command("list", "List active and retained terminal LivePatch evidence.");
        command.Options.Add(session);
        command.Options.Add(discovery);
        command.Options.Add(workspace);
        command.SetAction(parse => cli.PatchList(
            parse.GetRequiredValue(session),
            parse.GetValue(discovery),
            parse.GetValue(workspace)));
        return command;
    }

    private static Command Status(LivePatchCli cli)
    {
        var session = RequiredOption("--session", "Exact session id or display name.");
        var patch = RequiredUInt64Option();
        var discovery = DiscoveryOption();
        var workspace = WorkspaceOption();
        var command = new Command("status", "Read one patch and its submitted-code lifetime.");
        command.Options.Add(session);
        command.Options.Add(patch);
        command.Options.Add(discovery);
        command.Options.Add(workspace);
        command.SetAction(parse => cli.PatchStatus(
            parse.GetRequiredValue(session),
            parse.GetRequiredValue(patch),
            parse.GetValue(discovery),
            parse.GetValue(workspace)));
        return command;
    }

    private static Command Install(LivePatchCli cli)
    {
        var session = RequiredOption("--session", "Exact session id or display name.");
        var scope = RequiredOption(
            "--scope",
            "Executor/selector scope id, exact label, or exact scope type name.");
        var selector = new Option<string>("--selector")
        {
            Description = "exact-instance, exact-scope, descendants, or all.",
            DefaultValueFactory = _ => "exact-scope"
        };
        var target = RequiredOption(
            "--target",
            "Exact target in Namespace.Type::Method form; handler signature selects the overload.");
        var targetAssembly = new Option<string?>("--target-assembly")
        {
            Description = "Optional exact simple assembly name used to disambiguate the target type."
        };
        var name = new Option<string?>("--name")
        {
            Description = "Diagnostic patch name."
        };
        var file = SourceFileOption();
        var discovery = DiscoveryOption();
        var workspace = WorkspaceOption();
        var command = new Command(
            "install",
            "Compile a [LivePatchHandler] class and install it for an explicit receiver selector.");
        command.Options.Add(session);
        command.Options.Add(scope);
        command.Options.Add(selector);
        command.Options.Add(target);
        command.Options.Add(targetAssembly);
        command.Options.Add(name);
        command.Options.Add(file);
        command.Options.Add(discovery);
        command.Options.Add(workspace);
        command.SetAction(parse => cli.PatchInstall(
            parse.GetRequiredValue(session),
            parse.GetRequiredValue(scope),
            parse.GetValue(selector)!,
            parse.GetRequiredValue(target),
            parse.GetValue(targetAssembly),
            parse.GetValue(name),
            parse.GetValue(file),
            parse.GetValue(discovery),
            parse.GetValue(workspace)));
        return command;
    }

    private static Command Replace(LivePatchCli cli)
    {
        var session = RequiredOption("--session", "Exact session id or display name.");
        var patch = RequiredUInt64Option();
        var scope = new Option<string?>("--scope")
        {
            Description = "Optional replacement constructor executor scope; defaults to the installed scope."
        };
        var file = SourceFileOption();
        var discovery = DiscoveryOption();
        var workspace = WorkspaceOption();
        var command = new Command(
            "replace",
            "Atomically replace a patch handler without another ReJIT.");
        command.Options.Add(session);
        command.Options.Add(patch);
        command.Options.Add(scope);
        command.Options.Add(file);
        command.Options.Add(discovery);
        command.Options.Add(workspace);
        command.SetAction(parse => cli.PatchReplace(
            parse.GetRequiredValue(session),
            parse.GetRequiredValue(patch),
            parse.GetValue(scope),
            parse.GetValue(file),
            parse.GetValue(discovery),
            parse.GetValue(workspace)));
        return command;
    }

    private static Command Remove(LivePatchCli cli)
    {
        var session = RequiredOption("--session", "Exact session id or display name.");
        var patch = RequiredUInt64Option();
        var discovery = DiscoveryOption();
        var workspace = WorkspaceOption();
        var command = new Command(
            "remove",
            "Stop managed dispatch immediately and asynchronously restore original IL.");
        command.Options.Add(session);
        command.Options.Add(patch);
        command.Options.Add(discovery);
        command.Options.Add(workspace);
        command.SetAction(parse => cli.PatchRemove(
            parse.GetRequiredValue(session),
            parse.GetRequiredValue(patch),
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

    private static Option<ulong> RequiredUInt64Option() =>
        new("--patch")
        {
            Description = "Stable LivePatch ID.",
            Required = true
        };
}
