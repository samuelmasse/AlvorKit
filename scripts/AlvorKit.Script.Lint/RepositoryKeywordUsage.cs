namespace AlvorKit;

/// <summary>Location of a prohibited C# keyword in repository-owned source.</summary>
/// <param name="File">Repository-relative source file.</param>
/// <param name="Line">One-based source line.</param>
/// <param name="Column">One-based source column.</param>
internal readonly record struct RepositoryKeywordUsage(string File, int Line, int Column)
{
    /// <summary>Formats the location for command-line diagnostics.</summary>
    public override string ToString() => $"{File}({Line},{Column})";
}
