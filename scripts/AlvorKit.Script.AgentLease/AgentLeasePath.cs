namespace AlvorKit.Script.AgentLease;

/// <summary>Normalizes advisory path claims and conservatively detects possible overlap.</summary>
internal static class AgentLeasePath
{
    /// <summary>Returns whether two path claims might refer to the same repository area.</summary>
    /// <param name="left">First repository-relative path, directory, or glob.</param>
    /// <param name="right">Second repository-relative path, directory, or glob.</param>
    public static bool MayOverlap(string left, string right)
    {
        var first = Normalize(left);
        var second = Normalize(right);

        if (IsGlobal(first) || IsGlobal(second))
            return true;
        if (Matches(first, second) || Matches(second, first))
            return true;

        var firstPrefix = LiteralPrefix(first);
        var secondPrefix = LiteralPrefix(second);
        return firstPrefix.Length == 0
            || secondPrefix.Length == 0
            || StartsWithClaim(firstPrefix, secondPrefix)
            || StartsWithClaim(secondPrefix, firstPrefix);
    }

    /// <summary>Normalizes a repository-relative path or glob to slash-separated form.</summary>
    /// <param name="path">Path or glob supplied by an agent.</param>
    public static string Normalize(string path)
    {
        var normalized = path.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/').Trim();
        while (normalized.StartsWith("./", StringComparison.Ordinal))
            normalized = normalized[2..];
        return normalized.TrimStart('/');
    }

    /// <summary>Returns whether a concrete path claim matches a glob claim.</summary>
    /// <param name="path">Concrete or glob path candidate.</param>
    /// <param name="glob">Glob pattern that may contain <c>*</c>, <c>?</c>, or <c>**</c>.</param>
    private static bool Matches(string path, string glob) =>
        HasGlob(glob) && ToRegex(glob).IsMatch(path);

    /// <summary>Returns whether a claim covers the whole repository.</summary>
    /// <param name="path">Normalized path claim.</param>
    private static bool IsGlobal(string path) =>
        path is "*" or "repo-wide";

    /// <summary>Returns whether a path claim includes wildcard tokens.</summary>
    /// <param name="path">Normalized path claim.</param>
    private static bool HasGlob(string path) =>
        path.IndexOfAny(['*', '?']) >= 0;

    /// <summary>Returns the literal directory prefix before any wildcard token.</summary>
    /// <param name="path">Normalized path claim.</param>
    private static string LiteralPrefix(string path)
    {
        var wildcard = path.IndexOfAny(['*', '?']);
        if (wildcard < 0)
            return path;

        var prefix = path[..wildcard];
        var slash = prefix.LastIndexOf('/');
        return prefix[..Math.Max(0, slash + 1)];
    }

    /// <summary>Returns whether a normalized claim starts with a normalized path prefix.</summary>
    /// <param name="claim">Normalized claim that may be a path or directory prefix.</param>
    /// <param name="prefix">Normalized prefix to compare against.</param>
    private static bool StartsWithClaim(string claim, string prefix)
    {
        if (claim.Equals(prefix, StringComparison.OrdinalIgnoreCase))
            return true;

        var directoryPrefix = prefix.EndsWith("/", StringComparison.Ordinal) ? prefix : prefix + "/";
        return claim.StartsWith(directoryPrefix, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Converts the supported glob syntax into a regular expression.</summary>
    /// <param name="glob">Normalized glob to convert.</param>
    private static Regex ToRegex(string glob)
    {
        var regex = new StringBuilder("^");
        for (var index = 0; index < glob.Length; index++)
            AppendToken(regex, glob, ref index);

        regex.Append('$');
        return new(regex.ToString(), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    /// <summary>Appends one glob token to a regular-expression builder.</summary>
    /// <param name="regex">Target regular-expression builder.</param>
    /// <param name="glob">Glob text being converted.</param>
    /// <param name="index">Current token index, advanced when <c>**/</c> is consumed.</param>
    private static void AppendToken(StringBuilder regex, string glob, ref int index)
    {
        if (glob[index] == '*' && index + 1 < glob.Length && glob[index + 1] == '*')
        {
            var followedBySlash = index + 2 < glob.Length && glob[index + 2] == '/';
            regex.Append(followedBySlash ? "(?:.*/)?" : ".*");
            index += followedBySlash ? 2 : 1;
        }
        else if (glob[index] == '*')
            regex.Append("[^/]*");
        else if (glob[index] == '?')
            regex.Append("[^/]");
        else
            regex.Append(Regex.Escape(glob[index].ToString()));
    }
}
