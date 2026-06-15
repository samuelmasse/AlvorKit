namespace AlvorKit.Script.AgentLease;

/// <summary>Valid coordination modes understood by the lease helper.</summary>
internal static class AgentLeaseModes
{
    /// <summary>Default mode for ordinary edits.</summary>
    public const string Default = "write";

    /// <summary>Known mode names for lease validation and documentation.</summary>
    public static IReadOnlySet<string> Known { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "write",
        "generate",
        "format",
        "test",
        "cleanup",
        "review"
    };

    /// <summary>Normalizes a mode value for stable JSON output.</summary>
    /// <param name="mode">Mode value supplied by command-line input or an existing lease.</param>
    public static string Normalize(string mode) => mode.Trim().ToLowerInvariant();

    /// <summary>Returns whether a mode is one of the supported coordination modes.</summary>
    /// <param name="mode">Mode value to validate.</param>
    public static bool IsKnown(string mode) => Known.Contains(mode.Trim());
}
