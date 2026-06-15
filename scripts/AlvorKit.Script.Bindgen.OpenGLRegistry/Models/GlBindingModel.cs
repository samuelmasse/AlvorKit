namespace AlvorKit.Script.Bindgen;

/// <summary>Complete OpenGL binding model consumed by the generated-code emitters.</summary>
/// <param name="Groups">Typed enum groups emitted as public enums.</param>
/// <param name="AllTokens">Catch-all enum containing every uint-sized selected token.</param>
/// <param name="WideTokens">Catch-all enum containing selected tokens too wide for uint, when any exist.</param>
/// <param name="Commands">Selected OpenGL commands.</param>
/// <param name="UngroupedEnumUses">Diagnostics for enum-like command positions without typed groups.</param>
/// <param name="SkippedCommands">Configured command skips with reasons.</param>
/// <param name="HandleTypes">Generated strongly typed handle names.</param>
/// <param name="Delegates">Generated callback delegate types.</param>
public sealed record GlBindingModel(
    IReadOnlyList<GlEnumGroup> Groups,
    GlEnumGroup AllTokens,
    GlEnumGroup? WideTokens,
    IReadOnlyList<GlCommand> Commands,
    IReadOnlyList<string> UngroupedEnumUses,
    IReadOnlyList<string> SkippedCommands,
    IReadOnlyList<string> HandleTypes,
    IReadOnlyList<GlDelegate> Delegates);
