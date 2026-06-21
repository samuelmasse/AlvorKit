namespace AlvorKit.Script.Lint.Test;

/// <summary>Tests scoped lint file expansion and classification.</summary>
[TestClass]
public sealed class LintScopeTest
{
    /// <summary>Expands literal files, directories, and globs into stable repository-relative paths.</summary>
    [TestMethod]
    public void FromPatternsExpandsIncludes()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("AGENTS.md", "# Agents");
        workspace.Write("scripts/Tool/A.cs", "namespace Tool;");
        workspace.Write("scripts/Tool/B.txt", "notes");
        workspace.Write(".github/workflows/build.yml", "name: Build");

        var scope = LintScope.FromPatterns(workspace.Root, ["scripts/**/*.cs", "AGENTS.md", ".github/workflows"]);

        CollectionAssert.AreEqual(
            new[] { ".github/workflows/build.yml", "AGENTS.md", "scripts/Tool/A.cs" },
            scope.AllFiles.ToArray());
    }

    /// <summary>Classifies selected files by the tools that own their lint checks.</summary>
    [TestMethod]
    public void FromPatternsClassifiesSelectedFiles()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("AGENTS.md", "# Agents");
        workspace.Write("res/templates/README.md", "# Templates");
        workspace.Write("scripts/Tool/A.cs", "namespace Tool;");
        workspace.Write(".github/workflows/build.yml", "name: Build");

        var scope = LintScope.FromPatterns(workspace.Root, ["**/*"]);

        CollectionAssert.AreEqual(new[] { "scripts/Tool/A.cs" }, scope.CSharpFiles.ToArray());
        CollectionAssert.AreEqual(new[] { ".github/workflows/build.yml", "AGENTS.md", "res/templates/README.md" }, scope.PrettierFiles.ToArray());
        CollectionAssert.AreEqual(new[] { ".github/workflows/build.yml" }, scope.ActionlintFiles.ToArray());
    }

    /// <summary>Skips missing literal paths so deleted files do not make scoped lint fail.</summary>
    [TestMethod]
    public void FromPatternsSkipsMissingLiteralPaths()
    {
        using var workspace = TempWorkspace.Create();

        var scope = LintScope.FromPatterns(workspace.Root, ["scripts/Tool/Deleted.cs"]);

        Assert.IsTrue(scope.IsEmpty);
    }

    /// <summary>Skips generated and tool-output directories when expanding directory or glob scopes.</summary>
    [TestMethod]
    public void FromPatternsSkipsToolOutputDirectories()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("scripts/Tool/A.cs", "namespace Tool;");
        workspace.Write("scripts/Tool/bin/Generated.cs", "namespace Tool;");
        workspace.Write("scripts/Tool/obj/Generated.cs", "namespace Tool;");
        workspace.Write("scripts/Tool/out/Generated.cs", "namespace Tool;");
        workspace.Write("scripts/Tool/.git/config", "[core]");

        var scope = LintScope.FromPatterns(workspace.Root, ["scripts/Tool"]);

        CollectionAssert.AreEqual(new[] { "scripts/Tool/A.cs" }, scope.AllFiles.ToArray());
    }

    /// <summary>Accepts the repository root as a literal directory include.</summary>
    [TestMethod]
    public void FromPatternsAcceptsRepositoryRootLiteral()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("AGENTS.md", "# Agents");

        var scope = LintScope.FromPatterns(workspace.Root, ["."]);

        CollectionAssert.AreEqual(new[] { "AGENTS.md" }, scope.AllFiles.ToArray());
    }

    /// <summary>Rejects empty include patterns because they are almost always caller mistakes.</summary>
    [TestMethod]
    public void FromPatternsRejectsEmptyPattern()
    {
        using var workspace = TempWorkspace.Create();

        Assert.ThrowsException<ArgumentException>(() => LintScope.FromPatterns(workspace.Root, [""]));
    }

    /// <summary>Rejects scoped glob patterns that try to escape the repository.</summary>
    [TestMethod]
    public void FromPatternsRejectsEscapingGlob()
    {
        using var workspace = TempWorkspace.Create();

        Assert.ThrowsException<ArgumentException>(() => LintScope.FromPatterns(workspace.Root, ["../*.cs"]));
    }

    /// <summary>Rejects rooted scoped glob patterns because they are not repository-relative.</summary>
    [TestMethod]
    public void FromPatternsRejectsRootedGlob()
    {
        using var workspace = TempWorkspace.Create();
        var rootedGlob = Path.Combine(workspace.Root, "*.cs");

        Assert.ThrowsException<ArgumentException>(() => LintScope.FromPatterns(workspace.Root, [rootedGlob]));
    }

    /// <summary>Rejects literal include paths that try to escape the repository.</summary>
    [TestMethod]
    public void FromPatternsRejectsEscapingLiteral()
    {
        using var workspace = TempWorkspace.Create();

        Assert.ThrowsException<ArgumentException>(() => LintScope.FromPatterns(workspace.Root, ["../outside.md"]));
    }

    /// <summary>Matches double-star globs across zero or more directories.</summary>
    [TestMethod]
    public void MatchesGlobSupportsDoubleStarDirectories()
    {
        Assert.IsTrue(GlobPattern.Matches("scripts/Tool/A.cs", "scripts/**/*.cs"));
        Assert.IsTrue(GlobPattern.Matches("scripts/A.cs", "scripts/**/*.cs"));
        Assert.IsFalse(GlobPattern.Matches("tests/Tool/A.cs", "scripts/**/*.cs"));
    }

    /// <summary>Matches double-star globs when the wildcard is not followed by a slash.</summary>
    [TestMethod]
    public void MatchesGlobSupportsDeepWildcardWithoutSlash()
    {
        Assert.IsTrue(GlobPattern.Matches("scripts/Tool/A.cs", "scripts/**"));
    }

    /// <summary>Matches question-mark globs against exactly one path character.</summary>
    [TestMethod]
    public void MatchesGlobSupportsQuestionMark()
    {
        Assert.IsTrue(GlobPattern.Matches("scripts/Tool/A1.cs", "scripts/Tool/A?.cs"));
        Assert.IsFalse(GlobPattern.Matches("scripts/Tool/A10.cs", "scripts/Tool/A?.cs"));
    }

    /// <summary>Normalizes leading current-directory segments before matching paths.</summary>
    [TestMethod]
    public void MatchesGlobNormalizesCurrentDirectoryPrefix()
    {
        Assert.IsTrue(GlobPattern.Matches("./AGENTS.md", "*.md"));
    }
}
