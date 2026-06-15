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
                state.RecordByNativeName[record.Name] = (RecordDecl)record.Definition;
            else if (declaration is TypedefDecl typedef
                && typedef.UnderlyingType.CanonicalType is RecordType recordType
                && recordType.Decl is RecordDecl { Definition: not null } aliased)
                state.RecordByNativeName.TryAdd(typedef.Name, (RecordDecl)aliased.Definition);
        }
    }
}
