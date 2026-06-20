namespace AlvorKit.Windowing;

/// <summary>Executes line-oriented commands against an agent-controlled window host.</summary>
internal class AgentWindowCommandRunner
{
    /// <summary>Prefix written before recoverable command errors reported to agent hosts.</summary>
    public const string CommandErrorPrefix = "ALVORSENSE_COMMAND_ERROR ";

    private readonly TextWriter output;
    private readonly AgentWindowCommandProtocol protocol;
    private readonly AgentWindowCommandTree commandTree;

    public AgentWindowCommandRunner(
        AgentGlfwWindowHost host,
        TextWriter output,
        Action<string>? screenshot = null)
    {
        this.output = output;
        protocol = new(host, output, screenshot);
        commandTree = new(protocol, output);
    }

    /// <summary>Reads commands until input ends or an exit command is received, reporting malformed commands without terminating the host.</summary>
    public void Run(TextReader input)
    {
        string? line;
        while ((line = input.ReadLine()) is not null)
        {
            try
            {
                if (!Execute(line))
                    break;
            }
            catch (Exception ex) when (IsRecoverableCommandError(ex))
            {
                output.WriteLine(CommandErrorPrefix + ex.Message);
            }
        }
    }

    /// <summary>Executes one command line and returns whether the caller should continue reading.</summary>
    public bool Execute(string command)
    {
        command = Normalize(command);
        if (string.IsNullOrWhiteSpace(command))
            return true;

        protocol.Reset();
        var parseResult = commandTree.Parse(command);
        if (parseResult.Errors.Count > 0)
            throw new InvalidOperationException(ParseErrorText(parseResult));

        var exitCode = commandTree.Invoke(parseResult);
        protocol.ThrowIfFailed();
        if (exitCode != 0)
            throw new InvalidOperationException($"Command exited with code {exitCode}.");

        return protocol.ContinueReading;
    }

    /// <summary>Writes generated command-line help for the interactive host protocol.</summary>
    public void WriteHelp() => commandTree.WriteHelp();

    /// <summary>Normalizes command text from shells that may prefix redirected input with BOM bytes.</summary>
    private static string Normalize(string command)
    {
        command = command.Trim();
        while (command.Length > 0 && (command[0] == '\uFEFF' || command[0] == '\uFFFD'))
            command = command[1..].TrimStart();
        if (command.StartsWith("\u00EF\u00BB\u00BF", StringComparison.Ordinal))
            command = command[3..].TrimStart();
        if (command.StartsWith("\u2229\u2557\u2510", StringComparison.Ordinal))
            command = command[3..].TrimStart();
        return command;
    }

    /// <summary>Identifies command-shape errors that should be reported without ending the agent command loop.</summary>
    private static bool IsRecoverableCommandError(Exception exception) =>
        exception is ArgumentException or FormatException or IOException or InvalidOperationException or OverflowException;

    private static string ParseErrorText(ParseResult parseResult)
    {
        var builder = new StringBuilder();
        foreach (var error in parseResult.Errors)
        {
            if (builder.Length > 0)
                builder.Append(' ');
            builder.Append(error.Message);
        }
        return builder.ToString();
    }
}
