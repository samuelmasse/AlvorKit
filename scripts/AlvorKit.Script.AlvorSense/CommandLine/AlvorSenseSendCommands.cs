namespace AlvorKit.Script.AlvorSense;

/// <summary>Reads and normalizes command batches for foreground send requests.</summary>
internal static class AlvorSenseSendCommands
{
    /// <summary>Parses send command sources while preserving the legacy source-order behavior.</summary>
    /// <param name="args">Original command-line arguments for the send command.</param>
    /// <param name="input">Standard input used when no explicit command source is supplied.</param>
    /// <returns>Normalized command lines to send to the hosted game process.</returns>
    internal static string[] FromArgs(string[] args, TextReader input)
    {
        var commands = new List<string>();
        var readInput = true;

        for (var i = 1; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--id" or "--timeout":
                    i++;
                    break;
                case "--command":
                    commands.AddRange(Parse(AlvorSenseCliOptions.OptionValue(args, ref i, "--command")));
                    readInput = false;
                    break;
                case "--commands" or "--file":
                    var option = args[i];
                    var path = AlvorSenseCliOptions.OptionValue(args, ref i, option);
                    commands.AddRange(Parse(File.ReadAllText(path, Encoding.UTF8)));
                    readInput = false;
                    break;
                default:
                    if (args[i].StartsWith("--", StringComparison.Ordinal))
                        throw new ArgumentException($"Unknown send option: {args[i]}");
                    commands.Add(string.Join(' ', args[i..]));
                    readInput = false;
                    i = args.Length;
                    break;
            }
        }

        if (readInput)
            commands.AddRange(Parse(input.ReadToEnd()));

        return [.. commands];
    }

    /// <summary>Parses newline-delimited game commands, ignoring blank lines and comment lines.</summary>
    /// <param name="text">Command text read from standard input or a command file.</param>
    /// <returns>Normalized command lines to send to the hosted game process.</returns>
    internal static string[] Parse(string text) =>
        [.. text.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n', StringSplitOptions.TrimEntries)
            .Select(Normalize)
            .Where(static line => line.Length > 0 && !line.StartsWith('#'))];

    /// <summary>Strips BOM prefixes that can appear when shell pipelines cross console encodings.</summary>
    /// <param name="line">One command line from standard input or a command file.</param>
    /// <returns>The normalized line.</returns>
    private static string Normalize(string line)
    {
        while (line.Length > 0 && (line[0] == '\uFEFF' || line[0] == '\uFFFD'))
            line = line[1..].TrimStart();

        if (line.StartsWith("\u00EF\u00BB\u00BF", StringComparison.Ordinal))
            line = line[3..].TrimStart();

        if (line.StartsWith("\u2229\u2557\u2510", StringComparison.Ordinal))
            line = line[3..].TrimStart();

        return line;
    }
}
