namespace AlvorKit.Script.AlvorSense;

/// <summary>Converts System.CommandLine parse failures into compact CLI errors.</summary>
internal static class AlvorSenseCommandLineErrors
{
    /// <summary>Throws an argument exception when System.CommandLine found parse errors.</summary>
    /// <param name="result">Parse result to inspect.</param>
    internal static void ThrowIfErrors(ParseResult result)
    {
        if (result.Errors.Count > 0)
            throw new ArgumentException(ParseErrorText(result));
    }

    /// <summary>Combines parse errors into the compact single-line style used by the CLI.</summary>
    /// <param name="result">Parse result containing one or more errors.</param>
    /// <returns>Human-readable parse error text.</returns>
    private static string ParseErrorText(ParseResult result)
    {
        var builder = new StringBuilder();
        foreach (var error in result.Errors)
        {
            if (builder.Length > 0)
                builder.Append(' ');
            builder.Append(error.Message);
        }

        return builder.Length == 0 ? "Invalid command." : builder.ToString();
    }
}
