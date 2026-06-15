namespace AlvorKit.Script.Bindgen;

/// <summary>Builds generated enum groups from collected registry tokens.</summary>
/// <param name="config">Bindgen configuration used for type renames and API class prefixing.</param>
/// <param name="catchAllName">Generated catch-all enum name reserved from group output.</param>
internal sealed class GlRegistryGroupBuilder(BindgenConfig config, string catchAllName)
{
    /// <summary>Builds typed enum groups from token group attributes.</summary>
    public GlRegistryGroupSet Build(IReadOnlyList<GlRegistryToken> tokens)
    {
        var membersByGroup = MembersByGroup(tokens);
        var nativeGroups = membersByGroup.Keys.ToHashSet(StringComparer.Ordinal);
        var names = membersByGroup.Keys.ToDictionary(group => group, group => ManagedGroupName(group, nativeGroups));
        if (names.Values.Contains(catchAllName))
            throw new InvalidOperationException($"An enum group collides with the {catchAllName} catch-all.");
        GlManagedNameGuard.AssertUnique(names.Select(pair => (pair.Value, pair.Key)), "enum group");

        var groups = membersByGroup
            .Select(pair => new GlEnumGroup(
                pair.Key,
                names[pair.Key],
                pair.Value.Any(token => token.IsBitmask),
                GlRegistryMemberSorter.Sort(pair.Value, names)))
            .OrderBy(group => group.ManagedName, StringComparer.Ordinal)
            .ToList();
        return new(groups, names);
    }

    /// <summary>Groups uint-sized registry tokens by their native enum group names.</summary>
    private static Dictionary<string, List<GlRegistryToken>> MembersByGroup(IReadOnlyList<GlRegistryToken> tokens)
    {
        var membersByGroup = new Dictionary<string, List<GlRegistryToken>>();
        foreach (var token in tokens.Where(token => token.Value <= uint.MaxValue))
            foreach (var group in token.Groups)
            {
                if (!membersByGroup.TryGetValue(group, out var members))
                    membersByGroup[group] = members = [];
                members.Add(token);
            }
        return membersByGroup;
    }

    /// <summary>Computes the generated managed enum type name for a native registry group.</summary>
    private string ManagedGroupName(string nativeName, ISet<string> nativeGroups)
    {
        var stem = config.TypeRenames.TryGetValue(nativeName, out var renamed)
            ? renamed
            : nativeName.EndsWith("ARB") && nativeName.Length > 3 && !nativeGroups.Contains(nativeName[..^3]) ? nativeName[..^3] : nativeName;
        return config.ApiClass + stem;
    }
}
