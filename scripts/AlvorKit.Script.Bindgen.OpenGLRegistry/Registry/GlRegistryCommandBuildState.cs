namespace AlvorKit.Script.Bindgen;

/// <summary>Mutable command-build collections kept private to command construction.</summary>
/// <param name="Commands">Commands accumulated during the build.</param>
/// <param name="UngroupedEnumUses">Enum-like declaration diagnostics accumulated during mapping.</param>
/// <param name="SkippedCommands">Configured command skips accumulated during mapping.</param>
/// <param name="HandleTypes">Strongly typed handle names referenced during mapping.</param>
/// <param name="UsedCallbacks">Callback typedefs referenced during mapping.</param>
internal sealed record GlRegistryCommandBuildState(
    List<GlCommand> Commands,
    List<string> UngroupedEnumUses,
    List<string> SkippedCommands,
    SortedSet<string> HandleTypes,
    HashSet<string> UsedCallbacks);
