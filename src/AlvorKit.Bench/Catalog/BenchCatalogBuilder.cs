namespace AlvorKit;

/// <summary>Flattens suite trees while preserving descriptions and baseline ordering.</summary>
[Bench]
public class BenchCatalogBuilder
{
    /// <summary>Flattens authored nodes and emits each baseline before its candidates.</summary>
    public BenchCatalog Create(BenchSuite suite)
    {
        List<BenchCase> cases = [];

        foreach (var child in suite.Children)
            AddNode(child, "", "", [], cases);

        return new(suite.Name, [.. cases]);
    }

    /// <summary>Inherits path and description metadata while visiting one subtree.</summary>
    private void AddNode(
        BenchNode node,
        string parentId,
        string inheritedDescription,
        string[] inheritedSegmentDescriptions,
        List<BenchCase> cases)
    {
        var id = AppendId(parentId, node.Name);
        var description = AppendDescription(inheritedDescription, node.Description);
        string[] segmentDescriptions = [.. inheritedSegmentDescriptions, node.Description];

        foreach (var child in node.Children)
            AddNode(child, id, description, segmentDescriptions, cases);

        if (node.Measurement is not null)
            cases.Add(CreateCase(id, description, segmentDescriptions, node.Measurement, BenchCaseRole.Measurement, null));

        if (node.Comparison is not null)
            AddComparison(node.Comparison, id, description, segmentDescriptions, cases);
    }

    /// <summary>Emits a baseline followed by candidates referencing its exact ID.</summary>
    private void AddComparison(
        BenchComparison comparison,
        string id,
        string description,
        string[] segmentDescriptions,
        List<BenchCase> cases)
    {
        var baselineId = AppendId(id, comparison.Baseline.Name);
        cases.Add(CreateCase(
            baselineId,
            description,
            [.. segmentDescriptions, ""],
            comparison.Baseline.Run,
            BenchCaseRole.Baseline,
            null));

        foreach (var candidate in comparison.Candidates)
        {
            var candidateId = AppendId(id, candidate.Name);
            cases.Add(CreateCase(
                candidateId,
                description,
                [.. segmentDescriptions, ""],
                candidate.Run,
                BenchCaseRole.Candidate,
                baselineId));
        }
    }

    /// <summary>Attaches segment descriptions to a runnable leaf.</summary>
    private BenchCase CreateCase(
        string id,
        string description,
        string[] segmentDescriptions,
        Func<BenchResult> measurement,
        BenchCaseRole role,
        string? baselineId) =>
        new(id, description, measurement, role, baselineId) { SegmentDescriptions = segmentDescriptions };

    /// <summary>Joins path segments without a leading separator.</summary>
    private string AppendId(string parentId, string name) => parentId.Length == 0 ? name : $"{parentId}/{name}";

    /// <summary>Combines nonempty inherited descriptions in hierarchy order.</summary>
    private string AppendDescription(string inheritedDescription, string description)
    {
        if (description.Length == 0)
            return inheritedDescription;

        return inheritedDescription.Length == 0 ? description : $"{inheritedDescription}; {description}";
    }
}
