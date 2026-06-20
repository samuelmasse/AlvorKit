namespace AlvorKit.Script.AlvorSense;

/// <summary>Owns the System.CommandLine command tree for foreground and private AlvorSense commands.</summary>
internal static class AlvorSenseCommandTree
{
    /// <summary>Creates the full command tree used by parsing and help rendering.</summary>
    /// <param name="context">Mutable parse context that receives the selected command.</param>
    /// <returns>The configured root command.</returns>
    internal static RootCommand Create(AlvorSenseParseContext context)
    {
        var command = new RootCommand("Persistent agent-controlled AlvorKit visual sessions.");
        command.Subcommands.Add(AlvorSenseStartCommandLine.Create(context));
        command.Subcommands.Add(AlvorSenseSendCommandLine.Create(context));
        command.Subcommands.Add(CreateStopCommand(context));
        command.Subcommands.Add(CreateListCommand(context));
        command.Subcommands.Add(CreateStatusCommand(context));
        command.Subcommands.Add(CreateHostCommand(context));
        return command;
    }

    /// <summary>Creates an invocation configuration that suppresses output during parse-only tests.</summary>
    /// <returns>The configured invocation.</returns>
    internal static InvocationConfiguration SilentInvocation() =>
        new() { Output = TextWriter.Null, Error = TextWriter.Null, EnableDefaultExceptionHandler = false };

    /// <summary>Creates the stop command and its shared session options.</summary>
    /// <param name="context">Mutable parse context that receives the parsed command.</param>
    /// <returns>The configured command.</returns>
    private static Command CreateStopCommand(AlvorSenseParseContext context)
    {
        var id = AlvorSenseCliOptions.SessionIdOption();
        var timeout = AlvorSenseCliOptions.TimeoutOption();
        var command = new Command("stop", "Stop a running session.");
        command.Options.Add(id);
        command.Options.Add(timeout);
        command.SetAction(parse => context.Command = new AlvorSenseStopCommand(
            AlvorSenseCliOptions.ValidateSessionId(parse.GetRequiredValue(id)),
            AlvorSenseCliOptions.Timeout(parse.GetValue(timeout))));
        return command;
    }

    /// <summary>Creates the list command.</summary>
    /// <param name="context">Mutable parse context that receives the parsed command.</param>
    /// <returns>The configured command.</returns>
    private static Command CreateListCommand(AlvorSenseParseContext context)
    {
        var command = new Command("list", "List known session directories.");
        command.SetAction(_ => context.Command = new AlvorSenseListCommand());
        return command;
    }

    /// <summary>Creates the status command and its required session id option.</summary>
    /// <param name="context">Mutable parse context that receives the parsed command.</param>
    /// <returns>The configured command.</returns>
    private static Command CreateStatusCommand(AlvorSenseParseContext context)
    {
        var id = AlvorSenseCliOptions.SessionIdOption();
        var command = new Command("status", "Read persisted state for one session.");
        command.Options.Add(id);
        command.SetAction(parse => context.Command = new AlvorSenseStatusCommand(
            AlvorSenseCliOptions.ValidateSessionId(parse.GetRequiredValue(id))));
        return command;
    }

    /// <summary>Creates the private host command used by detached session workers.</summary>
    /// <param name="context">Mutable parse context that receives the parsed command.</param>
    /// <returns>The configured command.</returns>
    private static Command CreateHostCommand(AlvorSenseParseContext context)
    {
        var sessionDir = AlvorSenseCliOptions.RequiredStringOption(
            "--session-dir",
            "Session directory containing the manifest and mailbox.");
        var command = new Command("host", "Run the private background host loop.") { Hidden = true };
        command.Options.Add(sessionDir);
        command.SetAction(parse => context.Command = new AlvorSenseHostCommand(
            AlvorSenseCliOptions.RequiredText(parse.GetRequiredValue(sessionDir), "--session-dir")));
        return command;
    }
}
