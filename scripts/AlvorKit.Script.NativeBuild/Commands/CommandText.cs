namespace AlvorKit.Script.NativeBuild;

/// <summary>Formats commands and shell fragments for logs and generated scripts.</summary>
internal static class CommandText
{
    /// <summary>Formats a command for human-readable logging.</summary>
    public static string Display(CommandSpec command) =>
        command.FileName + " " + string.Join(" ", command.Arguments.Select(DisplayArgument));

    /// <summary>Quotes a PowerShell string literal with single quotes.</summary>
    public static string PowerShellQuote(string value) =>
        "'" + value.Replace("'", "''", StringComparison.Ordinal) + "'";

    /// <summary>Quotes many arguments for inclusion in a generated PowerShell script.</summary>
    public static string PowerShellArgs(IEnumerable<string> args) =>
        string.Join(" ", args.Select(PowerShellQuote));

    /// <summary>Quotes one argument for log output only.</summary>
    private static string DisplayArgument(string arg) =>
        arg.Any(char.IsWhiteSpace) ? "\"" + arg.Replace("\"", "\\\"", StringComparison.Ordinal) + "\"" : arg;
}
