namespace AlvorKit.Script.Bindgen;

/// <summary>Command-line options accepted by the bindgen executable.</summary>
/// <param name="Selection">Requested native library name, or <c>all</c> to generate every configured binding.</param>
/// <param name="Strict">Whether validation gaps should fail the run instead of printing warnings or updating local artifacts.</param>
/// <param name="OutputRoot">Optional repository-relative or absolute directory under <c>out/</c> for generated projects.</param>
public sealed record BindgenOptions(string Selection, bool Strict, string? OutputRoot)
{
    /// <summary>Parses bindgen arguments into the selected library, strictness flag, and optional generated-output root.</summary>
    public static BindgenOptions Parse(IReadOnlyList<string> args)
    {
        var selection = "all";
        var selectionSet = false;
        var strict = false;
        string? outputRoot = null;

        for (var index = 0; index < args.Count; index++)
        {
            switch (args[index])
            {
                case "--strict":
                    strict = true;
                    break;
                case "--output-root":
                case "--out":
                    outputRoot = ReadValue(args, ref index);
                    break;
                default:
                    if (args[index].StartsWith("--", StringComparison.Ordinal))
                        throw new ArgumentException($"Unknown argument '{args[index]}'.");
                    if (selectionSet)
                        throw new ArgumentException($"Unexpected argument '{args[index]}'.");

                    selection = args[index];
                    selectionSet = true;
                    break;
            }
        }

        return new(selection, strict, outputRoot);
    }

    /// <summary>Reads the value following an option name.</summary>
    private static string ReadValue(IReadOnlyList<string> args, ref int index)
    {
        if (++index >= args.Count)
            throw new ArgumentException($"Missing value for '{args[index - 1]}'.");

        return args[index];
    }
}
