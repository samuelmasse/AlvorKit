namespace AlvorKit.Script.BindgenReview.Test;

/// <summary>Tests command-line parsing for the bindgen review helper.</summary>
[TestClass]
public sealed class BindgenReviewCommandParserTest
{
    /// <summary>Start commands parse the selected library, case name, and repository root.</summary>
    [TestMethod]
    public void Parse_StartCommand_ReturnsValues()
    {
        var command = BindgenReviewCommandParser.Parse(["start", "xxhash", "--case", "overloads", "--repo-root", "C:/repo"], "C:/fallback");

        Assert.AreEqual(BindgenReviewCommandKind.Start, command.Kind);
        Assert.AreEqual("xxhash", command.Library);
        Assert.AreEqual("overloads", command.CaseName);
        Assert.AreEqual(Path.GetFullPath("C:/repo"), command.RepoRoot);
    }

    /// <summary>Finish commands parse the review root and keep flag.</summary>
    [TestMethod]
    public void Parse_FinishCommand_ReturnsValues()
    {
        var command = BindgenReviewCommandParser.Parse(["finish", "out/bindgen-review/xxhash-a1b2c", "--keep"], "C:/repo");

        Assert.AreEqual(BindgenReviewCommandKind.Finish, command.Kind);
        Assert.AreEqual("out/bindgen-review/xxhash-a1b2c", command.ReviewRoot);
        Assert.IsTrue(command.Keep);
    }

    /// <summary>Help parsing does not require repository discovery.</summary>
    [TestMethod]
    public void Parse_HelpCommand_ReturnsHelp()
    {
        var command = BindgenReviewCommandParser.Parse(["help"], "C:/nowhere");

        Assert.AreEqual(BindgenReviewCommandKind.Help, command.Kind);
    }

    /// <summary>Public help parsing returns help without repository discovery.</summary>
    [TestMethod]
    public void Parse_PublicHelp_ReturnsHelp()
    {
        Assert.AreEqual(BindgenReviewCommandKind.Help, BindgenReviewCommandParser.Parse([]).Kind);
        Assert.AreEqual(BindgenReviewCommandKind.Help, BindgenReviewCommandParser.Parse(["-h"]).Kind);
    }

    /// <summary>Public parsing honors an explicit repository root before default discovery.</summary>
    [TestMethod]
    public void Parse_PublicWithRepoRoot_ReturnsRoot()
    {
        using var workspace = TempWorkspace.Create();

        var command = BindgenReviewCommandParser.Parse(["start", "xxhash", "--repo-root", workspace.Root]);

        Assert.AreEqual(Path.GetFullPath(workspace.Root), command.RepoRoot);
    }

    /// <summary>Public parsing falls back to repository-root discovery when no root argument is supplied.</summary>
    [TestMethod]
    public void Parse_PublicWithoutRepoRoot_UsesDiscoveredRoot()
    {
        var command = BindgenReviewCommandParser.Parse(["start", "xxhash"]);

        Assert.IsTrue(File.Exists(Path.Combine(command.RepoRoot, "AlvorKit.slnx")));
    }

    /// <summary>Command names map to the expected command kinds.</summary>
    [TestMethod]
    public void Parse_CommandKinds_ReturnExpectedValues()
    {
        Assert.AreEqual(BindgenReviewCommandKind.After, BindgenReviewCommandParser.Parse(["after", "out/bindgen-review/x"], "C:/repo").Kind);
        Assert.AreEqual(BindgenReviewCommandKind.Diff, BindgenReviewCommandParser.Parse(["diff", "out/bindgen-review/x"], "C:/repo").Kind);
        Assert.AreEqual(BindgenReviewCommandKind.Clean, BindgenReviewCommandParser.Parse(["clean", "out/bindgen-review/x"], "C:/repo").Kind);
    }

    /// <summary>Non-start commands reject case names because the manifest owns that value.</summary>
    [TestMethod]
    public void Parse_CaseOnDiff_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => BindgenReviewCommandParser.Parse(["diff", "out/bindgen-review/x", "--case", "x"], "C:/repo"));
    }

    /// <summary>Only finish accepts keep because other commands do not delete review directories.</summary>
    [TestMethod]
    public void Parse_KeepOnClean_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => BindgenReviewCommandParser.Parse(["clean", "out/bindgen-review/x", "--keep"], "C:/repo"));
    }

    /// <summary>Commands require exactly one positional argument.</summary>
    [TestMethod]
    public void Parse_MissingPositional_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => BindgenReviewCommandParser.Parse(["start"], "C:/repo"));
    }

    /// <summary>Blank positional arguments are rejected before they can become paths.</summary>
    [TestMethod]
    public void Parse_BlankPositional_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => BindgenReviewCommandParser.Parse(["clean", " "], "C:/repo"));
    }

    /// <summary>Unexpected extra positional arguments are rejected.</summary>
    [TestMethod]
    public void Parse_ExtraPositional_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => BindgenReviewCommandParser.Parse(["start", "xxhash", "extra"], "C:/repo"));
    }

    /// <summary>Unknown options are rejected so typos do not silently skip cleanup.</summary>
    [TestMethod]
    public void Parse_UnknownOption_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => BindgenReviewCommandParser.Parse(["start", "xxhash", "--wat"], "C:/repo"));
    }

    /// <summary>Missing option values produce a targeted error.</summary>
    [TestMethod]
    public void Parse_MissingOptionValue_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => BindgenReviewCommandParser.Parse(["start", "xxhash", "--case"], "C:/repo"));
    }

    /// <summary>Inline help options after a command point callers to the help command.</summary>
    [TestMethod]
    public void Parse_CommandHelpOption_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => BindgenReviewCommandParser.Parse(["start", "xxhash", "--help"], "C:/repo"));
    }

    /// <summary>Unknown command names are rejected.</summary>
    [TestMethod]
    public void Parse_UnknownCommand_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => BindgenReviewCommandParser.Parse(["wat", "xxhash"], "C:/repo"));
    }
}
