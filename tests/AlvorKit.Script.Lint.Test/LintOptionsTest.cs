namespace AlvorKit.Script.Lint.Test;

/// <summary>Tests command-line parsing for the lint coordinator.</summary>
[TestClass]
public sealed class LintOptionsTest
{
    /// <summary>Parses the explicit repository root and fix mode flags.</summary>
    [TestMethod]
    public void ParseUsesExplicitRepoRootAndFixMode()
    {
        var repoRoot = Path.GetTempPath();

        var options = LintOptions.Parse(["--repo-root", repoRoot, "--fix"]);

        Assert.AreEqual(Path.GetFullPath(repoRoot), options.RepoRoot);
        Assert.IsTrue(options.Fix);
        Assert.IsFalse(options.ShowHelp);
    }

    /// <summary>Parses the help flag without enabling fix mode.</summary>
    [TestMethod]
    public void ParseUsesHelpMode()
    {
        var options = LintOptions.Parse(["--help"]);

        Assert.IsTrue(options.ShowHelp);
        Assert.IsFalse(options.Fix);
    }

    /// <summary>Rejects a repository root flag without a value.</summary>
    [TestMethod]
    public void ParseRejectsRepoRootWithoutPath()
    {
        Assert.ThrowsException<ArgumentException>(() => LintOptions.Parse(["--repo-root"]));
    }

    /// <summary>Rejects unknown options so typos do not silently skip checks.</summary>
    [TestMethod]
    public void ParseRejectsUnknownOption()
    {
        Assert.ThrowsException<ArgumentException>(() => LintOptions.Parse(["--wat"]));
    }
}
