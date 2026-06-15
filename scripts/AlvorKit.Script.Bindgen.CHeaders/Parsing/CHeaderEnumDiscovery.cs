using ClangSharp;
using ClangType = ClangSharp.Type;

namespace AlvorKit.Script.Bindgen;

/// <summary>Discovers native enums and emits their managed enum descriptions.</summary>
internal static class CHeaderEnumDiscovery
{
    /// <summary>Adds all supported enum declarations from the selected Clang declarations.</summary>
    public static void Discover(
        BindgenConfig config,
        CHeaderParseState state,
        CHeaderNameMapper names,
        List<Decl> declarations)
    {
        TypedefDecl? previousTypedef = null;
        uint previousTypedefLine = 0;

        foreach (var declaration in declarations)
        {
            declaration.Location.GetExpansionLocation(out _, out var line, out _, out _);
            if (declaration is TypedefDecl typedef)
            {
                if (typedef.UnderlyingType.CanonicalType is EnumType enumType)
                    AddEnum(config, state, names, typedef.Name, enumType.Decl, enumType.Decl.IntegerType, typedef);
                else
                    (previousTypedef, previousTypedefLine) = (typedef, line);
            }
            else if (declaration is EnumDecl enumDecl && previousTypedef is not null && line == previousTypedefLine)
            {
                AddEnum(config, state, names, previousTypedef.Name, enumDecl, previousTypedef.UnderlyingType, previousTypedef);
            }
        }
    }

    /// <summary>Adds one enum after dropping values outside its selected underlying type range.</summary>
    private static void AddEnum(
        BindgenConfig config,
        CHeaderParseState state,
        CHeaderNameMapper names,
        string nativeName,
        EnumDecl enumDecl,
        ClangType underlyingType,
        TypedefDecl typedef)
    {
        if (state.EnumByNativeName.ContainsKey(nativeName))
            return;

        var lookupName = nativeName.TrimStart('_');
        var members = CHeaderEnumMembers.Read(config, enumDecl);
        var managedUnderlyingType = CHeaderTypeMapper.MapIntegerType(underlyingType);
        var (Min, Max) = RangeFor(managedUnderlyingType);
        foreach (var outOfRange in members.Where(member => member.Value < Min || member.Value > Max).ToList())
        {
            Console.WriteLine($"  dropping {nativeName}.{outOfRange.ManagedName} = {outOfRange.Value} (out of range for underlying type)");
            members.Remove(outOfRange);
        }

        state.EnumByNativeName[lookupName] = state.EnumByNativeName[nativeName] = new(
            nativeName,
            names.TypeName(lookupName),
            managedUnderlyingType,
            CHeaderFlagsHeuristic.ShouldEmit(config, nativeName, members),
            members,
            XmlDocComment.Parse(typedef.Handle.RawCommentText.ToString())?.Summary);
    }

    /// <summary>Returns the representable value range for a managed integral type.</summary>
    private static (long Min, long Max) RangeFor(string integerType) => integerType switch
    {
        "byte" => (0L, byte.MaxValue),
        "sbyte" => (sbyte.MinValue, sbyte.MaxValue),
        "ushort" => (0L, ushort.MaxValue),
        "short" => (short.MinValue, short.MaxValue),
        "uint" => (0L, uint.MaxValue),
        _ => (long.MinValue, long.MaxValue)
    };
}
