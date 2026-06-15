namespace AlvorKit.Script.Lint.Test;

/// <summary>Tests command planning for repository lint checks.</summary>
[TestClass]
public sealed class LintPlanTest
{
    /// <summary>Discovers source, script, and test projects while ignoring other repo folders.</summary>
    [TestMethod]
    public void DiscoverProjectsUsesLintedProjectRootsOnly()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("src/App/App.csproj", "<Project />");
        workspace.Write("scripts/Tool/Tool.csproj", "<Project />");
        workspace.Write("tests/App.Test/App.Test.csproj", "<Project />");
        workspace.Write("demos/Demo/Demo.csproj", "<Project />");

        var projects = LintPlan.DiscoverProjects(workspace.Root);

        CollectionAssert.AreEqual(
            new[]
            {
                "scripts/Tool/Tool.csproj",
                "src/App/App.csproj",
                "tests/App.Test/App.Test.csproj",
            },
            projects.ToArray());
    }

    /// <summary>Plans dotnet format in verify mode by default.</summary>
    [TestMethod]
    public void DotNetFormatCommandsVerifyNoChanges()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("scripts/Tool/Tool.csproj", "<Project />");

        var command = LintPlan.DotNetFormatCommands(workspace.Root, fix: false).Single();

        CollectionAssert.AreEqual(
            new[] { "format", "scripts/Tool/Tool.csproj", "--verify-no-changes", "--verbosity", "minimal" },
            command.Arguments.ToArray());
        Assert.AreEqual(workspace.Root, command.WorkingDirectory);
        Assert.AreEqual("dotnet format scripts/Tool/Tool.csproj", command.Label);
    }

    /// <summary>Plans dotnet format in write mode when requested.</summary>
    [TestMethod]
    public void DotNetFormatCommandsCanWriteChanges()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("scripts/Tool/Tool.csproj", "<Project />");

        var command = LintPlan.DotNetFormatCommands(workspace.Root, fix: true).Single();

        CollectionAssert.AreEqual(
            new[] { "format", "scripts/Tool/Tool.csproj", "--verbosity", "minimal" },
            command.Arguments.ToArray());
    }

    /// <summary>Combines project formatting, Prettier, and EditorConfig into the pre-actionlint command plan.</summary>
    [TestMethod]
    public void CommandsBeforeActionlintIncludeAllPreChecks()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("scripts/Tool/Tool.csproj", "<Project />");

        var commands = LintPlan.CommandsBeforeActionlint(workspace.Root, fix: false);

        Assert.AreEqual(3, commands.Count);
        Assert.AreEqual("dotnet", commands[0].FileName);
        CollectionAssert.Contains(commands[1].Arguments.ToArray(), "prettier@3");
        CollectionAssert.Contains(commands[2].Arguments.ToArray(), "editorconfig-checker@6.1.1");
    }

    /// <summary>Plans Prettier for the JSON, YAML, and Markdown globs owned by the repo policy.</summary>
    [TestMethod]
    public void PrettierCommandIncludesMarkdownAndVsCodeJson()
    {
        var command = LintPlan.PrettierCommand("repo", fix: false);

        CollectionAssert.Contains(command.Arguments.ToArray(), "--check");
        CollectionAssert.Contains(command.Arguments.ToArray(), ".vscode/*.json");
        CollectionAssert.Contains(command.Arguments.ToArray(), "native/**/*.md");
        CollectionAssert.Contains(command.Arguments.ToArray(), "*.md");
    }

    /// <summary>Plans Prettier in write mode when requested.</summary>
    [TestMethod]
    public void PrettierCommandCanWriteChanges()
    {
        var command = LintPlan.PrettierCommand("repo", fix: true);

        CollectionAssert.Contains(command.Arguments.ToArray(), "--write");
    }

    /// <summary>Plans the EditorConfig checker without indentation checks that conflict with formatter alignment.</summary>
    [TestMethod]
    public void EditorConfigCommandDisablesIndentation()
    {
        var command = LintPlan.EditorConfigCommand("repo");

        CollectionAssert.Contains(command.Arguments.ToArray(), "editorconfig-checker@6.1.1");
        CollectionAssert.Contains(command.Arguments.ToArray(), "-disable-indentation");
        CollectionAssert.Contains(command.Arguments.ToArray(), "github-actions");
    }

    /// <summary>Plans actionlint with color output enabled.</summary>
    [TestMethod]
    public void ActionlintCommandUsesResolvedPath()
    {
        var command = LintPlan.ActionlintCommand("repo", "actionlint");

        Assert.AreEqual("actionlint", command.FileName);
        CollectionAssert.AreEqual(new[] { "-color" }, command.Arguments.ToArray());
    }
}
