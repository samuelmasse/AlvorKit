namespace AlvorKit.Script.Bindgen;

/// <summary>Creates configured enum groups from macro constants.</summary>
internal static class CHeaderEnumGroupSynthesizer
{
    /// <summary>Adds all configured enum groups to the parse state.</summary>
    public static void Synthesize(BindgenConfig config, CHeaderParseState state)
    {
        foreach (var (enumName, group) in config.EnumGroups)
        {
            var members = NativeNames(group, state)
                .Where(state.ValuesByNativeName.ContainsKey)
                .Select(native => new BindingEnumMember(MemberName(group, native), state.ValuesByNativeName[native], null))
                .ToList();
            state.EnumByNativeName[enumName] = new(enumName, enumName, "int", group.Flags, members, null);
        }
    }

    /// <summary>Returns configured members or all discovered constants with the group prefix.</summary>
    private static IEnumerable<string> NativeNames(EnumGroup group, CHeaderParseState state) =>
        group.Members ?? state.NativeNamesInOrder.Where(name => name.StartsWith(group.Prefix) && !group.Exclude.Contains(name));

    /// <summary>Returns the managed enum member name for a native macro name.</summary>
    private static string MemberName(EnumGroup group, string nativeName)
    {
        var name = CSharpName.FromNativeIdentifier(nativeName, group.Prefix, group.DigitPrefix);
        return group.Suffix.Length > 0 && name.Length > group.Suffix.Length && name.EndsWith(group.Suffix)
            ? name[..^group.Suffix.Length]
            : name;
    }
}
