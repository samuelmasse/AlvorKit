namespace AlvorKit.Script.NativeBuild;

/// <summary>Pure parser for the native build command-line arguments.</summary>
internal static class CliParser
{
    /// <summary>Parses arguments into a request or throws a user-facing argument error.</summary>
    public static CliRequest Parse(IReadOnlyList<string> args)
    {
        var command = args.FirstOrDefault();
        if (command is null or "-h" or "--help" or "help")
            return new(CliCommand.List, selection: null, rid: null, showHelp: true);

        return command switch
        {
            "list" => new(CliCommand.List, selection: null, rid: null, showHelp: false),
            "version" => new(CliCommand.Version, Required(args, 1, "version requires a library name."), null, false),
            "build" => new(CliCommand.Build, Required(args, 1, "build requires a library name or 'all'."), Option(args, "--rid"), false),
            _ => throw new ArgumentException($"Unknown command '{command}'.")
        };
    }

    /// <summary>Returns the required positional argument at an index.</summary>
    private static string Required(IReadOnlyList<string> args, int index, string message) =>
        args.Count > index ? args[index] : throw new ArgumentException(message);

    /// <summary>Finds an option value from either --name value or --name=value syntax.</summary>
    private static string? Option(IReadOnlyList<string> args, string name)
    {
        for (var i = 0; i < args.Count; i++)
        {
            if (args[i] == name)
            {
                if (i + 1 >= args.Count)
                    throw new ArgumentException($"{name} requires a value.");
                return args[i + 1];
            }

            var prefix = name + "=";
            if (args[i].StartsWith(prefix, StringComparison.Ordinal))
                return args[i][prefix.Length..];
        }

        return null;
    }
}
