namespace AlvorKit.Script.Lint;

/// <summary>Formats process commands for human-readable logging.</summary>
internal static class CommandText
{
    /// <summary>Formats a command using shell-like quoting for display only.</summary>
    public static string Display(CommandSpec command) =>
        command.FileName + " " + string.Join(" ", command.Arguments.Select(DisplayArgument));

    /// <summary>Quotes one argument when whitespace would make the log ambiguous.</summary>
    private static string DisplayArgument(string arg) =>
        arg.Any(char.IsWhiteSpace) ? "\"" + arg.Replace("\"", "\\\"", StringComparison.Ordinal) + "\"" : arg;
}
