namespace AlvorKit.Script.Bindgen;

/// <summary>Configured callback typedef definitions and managed names.</summary>
/// <param name="Signatures">Parsed typedef signatures by native typedef name.</param>
/// <param name="ManagedNames">Generated managed delegate names by native typedef name.</param>
internal sealed record GlCallbackDefinitions(
    IReadOnlyDictionary<string, GlCallbackSignature> Signatures,
    IReadOnlyDictionary<string, string> ManagedNames);
