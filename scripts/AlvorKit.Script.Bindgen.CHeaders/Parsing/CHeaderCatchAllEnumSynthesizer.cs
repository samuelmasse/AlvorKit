namespace AlvorKit.Script.Bindgen;

/// <summary>Promotes discovered macro constants into a catch-all managed enum.</summary>
internal static class CHeaderCatchAllEnumSynthesizer
{
    /// <summary>Adds the generated catch-all enum when the binding exposes any constants.</summary>
    public static void Synthesize(BindgenConfig config, CHeaderParseState state)
    {
        if (state.ConstantTokens.Count == 0)
            return;

        var managedName = config.ApiClass + "Enum";
        if (state.EnumByNativeName.Values.Any(type => type.ManagedName == managedName))
            throw new InvalidOperationException($"An enum collides with the {managedName} catch-all.");

        state.EnumByNativeName[managedName] = new(
            managedName,
            managedName,
            "long",
            IsFlags: false,
            [.. state.ConstantTokens.Select(token => Member(config, state, token))],
            $"Native macro constants from <c>{config.NativeLibrary}</c>.");
    }

    /// <summary>Builds one catch-all enum member with links to narrower generated enum groups.</summary>
    private static BindingEnumMember Member(BindgenConfig config, CHeaderParseState state, BindingConstant token)
    {
        var groups = GroupNames(config, state, token.NativeName).ToList();
        var membership = groups.Count == 0
            ? ""
            : $" See {string.Join(", ", groups.Select(group => $"<see cref=\"{group}\"/>"))}.";
        var documentation = XmlDocComment.NativeSummary(token.NativeName, token.Documentation, $"<c>{token.NativeName}</c>.");
        return new(token.NativeName, token.ManagedName, token.Value, documentation + membership);
    }

    /// <summary>Returns generated enum group names containing the native macro.</summary>
    private static IEnumerable<string> GroupNames(BindgenConfig config, CHeaderParseState state, string nativeName) =>
        config.EnumGroups
            .Where(pair => Contains(pair.Value, state, nativeName))
            .Select(pair => pair.Key)
            .OrderBy(name => name, StringComparer.Ordinal);

    /// <summary>Returns true when a configured enum group contains the native macro.</summary>
    private static bool Contains(EnumGroup group, CHeaderParseState state, string nativeName)
    {
        if (!state.ValuesByNativeName.ContainsKey(nativeName))
            return false;
        return group.Members is { } members
            ? members.Contains(nativeName, StringComparer.Ordinal)
            : nativeName.StartsWith(group.Prefix, StringComparison.Ordinal) && !group.Exclude.Contains(nativeName);
    }
}
