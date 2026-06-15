namespace AlvorKit.Script.Lint;

/// <summary>Matches repository-relative paths against the limited glob syntax used by lint scopes.</summary>
internal static class GlobPattern
{
    /// <summary>Tests whether a repository-relative path matches a glob pattern.</summary>
    public static bool Matches(string relativePath, string glob) =>
        ToRegex(glob).IsMatch(NormalizePath(relativePath));

    /// <summary>Normalizes a repository-relative path for stable glob matching and command arguments.</summary>
    public static string NormalizePath(string path)
    {
        var normalized = path.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');
        while (normalized.StartsWith("./", StringComparison.Ordinal))
            normalized = normalized[2..];
        return normalized.TrimStart('/');
    }

    /// <summary>Converts a supported repository glob to a regular expression.</summary>
    public static Regex ToRegex(string glob)
    {
        var pattern = NormalizePath(glob);
        var regex = new StringBuilder("^");
        for (var i = 0; i < pattern.Length; i++)
            AppendToken(regex, pattern, ref i);

        regex.Append('$');
        return new(regex.ToString(), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    /// <summary>Appends the regular expression equivalent for one glob token.</summary>
    private static void AppendToken(StringBuilder regex, string pattern, ref int index)
    {
        if (pattern[index] == '*' && index + 1 < pattern.Length && pattern[index + 1] == '*')
        {
            var followedBySlash = index + 2 < pattern.Length && pattern[index + 2] == '/';
            regex.Append(followedBySlash ? "(?:.*/)?" : ".*");
            index += followedBySlash ? 2 : 1;
        }
        else if (pattern[index] == '*')
            regex.Append("[^/]*");
        else if (pattern[index] == '?')
            regex.Append("[^/]");
        else
            regex.Append(Regex.Escape(pattern[index].ToString()));
    }
}
