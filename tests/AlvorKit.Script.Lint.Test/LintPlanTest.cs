namespace AlvorKit.Script.Lint.Test;

/// <summary>Tests command planning for repository lint checks.</summary>
[TestClass]
public sealed class LintPlanTest
{
    /// <summary>Plans solution-wide dotnet format and info-level code style checks in verify mode by default.</summary>
    [TestMethod]
    public void DotNetFormatCommandsVerifyNoChanges()
    {
        using var workspace = TempWorkspace.Create();

        var commands = LintPlan.DotNetFormatCommands(workspace.Root, fix: false).ToArray();

        Assert.AreEqual(2, commands.Length);
        CollectionAssert.AreEqual(
            new[] { "format", "AlvorKit.slnx", "--verify-no-changes", "--verbosity", "minimal" },
            commands[0].Arguments.ToArray());
        CollectionAssert.AreEqual(
            new[] { "format", "style", "AlvorKit.slnx", "--verify-no-changes", "--severity", "info", "--verbosity", "minimal" },
            commands[1].Arguments.ToArray());
        Assert.AreEqual(workspace.Root, commands[0].WorkingDirectory);
        Assert.AreEqual("dotnet format AlvorKit.slnx", commands[0].Label);
        Assert.AreEqual("dotnet format style AlvorKit.slnx", commands[1].Label);
    }

    /// <summary>Plans solution-wide dotnet format and info-level code style checks in write mode when requested.</summary>
    [TestMethod]
    public void DotNetFormatCommandsCanWriteChanges()
    {
        using var workspace = TempWorkspace.Create();

        var commands = LintPlan.DotNetFormatCommands(workspace.Root, fix: true).ToArray();

        Assert.AreEqual(2, commands.Length);
        CollectionAssert.AreEqual(
            new[] { "format", "AlvorKit.slnx", "--verbosity", "minimal" },
            commands[0].Arguments.ToArray());
        CollectionAssert.AreEqual(
            new[] { "format", "style", "AlvorKit.slnx", "--severity", "info", "--verbosity", "minimal" },
            commands[1].Arguments.ToArray());
    }

    /// <summary>Plans dotnet format for scoped C# files under their owning project.</summary>
    [TestMethod]
    public void DotNetFormatCommandsCanScopeIncludedFiles()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("scripts/Tool/Tool.csproj", "<Project />");
        workspace.Write("scripts/Tool/A.cs", "namespace Tool;");
        var scope = LintScope.FromPatterns(workspace.Root, ["scripts/Tool/A.cs"]);

        var commands = LintPlan.DotNetFormatCommands(workspace.Root, fix: false, scope).ToArray();

        Assert.AreEqual(2, commands.Length);
        CollectionAssert.AreEqual(
            new[]
            {
                "format",
                "scripts/Tool/Tool.csproj",
                "--verify-no-changes",
                "--verbosity",
                "minimal",
                "--include",
                "scripts/Tool/A.cs",
            },
            commands[0].Arguments.ToArray());
        CollectionAssert.AreEqual(
            new[]
            {
                "format",
                "style",
                "scripts/Tool/Tool.csproj",
                "--verify-no-changes",
                "--severity",
                "info",
                "--verbosity",
                "minimal",
                "--include",
                "scripts/Tool/A.cs",
            },
            commands[1].Arguments.ToArray());
    }

    /// <summary>Finds the nearest owning project for scoped C# files in nested directories.</summary>
    [TestMethod]
    public void DotNetFormatCommandsFindNestedOwningProject()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("scripts/Tool/Tool.csproj", "<Project />");
        workspace.Write("scripts/Tool/Sub/A.cs", "namespace Tool;");
        var scope = LintScope.FromPatterns(workspace.Root, ["scripts/Tool/Sub/A.cs"]);

        var commands = LintPlan.DotNetFormatCommands(workspace.Root, fix: false, scope).ToArray();

        Assert.AreEqual(2, commands.Length);
        Assert.AreEqual("dotnet format scripts/Tool/Tool.csproj", commands[0].Label);
        Assert.AreEqual("dotnet format style scripts/Tool/Tool.csproj", commands[1].Label);
    }

    /// <summary>Rejects scoped C# files in a directory with multiple project files.</summary>
    [TestMethod]
    public void DotNetFormatCommandsRejectAmbiguousProjectDirectory()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("scripts/Tool/A.csproj", "<Project />");
        workspace.Write("scripts/Tool/B.csproj", "<Project />");
        workspace.Write("scripts/Tool/File.cs", "namespace Tool;");
        var scope = LintScope.FromPatterns(workspace.Root, ["scripts/Tool/File.cs"]);

        Assert.ThrowsException<InvalidOperationException>(() => LintPlan.DotNetFormatCommands(workspace.Root, fix: false, scope));
    }

    /// <summary>Rejects scoped C# files that are not under a project file.</summary>
    [TestMethod]
    public void DotNetFormatCommandsRejectUnownedCSharpFile()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("Loose.cs", "namespace Tool;");
        var scope = LintScope.FromPatterns(workspace.Root, ["Loose.cs"]);

        Assert.ThrowsException<InvalidOperationException>(() => LintPlan.DotNetFormatCommands(workspace.Root, fix: false, scope));
    }

    /// <summary>Combines solution formatting, Prettier, and EditorConfig into the pre-actionlint command plan.</summary>
    [TestMethod]
    public void CommandsBeforeActionlintIncludeAllPreChecks()
    {
        using var workspace = TempWorkspace.Create();

        var commands = LintPlan.CommandsBeforeActionlint(workspace.Root, fix: false);

        Assert.AreEqual(4, commands.Count);
        Assert.AreEqual("dotnet", commands[0].FileName);
        Assert.AreEqual("dotnet format AlvorKit.slnx", commands[0].Label);
        Assert.AreEqual("dotnet format style AlvorKit.slnx", commands[1].Label);
        CollectionAssert.Contains(commands[2].Arguments.ToArray(), "prettier@3");
        CollectionAssert.Contains(commands[3].Arguments.ToArray(), "editorconfig-checker@6.1.1");
    }

    /// <summary>Combines only the checks needed for scoped files before actionlint.</summary>
    [TestMethod]
    public void CommandsBeforeActionlintCanUseScopedFiles()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("scripts/Tool/Tool.csproj", "<Project />");
        workspace.Write("scripts/Tool/A.cs", "namespace Tool;");
        var scope = LintScope.FromPatterns(workspace.Root, ["scripts/Tool/A.cs"]);

        var commands = LintPlan.CommandsBeforeActionlint(workspace.Root, fix: false, scope);

        Assert.AreEqual(3, commands.Count);
        Assert.AreEqual("dotnet format scripts/Tool/Tool.csproj", commands[0].Label);
        Assert.AreEqual("dotnet format style scripts/Tool/Tool.csproj", commands[1].Label);
        Assert.AreEqual("editorconfig", commands[2].Label);
        CollectionAssert.Contains(commands[2].Arguments.ToArray(), "scripts/Tool/A.cs");
    }

    /// <summary>Returns no pre-actionlint commands when scoped includes resolve to no files.</summary>
    [TestMethod]
    public void CommandsBeforeActionlintReturnsNoCommandsForEmptyScope()
    {
        using var workspace = TempWorkspace.Create();
        var scope = LintScope.FromPatterns(workspace.Root, ["Deleted.cs"]);

        var commands = LintPlan.CommandsBeforeActionlint(workspace.Root, fix: false, scope);

        Assert.AreEqual(0, commands.Count);
    }

    /// <summary>Includes scoped Prettier checks in the combined pre-actionlint plan when needed.</summary>
    [TestMethod]
    public void CommandsBeforeActionlintIncludesScopedPrettier()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("AGENTS.md", "# Agents");
        var scope = LintScope.FromPatterns(workspace.Root, ["AGENTS.md"]);

        var commands = LintPlan.CommandsBeforeActionlint(workspace.Root, fix: false, scope);

        Assert.AreEqual(2, commands.Count);
        Assert.AreEqual("prettier", commands[0].Label);
        Assert.AreEqual("editorconfig", commands[1].Label);
    }

    /// <summary>Plans Prettier for the JSON, YAML, and Markdown globs owned by the repo policy.</summary>
    [TestMethod]
    public void PrettierCommandIncludesMarkdownNativeYamlAndVsCodeJson()
    {
        var command = LintPlan.PrettierCommand("repo", fix: false);

        CollectionAssert.Contains(command.Arguments.ToArray(), "--check");
        CollectionAssert.Contains(command.Arguments.ToArray(), ".vscode/*.json");
        CollectionAssert.Contains(command.Arguments.ToArray(), "native/**/*.yml");
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

    /// <summary>Plans Prettier only for scoped files covered by the Prettier policy.</summary>
    [TestMethod]
    public void PrettierCommandCanScopeSelectedFiles()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("AGENTS.md", "# Agents");
        workspace.Write("scripts/Tool/A.cs", "namespace Tool;");
        var scope = LintScope.FromPatterns(workspace.Root, ["AGENTS.md", "scripts/Tool/A.cs"]);

        var command = LintPlan.PrettierCommand(workspace.Root, fix: false, scope);

        Assert.IsNotNull(command);
        CollectionAssert.AreEqual(
            new[] { "--yes", "prettier@3", "--check", "AGENTS.md" },
            command.Arguments.ToArray());
    }

    /// <summary>Skips scoped Prettier when no selected files are covered by the Prettier policy.</summary>
    [TestMethod]
    public void PrettierCommandReturnsNullWithoutScopedPrettierFiles()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("scripts/Tool/A.cs", "namespace Tool;");
        var scope = LintScope.FromPatterns(workspace.Root, ["scripts/Tool/A.cs"]);

        var command = LintPlan.PrettierCommand(workspace.Root, fix: false, scope);

        Assert.IsNull(command);
    }

    /// <summary>Plans scoped Prettier in write mode when requested.</summary>
    [TestMethod]
    public void PrettierCommandCanWriteScopedFiles()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("AGENTS.md", "# Agents");
        var scope = LintScope.FromPatterns(workspace.Root, ["AGENTS.md"]);

        var command = LintPlan.PrettierCommand(workspace.Root, fix: true, scope);

        Assert.IsNotNull(command);
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

    /// <summary>Plans EditorConfig checker for exact scoped files.</summary>
    [TestMethod]
    public void EditorConfigCommandCanScopeSelectedFiles()
    {
        var command = LintPlan.EditorConfigCommand("repo", ["AGENTS.md", "scripts/Tool/A.cs"]);

        CollectionAssert.Contains(command.Arguments.ToArray(), "AGENTS.md");
        CollectionAssert.Contains(command.Arguments.ToArray(), "scripts/Tool/A.cs");
    }

    /// <summary>Plans actionlint with color output enabled.</summary>
    [TestMethod]
    public void ActionlintCommandUsesResolvedPath()
    {
        var command = LintPlan.ActionlintCommand("repo", "actionlint");

        Assert.AreEqual("actionlint", command.FileName);
        CollectionAssert.AreEqual(new[] { "-color" }, command.Arguments.ToArray());
    }

    /// <summary>Plans actionlint for exact scoped workflow files.</summary>
    [TestMethod]
    public void ActionlintCommandCanScopeSelectedWorkflowFiles()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write(".github/workflows/build.yml", "name: Build");
        var scope = LintScope.FromPatterns(workspace.Root, [".github/workflows/build.yml"]);

        var command = LintPlan.ActionlintCommand(workspace.Root, "actionlint", scope);

        CollectionAssert.AreEqual(new[] { "-color", ".github/workflows/build.yml" }, command.Arguments.ToArray());
    }

    /// <summary>Runs actionlint for repo-wide linting or when scoped workflow files are selected.</summary>
    [TestMethod]
    public void RequiresActionlintUsesScope()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("AGENTS.md", "# Agents");
        var scope = LintScope.FromPatterns(workspace.Root, ["AGENTS.md"]);

        Assert.IsTrue(LintPlan.RequiresActionlint(null));
        Assert.IsFalse(LintPlan.RequiresActionlint(scope));
    }
}
