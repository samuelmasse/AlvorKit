namespace AlvorKit.Script.Bindgen;

/// <summary>Generated enum groups plus the native-to-managed group name map.</summary>
/// <param name="Groups">Typed enum groups selected for emission.</param>
/// <param name="ManagedNameByGroup">Generated managed enum type name by native group.</param>
internal sealed record GlRegistryGroupSet(
    IReadOnlyList<GlEnumGroup> Groups,
    IReadOnlyDictionary<string, string> ManagedNameByGroup);
