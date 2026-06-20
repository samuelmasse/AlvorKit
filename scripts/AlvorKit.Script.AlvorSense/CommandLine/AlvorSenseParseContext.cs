namespace AlvorKit.Script.AlvorSense;

/// <summary>Holds command-line parse inputs and the command selected by invocation.</summary>
/// <param name="args">Original command-line arguments, preserved for special send command sources.</param>
/// <param name="input">Standard input used by send commands when no explicit source is supplied.</param>
/// <param name="currentDirectory">Current directory used for command defaults.</param>
internal sealed class AlvorSenseParseContext(string[] args, TextReader input, string currentDirectory)
{
    /// <summary>Gets the original command-line arguments.</summary>
    internal string[] Args => args;

    /// <summary>Gets the standard input stream available to send commands.</summary>
    internal TextReader Input => input;

    /// <summary>Gets the current directory captured at parse time.</summary>
    internal string CurrentDirectory => currentDirectory;

    /// <summary>Gets or sets the command selected by the invoked command-line action.</summary>
    internal AlvorSenseCommand? Command { get; set; }

    /// <summary>Returns the selected command or fails if no command action ran.</summary>
    /// <returns>The parsed AlvorSense command.</returns>
    internal AlvorSenseCommand TakeCommand() =>
        Command ?? throw new ArgumentException("A command is required.");
}
