namespace AlvorKit.Script.Lint.Test;

/// <summary>Tests repository-wide file policy discovery.</summary>
[TestClass]
public sealed class RepositoryPolicyTest
{
    /// <summary>Minimal valid manifest used by agent-policy graph tests.</summary>
    private const string ValidAgentPolicyManifest = """
        {
          "budgets": {
            "AGENTS.md": { "maxBytes": 1000, "maxLines": 1000 },
            "docs/GameRepositoryInstructions.md": { "maxBytes": 1000, "maxLines": 1000 }
          },
          "rootExposedRules": ["CORE-001"],
          "rules": [
            { "id": "CORE-001", "owner": "AGENTS.md" }
          ],
          "modules": [
            { "id": "CSHARP", "path": "docs/AgentRules/CSharp.md" }
          ],
          "overrides": []
        }
        """;

    /// <summary>Finds hand-authored AssemblyInfo files while ignoring generated and similarly named files.</summary>
    [TestMethod]
    public void FindAssemblyInfoFilesFindsOnlyHandAuthoredFiles()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("AssemblyInfo.cs", "public sealed class RootAssemblyInfo;");
        workspace.Write("src/Game/AssemblyInfo.cs", "public sealed class GameAssemblyInfo;");
        workspace.Write("src/Game/Game.AssemblyInfo.cs", "public sealed class GeneratedAssemblyInfo;");
        workspace.Write("obj/Game/AssemblyInfo.cs", "public sealed class IntermediateAssemblyInfo;");
        workspace.Write("tmp/AssemblyInfo.cs", "public sealed class TemporaryAssemblyInfo;");

        var files = RepositoryPolicy.FindAssemblyInfoFiles(workspace.Root);

        CollectionAssert.AreEqual(
            new[] { "AssemblyInfo.cs", "src/Game/AssemblyInfo.cs" },
            files.ToArray());
    }

    /// <summary>Restricts repository policy checks to selected files during scoped lint.</summary>
    [TestMethod]
    public void FindAssemblyInfoFilesHonorsLintScope()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("src/Game/AssemblyInfo.cs", "public sealed class GameAssemblyInfo;");
        workspace.Write("src/Game/Game.cs", "public sealed class Game;");
        var scope = LintScope.FromPatterns(workspace.Root, ["src/Game/Game.cs"]);

        var files = RepositoryPolicy.FindAssemblyInfoFiles(workspace.Root, scope);

        Assert.IsEmpty(files);
    }

    /// <summary>Finds checked expressions in C# source and source templates without matching prose or string literals.</summary>
    [TestMethod]
    public void FindCheckedKeywordUsagesFindsOnlyCode()
    {
        using var workspace = TempWorkspace.Create();
        var keyword = "check" + "ed";
        workspace.Write("src/Game/Game.cs", $"var value = {keyword}(1 + 1);");
        workspace.Write("res/templates/value.csfrag.tmpl", $"return {keyword}((uint)value);");
        workspace.Write("tests/GameTest.cs", $"var source = \"{keyword}(value)\"; // {keyword}(value)");

        var usages = RepositoryPolicy.FindCheckedKeywordUsages(workspace.Root);

        CollectionAssert.AreEqual(
            new[]
            {
                new RepositoryKeywordUsage("res/templates/value.csfrag.tmpl", 1, 8),
                new RepositoryKeywordUsage("src/Game/Game.cs", 1, 13),
            },
            usages.ToArray());
    }

    /// <summary>Restricts checked keyword discovery to the selected C# lint scope.</summary>
    [TestMethod]
    public void FindCheckedKeywordUsagesHonorsLintScope()
    {
        using var workspace = TempWorkspace.Create();
        var keyword = "check" + "ed";
        workspace.Write("src/Game/A.cs", $"var first = {keyword}(1 + 1);");
        workspace.Write("src/Game/B.cs", $"var second = {keyword}(2 + 2);");
        var scope = LintScope.FromPatterns(workspace.Root, ["src/Game/B.cs"]);

        var usages = RepositoryPolicy.FindCheckedKeywordUsages(workspace.Root, scope);

        CollectionAssert.AreEqual(
            new[] { new RepositoryKeywordUsage("src/Game/B.cs", 1, 14) },
            usages.ToArray());
    }

    /// <summary>Accepts a complete, budgeted agent-policy graph with an inert starter payload.</summary>
    [TestMethod]
    public void FindAgentPolicyViolationsAcceptsValidGraph()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("AGENTS.md", "# Instructions\n\nCORE-001\n");
        workspace.Write("docs/GameRepositoryInstructions.md", "# Games\n");
        workspace.Write("docs/AgentRules/CSharp.md", "# C#\n\n## Scope\n\nC# source.\n");
        workspace.Write("res/templates/new-game/source/AGENTS.md.template", "# Generated instructions\n");
        workspace.Write("docs/AgentRules/RuleManifest.json", ValidAgentPolicyManifest);

        var violations = RepositoryPolicy.FindAgentPolicyViolations(workspace.Root);

        Assert.IsEmpty(violations);
    }

    /// <summary>Reports missing modules, hidden root rules, exceeded budgets, and active template payloads.</summary>
    [TestMethod]
    public void FindAgentPolicyViolationsReportsInvalidGraph()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("AGENTS.md", "# Instructions that exceed the declared budget\n");
        workspace.Write("docs/GameRepositoryInstructions.md", "# Games\n");
        workspace.Write("res/templates/new-game/source/AGENTS.md", "# Accidentally active\n");
        workspace.Write("docs/AgentRules/RuleManifest.json", ValidAgentPolicyManifest.Replace("1000", "10", StringComparison.Ordinal));

        var violations = RepositoryPolicy.FindAgentPolicyViolations(workspace.Root);

        Assert.IsTrue(violations.Any(value => value.Contains("exceeds", StringComparison.Ordinal)));
        Assert.IsTrue(violations.Any(value => value.Contains("registered agent-policy module does not exist", StringComparison.Ordinal)));
        Assert.IsTrue(violations.Any(value => value.Contains("CORE-001", StringComparison.Ordinal)));
        Assert.IsTrue(violations.Any(value => value.Contains("must be inert", StringComparison.Ordinal)));
        Assert.IsTrue(violations.Any(value => value.Contains("inert instruction payload is missing", StringComparison.Ordinal)));
    }

    /// <summary>Reports broken policy links and overrides that reference unregistered rules.</summary>
    [TestMethod]
    public void FindAgentPolicyViolationsReportsBrokenLinksAndOverrides()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("AGENTS.md", "# Instructions\n\nCORE-001\n");
        workspace.Write("docs/GameRepositoryInstructions.md", "# Games\n");
        workspace.Write("docs/AgentRules/CSharp.md", "# C#\n\n## Scope\n\n[Missing](../Missing.md)\n");
        workspace.Write("res/templates/new-game/source/AGENTS.md.template", "# Generated instructions\n");
        var manifest = ValidAgentPolicyManifest.Replace(
            "\"overrides\": []",
            "\"overrides\": [{ \"rule\": \"CORE-001\", \"overrides\": \"MISSING-001\" }]",
            StringComparison.Ordinal);
        workspace.Write("docs/AgentRules/RuleManifest.json", manifest);

        var violations = RepositoryPolicy.FindAgentPolicyViolations(workspace.Root);

        Assert.IsTrue(violations.Any(value => value.Contains("Markdown link target", StringComparison.Ordinal)));
        Assert.IsTrue(violations.Any(value => value.Contains("override target 'MISSING-001'", StringComparison.Ordinal)));
    }

}
