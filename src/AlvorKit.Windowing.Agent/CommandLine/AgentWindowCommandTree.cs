namespace AlvorKit.Windowing;

/// <summary>Owns the System.CommandLine command tree for the agent window protocol.</summary>
internal sealed class AgentWindowCommandTree
{
    private readonly InvocationConfiguration invocation;
    private readonly RootCommand root;

    /// <summary>Creates a command tree that writes generated help and errors to the supplied output.</summary>
    /// <param name="protocol">Command protocol that receives parsed agent actions.</param>
    /// <param name="output">Output stream used by generated command help.</param>
    internal AgentWindowCommandTree(AgentWindowCommandProtocol protocol, TextWriter output)
    {
        invocation = new()
        {
            Output = output,
            Error = output,
            EnableDefaultExceptionHandler = false
        };
        root = CreateRootCommand(protocol);
        root.Subcommands.Add(CreateHelpCommand());
    }

    /// <summary>Parses one interactive command line without invoking it.</summary>
    /// <param name="command">Command text read from the agent stream.</param>
    /// <returns>The parse result.</returns>
    internal ParseResult Parse(string command) => root.Parse(command);

    /// <summary>Invokes a previously parsed command against the configured protocol.</summary>
    /// <param name="parseResult">Parse result returned by <see cref="Parse" />.</param>
    /// <returns>The command exit code.</returns>
    internal int Invoke(ParseResult parseResult) => parseResult.Invoke(invocation);

    /// <summary>Writes generated command-line help for the command tree.</summary>
    internal void WriteHelp()
    {
        var exitCode = root.Parse("--help").Invoke(invocation);
        if (exitCode != 0)
            throw new InvalidOperationException($"Help exited with code {exitCode}.");
    }

    /// <summary>Creates the root command and all interactive subcommands.</summary>
    /// <param name="protocol">Command protocol that receives parsed agent actions.</param>
    /// <returns>The configured root command.</returns>
    private static RootCommand CreateRootCommand(AgentWindowCommandProtocol protocol)
    {
        var command = new RootCommand("AlvorKit agent window command interface.");
        AgentWindowFrameCommands.AddTo(command, protocol);
        AgentWindowInputCommands.AddTo(command, protocol);
        AgentWindowUtilityCommands.AddTo(command, protocol);
        return command;
    }

    /// <summary>Creates the generated-help command.</summary>
    /// <returns>The configured command.</returns>
    private Command CreateHelpCommand()
    {
        var command = new Command("help", "Show command help.");
        command.SetAction(_ => WriteHelp());
        return command;
    }
}
