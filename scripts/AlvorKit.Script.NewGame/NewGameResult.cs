namespace AlvorKit.Script.NewGame;

/// <summary>Summary of a generated game repository.</summary>
/// <param name="OutputPath">Absolute path of the created repository.</param>
/// <param name="FileCount">Number of source files copied into the repository.</param>
internal sealed record NewGameResult(string OutputPath, int FileCount);
