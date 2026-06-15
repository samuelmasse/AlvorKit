namespace AlvorKit.Script.Bindgen;

/// <summary>Command-line options accepted by the bindgen executable.</summary>
/// <param name="Selection">Requested native library name, or <c>all</c> to generate every configured binding.</param>
/// <param name="Strict">Whether validation gaps should fail the run instead of printing warnings or updating local artifacts.</param>
public sealed record BindgenOptions(string Selection, bool Strict)
{
    /// <summary>Parses bindgen arguments into the selected library and strictness flag.</summary>
    public static BindgenOptions Parse(string[] args) => new(
        Selection: args.FirstOrDefault(arg => !arg.StartsWith("--")) ?? "all",
        Strict: args.Contains("--strict"));
}
