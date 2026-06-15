using System.Security.Cryptography;

namespace AlvorKit.Script.BindgenReview;

/// <summary>Builds and validates paths used by disposable bindgen review snapshots.</summary>
internal static class BindgenReviewPaths
{
    /// <summary>Repository-relative base directory for disposable bindgen review snapshots.</summary>
    public const string ReviewRootRelative = "out/bindgen-review";

    /// <summary>Creates a unique review session path under <c>out/bindgen-review</c>.</summary>
    public static BindgenReviewSession Create(string repoRoot, string library, string? caseName, Func<string> suffixFactory)
    {
        var suffix = suffixFactory();
        ValidateSuffix(suffix);
        var directoryName = DirectoryName(library, caseName, suffix);
        var relativeRoot = Path.Combine(ReviewRootRelative, directoryName);
        return FromAbsoluteRoot(repoRoot, Path.GetFullPath(Path.Combine(repoRoot, relativeRoot)));
    }

    /// <summary>Resolves and validates an existing review session path.</summary>
    public static BindgenReviewSession Existing(string repoRoot, string reviewRoot) =>
        FromAbsoluteRoot(repoRoot, ResolveInsideReviewRoot(repoRoot, reviewRoot));

    /// <summary>Creates a random five-character lowercase alphanumeric suffix.</summary>
    public static string RandomSuffix()
    {
        const string alphabet = "abcdefghijklmnopqrstuvwxyz0123456789";
        Span<byte> bytes = stackalloc byte[5];
        RandomNumberGenerator.Fill(bytes);
        var chars = new char[5];
        for (var index = 0; index < chars.Length; index++)
            chars[index] = alphabet[bytes[index] % alphabet.Length];

        return new(chars);
    }

    /// <summary>Builds a session object from an absolute root already known to be safe.</summary>
    private static BindgenReviewSession FromAbsoluteRoot(string repoRoot, string root)
    {
        var before = Path.Combine(root, "before");
        var after = Path.Combine(root, "after");
        return new(root, Relative(repoRoot, root), before, Relative(repoRoot, before), after, Relative(repoRoot, after));
    }

    /// <summary>Resolves a caller-supplied review root and requires it to stay under the review base directory.</summary>
    private static string ResolveInsideReviewRoot(string repoRoot, string reviewRoot)
    {
        var resolved = Path.GetFullPath(Path.Combine(repoRoot, reviewRoot));
        var reviewBase = Path.GetFullPath(Path.Combine(repoRoot, ReviewRootRelative));
        if (!IsInsideOrEqual(resolved, reviewBase))
            throw new InvalidOperationException($"Review root must stay under {ReviewRootRelative}.");

        return resolved;
    }

    /// <summary>Builds a readable directory name from the selected library, case, and random suffix.</summary>
    private static string DirectoryName(string library, string? caseName, string suffix)
    {
        var librarySlug = Slug(library);
        var caseSlug = Slug(string.IsNullOrWhiteSpace(caseName) ? library : caseName);
        return caseSlug == librarySlug
            ? $"{librarySlug}-{suffix}"
            : $"{librarySlug}-{caseSlug}-{suffix}";
    }

    /// <summary>Converts arbitrary human text into a stable lowercase path segment.</summary>
    private static string Slug(string text)
    {
        var builder = new StringBuilder();
        var previousDash = false;
        foreach (var character in text.Trim())
            AppendSlugCharacter(builder, character, ref previousDash);

        var slug = builder.ToString().Trim('-');
        return slug.Length == 0 ? "case" : slug;
    }

    /// <summary>Appends one normalized character to a slug builder.</summary>
    private static void AppendSlugCharacter(StringBuilder builder, char character, ref bool previousDash)
    {
        var normalized = NormalizeAscii(character);
        if (normalized is not null)
        {
            builder.Append(normalized.Value);
            previousDash = false;
        }
        else if (!previousDash)
        {
            builder.Append('-');
            previousDash = true;
        }
    }

    /// <summary>Returns a lowercase ASCII letter or digit when the input is safe for path slugs.</summary>
    private static char? NormalizeAscii(char character)
    {
        if (character is >= 'a' and <= 'z' or >= '0' and <= '9')
            return character;
        if (character is >= 'A' and <= 'Z')
            return (char)(character - 'A' + 'a');

        return null;
    }

    /// <summary>Validates the random suffix contract promised by the helper.</summary>
    private static void ValidateSuffix(string suffix)
    {
        if (suffix.Length != 5 || suffix.Any(character => character is not (>= 'a' and <= 'z' or >= '0' and <= '9')))
            throw new InvalidOperationException("Bindgen review suffixes must be five lowercase alphanumeric characters.");
    }

    /// <summary>Returns true when a resolved path is the expected directory or one of its descendants.</summary>
    private static bool IsInsideOrEqual(string path, string directory)
    {
        var relative = Path.GetRelativePath(directory, path);
        return relative == "."
            || (relative != ".."
                && !relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                && !relative.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal)
                && !Path.IsPathRooted(relative));
    }

    /// <summary>Returns a repository-relative path with forward slashes for command output and git args.</summary>
    private static string Relative(string repoRoot, string path) =>
        Path.GetRelativePath(repoRoot, path).Replace(Path.DirectorySeparatorChar, '/');
}
