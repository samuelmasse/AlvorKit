using ClangSharp.Interop;

namespace AlvorKit.Script.Bindgen;

/// <summary>Maps native names and type spellings into managed binding names.</summary>
internal sealed class CHeaderNameMapper(BindgenConfig config, string managedTypePrefix)
{
    /// <summary>Returns the configured or inferred managed type name for a native type.</summary>
    public string TypeName(string nativeName) =>
        config.TypeRenames.GetValueOrDefault(nativeName)
        ?? CSharpName.FromNativeTypeName(nativeName, config.Prefix, managedTypePrefix, config.DigitNamePrefix);

    /// <summary>Returns the configured or inferred managed delegate name for a native callback typedef.</summary>
    public string DelegateName(string nativeName)
    {
        if (config.TypeRenames.TryGetValue(nativeName, out var renamed))
            return renamed;
        return CSharpName.FromNativeTypeName(nativeName, config.Prefix, managedTypePrefix, config.DigitNamePrefix);
    }

    /// <summary>Removes C declaration keywords from a Clang type spelling.</summary>
    public static string CleanTypeSpelling(CXType type) => type.Spelling.ToString()
        .Replace("const ", "")
        .Replace("struct ", "")
        .Replace("union ", "")
        .Replace("enum ", "")
        .Trim();
}
