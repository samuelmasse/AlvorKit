namespace AlvorKit.Script.TestCoverage;

/// <summary>Creates branch coverage values from Coverlet branch JSON nodes.</summary>
internal static class BranchCoverageFactory
{
    /// <summary>Maps a Coverlet branch node into the internal branch hit shape.</summary>
    public static BranchCoverage Create(string module, string document, JsonObject branchNode)
    {
        var line = Value(branchNode, "Line");
        var offset = Value(branchNode, "Offset");
        var endOffset = Value(branchNode, "EndOffset");
        var path = Value(branchNode, "Path");
        var ordinal = Value(branchNode, "Ordinal");
        var branch = new BranchCoverage(module, document, line, offset, endOffset, path, ordinal)
        {
            Hits = Value(branchNode, "Hits")
        };

        return branch;
    }

    /// <summary>Reads an integer property from a Coverlet branch node.</summary>
    private static int Value(JsonObject node, string name) =>
        node[name]?.GetValue<int>() ?? 0;
}
