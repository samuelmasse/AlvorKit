namespace AlvorKit.Script.Bindgen;

/// <summary>Mapped command set plus diagnostics accumulated while mapping declarations.</summary>
/// <param name="Commands">Commands selected for emission.</param>
/// <param name="UngroupedEnumUses">Enum-like declaration positions without registry group metadata.</param>
/// <param name="SkippedCommands">Configured command skips with reasons.</param>
/// <param name="HandleTypes">Strongly typed handles referenced by selected commands.</param>
/// <param name="UsedCallbacks">Callback typedefs referenced by selected commands.</param>
internal sealed record GlCommandBuildResult(
    IReadOnlyList<GlCommand> Commands,
    IReadOnlyList<string> UngroupedEnumUses,
    IReadOnlyList<string> SkippedCommands,
    IReadOnlyList<string> HandleTypes,
    IReadOnlySet<string> UsedCallbacks);
