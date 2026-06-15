namespace AlvorKit.Script.Bindgen;

/// <summary>Builds a managed binding model from C declarations and library-specific hints.</summary>
public sealed class CHeaderBindingParser(BindgenConfig config, string managedTypePrefix)
{
    /// <summary>Parses a C translation unit and returns the generated binding model.</summary>
    public BindingModel Parse(
        string translationUnitPath,
        string includeDirectory,
        string filterRoot,
        string libraryDirectory,
        string targetTriple)
    {
        using var translationUnit = CHeaderTranslationUnit.Parse(
            config,
            translationUnitPath,
            includeDirectory,
            filterRoot,
            libraryDirectory,
            targetTriple);
        var state = new CHeaderParseState();
        var names = new CHeaderNameMapper(config, managedTypePrefix);
        CHeaderRecordResolver? records = null;
        var types = new CHeaderTypeMapper(config, state, nativeName => records?.ResolveStruct(nativeName));

        CHeaderEnumDiscovery.Discover(config, state, names, translationUnit.Declarations);
        CHeaderRecordIndex.Index(state, translationUnit.Declarations);
        records = new CHeaderRecordResolver(config, state, names, types);
        CHeaderCallbackDiscovery.Discover(config, state, names, types, translationUnit.Declarations);
        foreach (var nativeName in config.TransparentStructs)
            records.ResolveStruct(nativeName);
        new CHeaderFunctionDiscovery(config, state, types).Discover(translationUnit.Declarations);
        new CHeaderConstantDiscovery(config, state, translationUnit).Discover();
        CHeaderEnumGroupSynthesizer.Synthesize(config, state);
        CHeaderCatchAllEnumSynthesizer.Synthesize(config, state);
        return state.ToModel();
    }

    /// <summary>Re-parses for another ABI and rejects structs whose natural layout would differ.</summary>
    public static void ValidateNaturalLayout(
        BindgenConfig config,
        string translationUnitPath,
        string includeDirectory,
        string filterRoot,
        string libraryDirectory,
        string targetTriple,
        IEnumerable<string> nativeStructNames)
    {
        using var translationUnit = CHeaderTranslationUnit.Parse(
            config,
            translationUnitPath,
            includeDirectory,
            filterRoot,
            libraryDirectory,
            targetTriple);
        var state = new CHeaderParseState();
        CHeaderRecordIndex.Index(state, translationUnit.Declarations);
        foreach (var nativeName in nativeStructNames)
            if (state.RecordByNativeName.TryGetValue(nativeName, out var record))
                CHeaderLayoutValidator.ValidateNaturalRecordLayout(state, record, targetTriple);
    }
}
