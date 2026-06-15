using ClangSharp;

namespace AlvorKit.Script.Bindgen;

/// <summary>Indexes native record definitions by direct and typedef names.</summary>
internal static class CHeaderRecordIndex
{
    /// <summary>Adds record declarations found in the selected Clang declarations.</summary>
    public static void Index(CHeaderParseState state, List<Decl> declarations)
    {
        foreach (var declaration in declarations)
        {
            if (declaration is RecordDecl { Name.Length: > 0 } record && record.Definition is not null)
                AddRecord(state, record.Name, (RecordDecl)record.Definition);
            else if (declaration is TypedefDecl typedef
                && typedef.UnderlyingType.CanonicalType is RecordType recordType
                && recordType.Decl is RecordDecl { Definition: not null } aliased)
                AddRecord(state, typedef.Name, (RecordDecl)aliased.Definition, preferPublicName: true);
        }
    }

    /// <summary>Adds one record spelling and tracks the public emission name once.</summary>
    private static void AddRecord(
        CHeaderParseState state,
        string nativeName,
        RecordDecl definition,
        bool preferPublicName = false)
    {
        state.RecordByNativeName.TryAdd(nativeName, definition);
        if (preferPublicName)
            state.PublicRecordNames.RemoveAll(name => ReferenceEquals(state.RecordByNativeName[name], definition));
        if (!state.PublicRecordNames.Any(name => ReferenceEquals(state.RecordByNativeName[name], definition)))
            state.PublicRecordNames.Add(nativeName);
    }
}
