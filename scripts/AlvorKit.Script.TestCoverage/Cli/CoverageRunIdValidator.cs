namespace AlvorKit.Script.TestCoverage;

/// <summary>Validates coverage run identifiers before they are used as output directory names.</summary>
internal static class CoverageRunIdValidator
{
    /// <summary>Portable filename characters rejected by common repository hosts and developer platforms.</summary>
    private static readonly char[] PortableInvalidChars = ['/', '\\', ':', '*', '?', '"', '<', '>', '|'];

    /// <summary>Rejects run IDs that cannot safely be used as one directory name.</summary>
    public static void Validate(string value)
    {
        if (value.Length == 0)
            throw new ArgumentException("Run ID must not be empty.");
        if (value.IndexOfAny(PortableInvalidChars) >= 0 || value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new ArgumentException("Run ID must be a single valid directory name.");
    }
}
