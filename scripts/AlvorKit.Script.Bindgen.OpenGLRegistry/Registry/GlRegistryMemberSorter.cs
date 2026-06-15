namespace AlvorKit.Script.Bindgen;

/// <summary>Projects collected registry tokens into deterministic generated enum members.</summary>
internal static class GlRegistryMemberSorter
{
    /// <summary>Sorts and projects registry tokens into generated enum members.</summary>
    public static List<GlEnumMember> Sort(
        IEnumerable<GlRegistryToken> tokens,
        IReadOnlyDictionary<string, string> managedNameByGroup) =>
        [
            .. tokens
                .OrderBy(token => token.Value)
                .ThenBy(token => token.ManagedName, StringComparer.Ordinal)
                .Select(token => new GlEnumMember(
                    token.ManagedName,
                    token.NativeName,
                    token.Value,
                    token.Availability,
                    GroupNames(token, managedNameByGroup)))
        ];

    /// <summary>Returns generated enum group names for a token in deterministic order.</summary>
    private static List<string> GroupNames(
        GlRegistryToken token,
        IReadOnlyDictionary<string, string> managedNameByGroup) =>
        [
            .. token.Groups
                .Where(managedNameByGroup.ContainsKey)
                .Select(group => managedNameByGroup[group])
                .OrderBy(name => name, StringComparer.Ordinal)
        ];
}
