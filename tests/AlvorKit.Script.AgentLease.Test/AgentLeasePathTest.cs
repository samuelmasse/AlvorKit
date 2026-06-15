namespace AlvorKit.Script.AgentLease.Test;

/// <summary>Tests advisory path normalization and overlap heuristics.</summary>
[TestClass]
public sealed class AgentLeasePathTest
{
    /// <summary>Normalization produces repository-relative slash-separated claims.</summary>
    [TestMethod]
    public void Normalize_RemovesLocalPrefixAndBackslashes()
    {
        Assert.AreEqual("scripts/Tool/File.cs", AgentLeasePath.Normalize(@".\scripts\Tool\File.cs"));
    }

    /// <summary>Global claims overlap every path.</summary>
    [TestMethod]
    public void MayOverlap_GlobalClaim_ReturnsTrue()
    {
        Assert.IsTrue(AgentLeasePath.MayOverlap("repo-wide", "src/Foo.cs"));
        Assert.IsTrue(AgentLeasePath.MayOverlap("*", "tests/**"));
    }

    /// <summary>Directory globs overlap paths beneath the same directory.</summary>
    [TestMethod]
    public void MayOverlap_DirectoryGlobAndNestedPath_ReturnsTrue()
    {
        Assert.IsTrue(AgentLeasePath.MayOverlap("scripts/**", "scripts/AlvorKit.Script.AgentLease/Program.cs"));
    }

    /// <summary>Exact path claims overlap themselves.</summary>
    [TestMethod]
    public void MayOverlap_SamePath_ReturnsTrue()
    {
        Assert.IsTrue(AgentLeasePath.MayOverlap("src/Foo.cs", "src/Foo.cs"));
    }

    /// <summary>Directory claims with trailing slash overlap nested paths.</summary>
    [TestMethod]
    public void MayOverlap_TrailingSlashDirectory_ReturnsTrue()
    {
        Assert.IsTrue(AgentLeasePath.MayOverlap("src/", "src/Foo.cs"));
    }

    /// <summary>Disjoint directory claims do not overlap.</summary>
    [TestMethod]
    public void MayOverlap_DisjointDirectories_ReturnsFalse()
    {
        Assert.IsFalse(AgentLeasePath.MayOverlap("src/OpenGL/**", "tests/AlvorKit.Script.AgentLease.Test/**"));
    }

    /// <summary>Single-segment glob claims match files in their directory.</summary>
    [TestMethod]
    public void MayOverlap_FileGlobAndFile_ReturnsTrue()
    {
        Assert.IsTrue(AgentLeasePath.MayOverlap("AGENTS.md", "*.md"));
    }

    /// <summary>Different file globs in the same directory conservatively overlap by shared prefix.</summary>
    [TestMethod]
    public void MayOverlap_SiblingFileGlobs_ReturnsTrue()
    {
        Assert.IsTrue(AgentLeasePath.MayOverlap("src/Foo*.cs", "src/Bar*.cs"));
    }

    /// <summary>Question-mark glob tokens match exactly one character.</summary>
    [TestMethod]
    public void MayOverlap_QuestionMarkGlob_ReturnsTrue()
    {
        Assert.IsTrue(AgentLeasePath.MayOverlap("src/Foo1.cs", "src/Foo?.cs"));
    }

    /// <summary>Double-star directory globs match nested paths.</summary>
    [TestMethod]
    public void MayOverlap_DoubleStarDirectoryGlob_ReturnsTrue()
    {
        Assert.IsTrue(AgentLeasePath.MayOverlap("src/Deep/Foo.cs", "src/**/Foo.cs"));
    }
}
