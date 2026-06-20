namespace AlvorKit.Script.AlvorSense;

/// <summary>Creates the System.CommandLine surface for foreground send requests.</summary>
internal static class AlvorSenseSendCommandLine
{
    /// <summary>Creates the send command and its supported command-source options.</summary>
    /// <param name="context">Mutable parse context that receives the parsed command.</param>
    /// <returns>The configured command.</returns>
    internal static Command Create(AlvorSenseParseContext context)
    {
        var id = AlvorSenseCliOptions.SessionIdOption();
        var timeout = AlvorSenseCliOptions.TimeoutOption();
        var commandOption = AlvorSenseCliOptions.RepeatableTextOption("--command", "Command line to send.");
        var file = AlvorSenseCliOptions.RepeatableTextOption("--file", "UTF-8 command file to send.", "--commands");
        var trailing = new Argument<string[]>("command") { Arity = ArgumentArity.ZeroOrMore, Description = "Single trailing command line." };
        var command = new Command("send", "Send commands to a running session.");
        command.Options.Add(id);
        command.Options.Add(timeout);
        command.Options.Add(commandOption);
        command.Options.Add(file);
        command.Arguments.Add(trailing);
        command.SetAction(parse => context.Command = new AlvorSenseSendCommand(
            AlvorSenseCliOptions.ValidateSessionId(parse.GetRequiredValue(id)),
            AlvorSenseSendCommands.FromArgs(context.Args, context.Input),
            AlvorSenseCliOptions.Timeout(parse.GetValue(timeout))));
        return command;
    }
}
