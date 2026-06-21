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
                .Select(native => new BindingEnumMember(native, MemberName(group, native), state.ValuesByNativeName[native], null))
                .ToList();
            state.EnumByNativeName[enumName] = new(enumName, enumName, "int", group.Flags, members, Documentation(group));
        }
    }

    /// <summary>Returns configured members or all discovered constants with the group prefix.</summary>
    private static IEnumerable<string> NativeNames(EnumGroup group, CHeaderParseState state) =>
        group.Members ?? state.NativeNamesInOrder.Where(name => name.StartsWith(group.Prefix) && !group.Exclude.Contains(name));

    /// <summary>Returns XML documentation for one configured macro enum group.</summary>
    private static string Documentation(EnumGroup group)
    {
        var subject = group.Members is null ? "Native constants" : "Selected native constants";
        return $"{subject} matching <c>{group.Prefix}*</c>.";
    }

    /// <summary>Returns the managed enum member name for a native macro name.</summary>
    private static string MemberName(EnumGroup group, string nativeName)
    {
        var name = CSharpName.FromNativeIdentifier(nativeName, group.Prefix, group.DigitPrefix);
        return group.Suffix.Length > 0 && name.Length > group.Suffix.Length && name.EndsWith(group.Suffix)
            ? name[..^group.Suffix.Length]
            : name;
    }
}
