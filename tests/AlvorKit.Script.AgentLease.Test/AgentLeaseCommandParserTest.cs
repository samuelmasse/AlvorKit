using AlvorKit.Script.Workspace;

namespace AlvorKit.Script.AgentLease.Test;

/// <summary>Tests command-line parsing for the advisory lease helper.</summary>
[TestClass]
public sealed class AgentLeaseCommandParserTest
{
    /// <summary>Start commands parse task, paths, mode, notes, and timeout options.</summary>
    [TestMethod]
    public void Parse_StartCommand_ReturnsValues()
    {
        var command = AgentLeaseCommandParser.Parse(
            [
                "start",
                "--agent",
                "codex-a",
                "--task",
                "Edit tests",
                "--mode",
                "format",
                "--path",
                "src/Foo.cs",
                "--paths",
                "tests/**",
                "--notes",
                "Avoid broad format",
                "--timeout-minutes",
                "10"
            ],
            "C:/repo");

        Assert.AreEqual(AgentLeaseCommandKind.Start, command.Kind);
        Assert.AreEqual("codex-a", command.Agent);
        Assert.AreEqual("Edit tests", command.TaskDescription);
        Assert.AreEqual("format", command.Mode);
        CollectionAssert.AreEqual(new[] { "src/Foo.cs", "tests/**" }, command.Paths.ToArray());
        Assert.AreEqual("Avoid broad format", command.Notes);
        Assert.AreEqual(TimeSpan.FromMinutes(10), command.Timeout);
    }

    /// <summary>Help parsing does not require repository discovery.</summary>
    [TestMethod]
    public void Parse_HelpCommand_ReturnsHelp()
    {
        var command = AgentLeaseCommandParser.Parse(["help"], "C:/nowhere");

        Assert.AreEqual(AgentLeaseCommandKind.Help, command.Kind);
    }

    /// <summary>Public parsing can return help without repository discovery.</summary>
    [TestMethod]
    public void Parse_PublicNoArgs_ReturnsHelp()
    {
        var command = AgentLeaseCommandParser.Parse([]);

        Assert.AreEqual(AgentLeaseCommandKind.Help, command.Kind);
    }

    /// <summary>Public parsing recognizes help aliases before repository discovery.</summary>
    [TestMethod]
    public void Parse_PublicHelpAliases_ReturnHelp()
    {
        Assert.AreEqual(AgentLeaseCommandKind.Help, AgentLeaseCommandParser.Parse(["help"]).Kind);
        Assert.AreEqual(AgentLeaseCommandKind.Help, AgentLeaseCommandParser.Parse(["-h"]).Kind);
        Assert.AreEqual(AgentLeaseCommandKind.Help, AgentLeaseCommandParser.Parse(["--help"]).Kind);
    }

    /// <summary>Public parsing honors an explicit repository root before default discovery.</summary>
    [TestMethod]
    public void Parse_PublicWithRepoRoot_ReturnsRoot()
    {
        using var workspace = TempWorkspace.Create();

        var command = AgentLeaseCommandParser.Parse(["list", "--repo-root", workspace.Root]);

        Assert.AreEqual(Path.GetFullPath(workspace.Root), command.RepoRoot);
    }

    /// <summary>Public parsing falls back to repository-root discovery when no root argument is supplied.</summary>
    [TestMethod]
    public void Parse_PublicWithoutRepoRoot_UsesDiscoveredRoot()
    {
        var command = AgentLeaseCommandParser.Parse(["list"]);

        Assert.IsTrue(File.Exists(Path.Combine(command.RepoRoot, ProjectRoot.SolutionFileName)));
    }

    /// <summary>Command names map to the expected command kinds.</summary>
    [TestMethod]
    public void Parse_CommandKinds_ReturnExpectedValues()
    {
        Assert.AreEqual(AgentLeaseCommandKind.Touch, AgentLeaseCommandParser.Parse(["touch"], "C:/repo").Kind);
        Assert.AreEqual(AgentLeaseCommandKind.List, AgentLeaseCommandParser.Parse(["list"], "C:/repo").Kind);
        Assert.AreEqual(AgentLeaseCommandKind.Check, AgentLeaseCommandParser.Parse(["check", "--path", "src/**"], "C:/repo").Kind);
        Assert.AreEqual(AgentLeaseCommandKind.Done, AgentLeaseCommandParser.Parse(["done"], "C:/repo").Kind);
        Assert.AreEqual(
            AgentLeaseCommandKind.Conflict,
            AgentLeaseCommandParser.Parse(["conflict", "--path", "src/**", "--reason", "Need it"], "C:/repo").Kind);
    }

    /// <summary>List commands can request stale leases.</summary>
    [TestMethod]
    public void Parse_ListIncludeStale_ReturnsTrue()
    {
        var command = AgentLeaseCommandParser.Parse(["list", "--include-stale"], "C:/repo");

        Assert.IsTrue(command.IncludeStale);
    }

    /// <summary>Start commands require a task summary.</summary>
    [TestMethod]
    public void Parse_StartWithoutTask_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => AgentLeaseCommandParser.Parse(["start", "--path", "src/**"], "C:/repo"));
    }

    /// <summary>Path-oriented commands require at least one path claim.</summary>
    [TestMethod]
    public void Parse_CheckWithoutPath_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => AgentLeaseCommandParser.Parse(["check"], "C:/repo"));
    }

    /// <summary>Unknown modes are rejected before a lease can be written.</summary>
    [TestMethod]
    public void Parse_UnknownMode_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(
            () => AgentLeaseCommandParser.Parse(["start", "--task", "T", "--path", "src/**", "--mode", "paint"], "C:/repo"));
    }

    /// <summary>Conflict notes require a reason so they stay useful later.</summary>
    [TestMethod]
    public void Parse_ConflictWithoutReason_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => AgentLeaseCommandParser.Parse(["conflict", "--path", "src/**"], "C:/repo"));
    }

    /// <summary>Blank path claims are rejected for commands that can update lease paths.</summary>
    [TestMethod]
    public void Parse_TouchWithBlankPath_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => AgentLeaseCommandParser.Parse(["touch", "--path", " "], "C:/repo"));
    }

    /// <summary>Unknown commands are rejected.</summary>
    [TestMethod]
    public void Parse_UnknownCommand_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => AgentLeaseCommandParser.Parse(["paint"], "C:/repo"));
    }

    /// <summary>Unknown options are rejected.</summary>
    [TestMethod]
    public void Parse_UnknownOption_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => AgentLeaseCommandParser.Parse(["list", "--json"], "C:/repo"));
    }

    /// <summary>Missing option values produce a targeted error.</summary>
    [TestMethod]
    public void Parse_MissingValue_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => AgentLeaseCommandParser.Parse(["start", "--task"], "C:/repo"));
    }

    /// <summary>Inline help options after a command point callers to the help command.</summary>
    [TestMethod]
    public void Parse_CommandHelpOption_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => AgentLeaseCommandParser.Parse(["list", "--help"], "C:/repo"));
        Assert.ThrowsExactly<ArgumentException>(() => AgentLeaseCommandParser.Parse(["list", "-h"], "C:/repo"));
    }
}
