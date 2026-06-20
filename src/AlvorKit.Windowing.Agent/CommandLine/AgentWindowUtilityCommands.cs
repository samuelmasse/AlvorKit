namespace AlvorKit.Windowing;

/// <summary>Creates commands for inspection, screenshots, and command-loop control.</summary>
internal static class AgentWindowUtilityCommands
{
    /// <summary>Adds utility commands to the root command.</summary>
    /// <param name="root">Root command that receives the subcommands.</param>
    /// <param name="protocol">Command protocol that receives parsed utility actions.</param>
    internal static void AddTo(RootCommand root, AgentWindowCommandProtocol protocol)
    {
        root.Subcommands.Add(CreateScreenshotCommand(protocol));
        root.Subcommands.Add(CreateSimpleCommand("close", "Request window close.", protocol.Close));
        root.Subcommands.Add(CreateSimpleCommand("state", "Print deterministic host state.", protocol.WriteState));
        root.Subcommands.Add(CreateSimpleCommand("quit", "Stop reading commands.", protocol.StopReading));
        root.Subcommands.Add(CreateSimpleCommand("exit", "Stop reading commands.", protocol.StopReading));
    }

    /// <summary>Creates the screenshot command.</summary>
    /// <param name="protocol">Command protocol that receives the parsed output path.</param>
    /// <returns>The configured command.</returns>
    private static Command CreateScreenshotCommand(AgentWindowCommandProtocol protocol)
    {
        var path = new Argument<string>("path");
        var command = new Command("screenshot", "Render and save a PNG.");
        command.Arguments.Add(path);
        command.SetAction(parseResult => protocol.Screenshot(parseResult.GetValue(path) ?? string.Empty));
        return command;
    }

    /// <summary>Creates a no-argument utility command.</summary>
    /// <param name="name">Command name.</param>
    /// <param name="description">Help description.</param>
    /// <param name="action">Protocol action to run.</param>
    /// <returns>The configured command.</returns>
    private static Command CreateSimpleCommand(string name, string description, Action action)
    {
        var command = new Command(name, description);
        command.SetAction(_ => action());
        return command;
    }
}
