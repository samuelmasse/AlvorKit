namespace AlvorKit.Script.Bindgen;

/// <summary>Generated signature metadata for a planned combined overload.</summary>
/// <param name="Key">Deduplication key for the overload signature.</param>
/// <param name="GenericSuffix">Generic type parameter suffix, including angle brackets.</param>
/// <param name="ParameterList">Rendered public parameter list.</param>
/// <param name="Constraints">Rendered generic constraints.</param>
internal sealed record GlCombinedSignature(
    string Key,
    string GenericSuffix,
    string ParameterList,
    string Constraints);
