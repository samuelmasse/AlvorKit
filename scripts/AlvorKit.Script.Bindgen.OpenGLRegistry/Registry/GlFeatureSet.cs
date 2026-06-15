namespace AlvorKit.Script.Bindgen;

/// <summary>Selected registry symbol names plus their originating version or extension.</summary>
/// <param name="Commands">Selected command names.</param>
/// <param name="Tokens">Selected token names.</param>
/// <param name="Since">Version or extension that first selected each symbol.</param>
internal sealed record GlFeatureSet(
    HashSet<string> Commands,
    HashSet<string> Tokens,
    IReadOnlyDictionary<string, string> Since);
