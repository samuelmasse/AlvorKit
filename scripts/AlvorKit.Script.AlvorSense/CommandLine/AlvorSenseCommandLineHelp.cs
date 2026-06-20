namespace AlvorKit.Script.AlvorSense;

/// <summary>Recognizes and renders generated AlvorSense command-line help.</summary>
internal static class AlvorSenseCommandLineHelp
{
    /// <summary>Returns help arguments for explicit help requests.</summary>
    /// <param name="args">Command-line arguments supplied by the caller.</param>
    /// <returns>Arguments that render contextual help, or <see langword="null" /> when help was not requested.</returns>
    internal static string[]? Args(string[] args)
    {
        if (IsHelp(args[0]))
            return args.Length == 1 ? ["--help"] : [.. args[1..], "--help"];

        foreach (var arg in args)
        {
            if (IsHelp(arg))
                return args;
        }

        return null;
    }

    /// <summary>Writes generated help for the root command or a contextual subcommand.</summary>
    /// <param name="args">Arguments that select the help topic.</param>
    /// <param name="output">Output stream receiving generated help text.</param>
    internal static void Write(string[] args, TextWriter output)
    {
        var context = new AlvorSenseParseContext(args, TextReader.Null, Directory.GetCurrentDirectory());
        var result = AlvorSenseCommandTree.Create(context).Parse(args);
        var exitCode = result.Invoke(new()
        {
            Output = output,
            Error = output,
            EnableDefaultExceptionHandler = false
        });
        if (exitCode != 0)
            throw new InvalidOperationException($"Help exited with code {exitCode}.");
    }

    /// <summary>Checks whether an argument requests generated command-line help.</summary>
    /// <param name="arg">Argument to inspect.</param>
    /// <returns><see langword="true" /> when the argument is a help token.</returns>
    private static bool IsHelp(string arg) =>
        arg.Equals("help", StringComparison.OrdinalIgnoreCase) ||
        arg.Equals("--help", StringComparison.OrdinalIgnoreCase) ||
        arg.Equals("-h", StringComparison.OrdinalIgnoreCase);
}
