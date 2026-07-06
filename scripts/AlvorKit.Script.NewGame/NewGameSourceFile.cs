namespace AlvorKit.Script.NewGame;

/// <summary>A file loaded from the concrete starter game source tree.</summary>
/// <param name="RelativePath">Repository-style path relative to the starter source root.</param>
/// <param name="Bytes">Raw file bytes from the starter source root.</param>
/// <param name="IsText">Whether the file should receive starter-name substitutions.</param>
internal sealed record NewGameSourceFile(string RelativePath, byte[] Bytes, bool IsText);
