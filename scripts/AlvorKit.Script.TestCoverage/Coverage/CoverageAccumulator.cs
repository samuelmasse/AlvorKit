using System.Globalization;
using System.Text.Json.Nodes;

namespace AlvorKit.Script.TestCoverage;

/// <summary>Aggregates raw Coverlet JSON files into repository-level coverage counts.</summary>
internal sealed class CoverageAccumulator
{
    /// <summary>Line coverage keyed by module, document, and line number.</summary>
    private readonly Dictionary<string, LineCoverage> lines = new(StringComparer.Ordinal);

    /// <summary>Branch coverage keyed by module, document, line, offsets, path, and ordinal.</summary>
    private readonly Dictionary<string, BranchCoverage> branches = new(StringComparer.Ordinal);

    /// <summary>Method coverage keyed by module, document, type, and method signature.</summary>
    private readonly Dictionary<string, MethodCoverage> methods = new(StringComparer.Ordinal);

    /// <summary>Adds one Coverlet JSON report to the aggregate counts.</summary>
    public void AddCoverletJson(string path, string repoRoot)
    {
        var root = JsonNode.Parse(File.ReadAllText(path))?.AsObject()
            ?? throw new InvalidOperationException($"Coverage report '{path}' is not valid JSON.");

        foreach (var module in root)
            AddModule(module.Key, module.Value?.AsObject(), repoRoot);
    }

    /// <summary>Builds the final coverage summary for the expected source modules.</summary>
    public CoverageSummary BuildSummary(IReadOnlyCollection<string> expectedModules)
    {
        var modules = lines.Values.Select(line => line.Module)
            .Concat(branches.Values.Select(branch => branch.Module))
            .Concat(methods.Values.Select(method => method.Module))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .Select(BuildModuleSummary)
            .ToArray();
        var files = BuildFileSummaries();
        var unmeasuredModules = expectedModules
            .Except(modules.Select(module => module.Name), StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return new(CoverageSummaryFactory.BuildTotals(lines.Values, branches.Values, methods.Values), modules, unmeasuredModules, files);
    }

    /// <summary>Adds all documents for one measured module.</summary>
    private void AddModule(string modulePath, JsonObject? documents, string repoRoot)
    {
        if (documents is null)
            return;

        var moduleName = Path.GetFileNameWithoutExtension(modulePath);
        foreach (var document in documents)
            AddDocument(moduleName, CoveragePath.NormalizeDocument(document.Key, repoRoot), document.Value?.AsObject());
    }

    /// <summary>Adds all types and methods for one measured source document.</summary>
    private void AddDocument(string module, string document, JsonObject? classes)
    {
        if (classes is null)
            return;

        foreach (var type in classes)
        {
            if (type.Value is not JsonObject methodNodes)
                continue;

            foreach (var method in methodNodes)
                AddMethod(module, document, type.Key, method.Key, method.Value?.AsObject());
        }
    }

    /// <summary>Adds line, method, and branch hits for one method node.</summary>
    private void AddMethod(string module, string document, string type, string method, JsonObject? methodNode)
    {
        if (methodNode is null)
            return;

        var lineHits = AddLines(module, document, methodNode["Lines"] as JsonObject);
        AddMethodHits(module, document, type, method, lineHits);
        AddBranches(module, document, methodNode["Branches"] as JsonArray);
    }

    /// <summary>Adds line hits and returns their total for method coverage.</summary>
    private int AddLines(string module, string document, JsonObject? lineNodes)
    {
        var lineHits = 0;
        if (lineNodes is null)
            return lineHits;

        foreach (var line in lineNodes)
        {
            var hits = line.Value?.GetValue<int>() ?? 0;
            lineHits += hits;
            var lineKey = $"{module}|{document}|{line.Key}";

            if (!lines.TryGetValue(lineKey, out var lineCoverage))
                lines.Add(lineKey, lineCoverage = new(module, document, int.Parse(line.Key, CultureInfo.InvariantCulture)));

            lineCoverage.Hits += hits;
        }

        return lineHits;
    }

    /// <summary>Adds method hits using the summed line hits for the method body.</summary>
    private void AddMethodHits(string module, string document, string type, string method, int lineHits)
    {
        var methodKey = $"{module}|{document}|{type}|{method}";
        if (!methods.TryGetValue(methodKey, out var methodCoverage))
            methods.Add(methodKey, methodCoverage = new(module, document, type, method));

        methodCoverage.Hits += lineHits;
    }

    /// <summary>Adds branch hits for one method node.</summary>
    private void AddBranches(string module, string document, JsonArray? branchNodes)
    {
        if (branchNodes is null)
            return;

        foreach (var branchNode in branchNodes.OfType<JsonObject>())
        {
            var branch = BranchCoverageFactory.Create(module, document, branchNode);
            var branchKey = $"{module}|{document}|{branch.Line}|{branch.Offset}|{branch.EndOffset}|{branch.Path}|{branch.Ordinal}";

            if (!branches.TryGetValue(branchKey, out var existingBranch))
                branches.Add(branchKey, existingBranch = branch);

            existingBranch.Hits += branch.Hits;
        }
    }

    /// <summary>Builds summaries for files with measured lines, branches, or methods.</summary>
    private FileCoverageSummary[] BuildFileSummaries() =>
    [
        .. lines.Values.Select(line => line.Document)
            .Concat(branches.Values.Select(branch => branch.Document))
            .Concat(methods.Values.Select(method => method.Document))
            .Distinct(StringComparer.Ordinal)
            .OrderByDescending(document => lines.Values.Count(line => line.Document == document && line.Hits == 0))
            .ThenBy(document => document, StringComparer.Ordinal)
            .Select(document => CoverageSummaryFactory.BuildFile(document, lines.Values, branches.Values, methods.Values))
    ];

    /// <summary>Builds the coverage summary for one module.</summary>
    private ModuleCoverageSummary BuildModuleSummary(string module) =>
        CoverageSummaryFactory.BuildModule(module, lines.Values, branches.Values, methods.Values);
}
