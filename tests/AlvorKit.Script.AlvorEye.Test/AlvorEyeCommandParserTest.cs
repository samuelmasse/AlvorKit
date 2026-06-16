namespace AlvorKit.Script.AlvorEye.Test;

/// <summary>Tests command-line parsing for AlvorEye.</summary>
[TestClass]
public sealed class AlvorEyeCommandParserTest
{
    /// <summary>Parses the run command with an explicit scenario and repository root.</summary>
    [TestMethod]
    public void Parse_Run_ReturnsScenarioAndRepoRoot()
    {
        var command = AlvorEyeCommandParser.Parse(["run", "--scenario", "demo.json", "--repo-root", "C:/repo"], "C:/fallback");

        Assert.AreEqual(AlvorEyeCommandKind.Run, command.Kind);
        Assert.AreEqual(Path.GetFullPath("C:/repo"), command.RepoRoot);
        Assert.AreEqual(Path.GetFullPath("demo.json"), command.ScenarioPath);
    }

    /// <summary>Defaults to help when no command or a help flag is supplied.</summary>
    [TestMethod]
    public void Parse_HelpForms_ReturnHelp()
    {
        Assert.AreEqual(AlvorEyeCommandKind.Help, AlvorEyeCommandParser.Parse([], "C:/repo").Kind);
        Assert.AreEqual(AlvorEyeCommandKind.Help, AlvorEyeCommandParser.Parse(["--help"], "C:/repo").Kind);
        Assert.AreEqual(AlvorEyeCommandKind.Help, AlvorEyeCommandParser.Parse(["-h"], "C:/repo").Kind);
    }

    /// <summary>The public parser honors explicit repository roots before project-root discovery.</summary>
    [TestMethod]
    public void Parse_PublicParser_UsesRepoRootArgument()
    {
        var command = AlvorEyeCommandParser.Parse(["run", "--repo-root", "C:/repo", "--scenario", "demo.json"]);

        Assert.AreEqual(Path.GetFullPath("C:/repo"), command.RepoRoot);
        Assert.AreEqual(AlvorEyeCommandKind.Run, command.Kind);
    }

    /// <summary>Parses every supported command kind.</summary>
    [TestMethod]
    public void Parse_CommandKinds_ReturnExpectedValues()
    {
        Assert.AreEqual(AlvorEyeCommandKind.Help, AlvorEyeCommandParser.Parse(["help"], "C:/repo").Kind);
        Assert.AreEqual(AlvorEyeCommandKind.Session, AlvorEyeCommandParser.Parse(["session", "--scenario", "a.json"], "C:/repo").Kind);
        Assert.AreEqual(AlvorEyeCommandKind.Handoff, AlvorEyeCommandParser.Parse(["handoff", "--session", "s1"], "C:/repo").Kind);
        Assert.AreEqual(AlvorEyeCommandKind.Resume, AlvorEyeCommandParser.Parse(["resume", "--session", "s1"], "C:/repo").Kind);
    }

    /// <summary>Rejects unknown commands and options.</summary>
    [TestMethod]
    public void Parse_UnknownValues_Throw()
    {
        Assert.ThrowsExactly<ArgumentException>(() => AlvorEyeCommandParser.Parse(["wat"], "C:/repo"));
        Assert.ThrowsExactly<ArgumentException>(() => AlvorEyeCommandParser.Parse(["run", "--scenario", "a.json", "--wat"], "C:/repo"));
    }

    /// <summary>Rejects commands missing their required identifier.</summary>
    [TestMethod]
    public void Parse_MissingRequiredOptions_Throw()
    {
        Assert.ThrowsExactly<ArgumentException>(() => AlvorEyeCommandParser.Parse(["run"], "C:/repo"));
        Assert.ThrowsExactly<ArgumentException>(() => AlvorEyeCommandParser.Parse(["session"], "C:/repo"));
        Assert.ThrowsExactly<ArgumentException>(() => AlvorEyeCommandParser.Parse(["handoff"], "C:/repo"));
        Assert.ThrowsExactly<ArgumentException>(() => AlvorEyeCommandParser.Parse(["resume"], "C:/repo"));
        Assert.ThrowsExactly<ArgumentException>(() => AlvorEyeCommandParser.Parse(["run", "--scenario"], "C:/repo"));
    }
}
