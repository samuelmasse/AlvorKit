namespace AlvorKit.Script.Lint.Test;

/// <summary>Tests repository-wide file policy discovery.</summary>
[TestClass]
public sealed class RepositoryPolicyTest
{
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
}
