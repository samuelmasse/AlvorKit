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
}
