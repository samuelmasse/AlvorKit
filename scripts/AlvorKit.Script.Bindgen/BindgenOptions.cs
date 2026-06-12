namespace AlvorKit.Script.Bindgen;

public sealed record BindgenOptions(string Selection, bool Strict)
{
    public static BindgenOptions Parse(string[] args) => new(
        Selection: args.FirstOrDefault(arg => !arg.StartsWith("--")) ?? "all",
        Strict: args.Contains("--strict"));
}
