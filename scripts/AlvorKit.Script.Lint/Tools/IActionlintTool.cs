namespace AlvorKit.Script.Lint;

/// <summary>Resolves an actionlint executable for the current platform.</summary>
internal interface IActionlintTool
{
    /// <summary>Returns a usable actionlint executable path, downloading it when necessary.</summary>
    Task<string> EnsureAsync(string repoRoot);
}
