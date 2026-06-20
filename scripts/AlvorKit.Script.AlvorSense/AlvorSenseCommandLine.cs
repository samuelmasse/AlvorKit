namespace AlvorKit.Script.AlvorSense;

/// <summary>Parses foreground and private host commands for persistent agent-controlled window sessions.</summary>
internal static class AlvorSenseCommandLine
{
    /// <summary>Parses command-line arguments and command text for the AlvorSense script.</summary>
    /// <param name="args">Command-line arguments supplied after the script project separator.</param>
    /// <param name="input">Input used for send commands that do not provide a command file.</param>
    /// <returns>The parsed command to execute.</returns>
    internal static AlvorSenseCommand Parse(string[] args, TextReader input)
    {
        if (args.Length == 0)
            throw new ArgumentException("A command is required.");

        var helpArgs = AlvorSenseCommandLineHelp.Args(args);
        if (helpArgs is not null)
            return new AlvorSenseHelpCommand(helpArgs);

        var context = new AlvorSenseParseContext(args, input, Directory.GetCurrentDirectory());
        var result = AlvorSenseCommandTree.Create(context).Parse(args);
        AlvorSenseCommandLineErrors.ThrowIfErrors(result);

        var exitCode = result.Invoke(AlvorSenseCommandTree.SilentInvocation());
        if (exitCode != 0)
            throw new ArgumentException($"Command exited with code {exitCode}.");

        return context.TakeCommand();
    }

    /// <summary>Writes generated help for the root command or a contextual subcommand.</summary>
    /// <param name="args">Arguments that selected help.</param>
    /// <param name="output">Output stream receiving generated help text.</param>
    internal static void WriteHelp(string[] args, TextWriter output) =>
        AlvorSenseCommandLineHelp.Write(args, output);

    /// <summary>Parses newline-delimited game commands, ignoring blank lines and comment lines.</summary>
    /// <param name="text">Command text read from standard input or a command file.</param>
    /// <returns>Normalized command lines to send to the hosted game process.</returns>
    internal static string[] ParseCommands(string text) => AlvorSenseSendCommands.Parse(text);
}
