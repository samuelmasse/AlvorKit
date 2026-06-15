namespace AlvorKit.Script.TestCoverage;

/// <summary>Mutable hit accumulator for a source line.</summary>
/// <param name="module">Assembly that owns the line.</param>
/// <param name="document">Repository-relative document path.</param>
/// <param name="line">One-based source line number.</param>
internal sealed class LineCoverage(string module, string document, int line)
{
    /// <summary>Assembly that owns the line.</summary>
    public string Module { get; } = module;

    /// <summary>Repository-relative document path.</summary>
    public string Document { get; } = document;

    /// <summary>One-based source line number.</summary>
    public int Line { get; } = line;

    /// <summary>Total hit count across all coverage reports.</summary>
    public int Hits { get; set; }
}

/// <summary>Mutable hit accumulator for a branch path.</summary>
/// <param name="module">Assembly that owns the branch.</param>
/// <param name="document">Repository-relative document path.</param>
/// <param name="line">Source line containing the branch.</param>
/// <param name="offset">IL branch offset reported by Coverlet.</param>
/// <param name="endOffset">IL branch end offset reported by Coverlet.</param>
/// <param name="path">Coverlet path identifier for the branch.</param>
/// <param name="ordinal">Coverlet ordinal identifier for the branch.</param>
internal sealed class BranchCoverage(string module, string document, int line, int offset, int endOffset, int path, int ordinal)
{
    /// <summary>Assembly that owns the branch.</summary>
    public string Module { get; } = module;

    /// <summary>Repository-relative document path.</summary>
    public string Document { get; } = document;

    /// <summary>Source line containing the branch.</summary>
    public int Line { get; } = line;

    /// <summary>IL branch offset reported by Coverlet.</summary>
    public int Offset { get; } = offset;

    /// <summary>IL branch end offset reported by Coverlet.</summary>
    public int EndOffset { get; } = endOffset;

    /// <summary>Coverlet path identifier for the branch.</summary>
    public int Path { get; } = path;

    /// <summary>Coverlet ordinal identifier for the branch.</summary>
    public int Ordinal { get; } = ordinal;

    /// <summary>Total hit count across all coverage reports.</summary>
    public int Hits { get; set; }
}

/// <summary>Mutable hit accumulator for a method body.</summary>
/// <param name="module">Assembly that owns the method.</param>
/// <param name="document">Repository-relative document path.</param>
/// <param name="type">Metadata type name reported by Coverlet.</param>
/// <param name="method">Metadata method signature reported by Coverlet.</param>
internal sealed class MethodCoverage(string module, string document, string type, string method)
{
    /// <summary>Assembly that owns the method.</summary>
    public string Module { get; } = module;

    /// <summary>Repository-relative document path.</summary>
    public string Document { get; } = document;

    /// <summary>Metadata type name reported by Coverlet.</summary>
    public string Type { get; } = type;

    /// <summary>Metadata method signature reported by Coverlet.</summary>
    public string Method { get; } = method;

    /// <summary>Total line hits inside the method across all coverage reports.</summary>
    public int Hits { get; set; }
}
