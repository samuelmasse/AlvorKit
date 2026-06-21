namespace AlvorKit.Script.AlvorSense.Test;

/// <summary>Tests command parsing for the persistent AlvorSense session script.</summary>
[TestClass]
public sealed class AlvorSenseCommandLineTest
{
    /// <summary>Start commands parse required values, optional values, timeout, and repeated environment variables.</summary>
    [TestMethod]
    public void Parse_StartCommand_ReturnsValues()
    {
        var command = (AlvorSenseStartCommand)AlvorSenseCommandLine.Parse(
            [
                "start",
                "--project",
                "demos/Game/Game.csproj",
                "--id",
                "game-1",
                "--workdir",
                "C:/repo",
                "--env",
                "A=B",
                "--env",
                "ALVOREYE_DEMO_RESULT_PATH=out/result.json",
                "--timeout",
                "4.5"
            ],
            new StringReader(""));

        Assert.AreEqual("game-1", command.Id);
        Assert.AreEqual("demos/Game/Game.csproj", command.Project);
        Assert.AreEqual("C:/repo", command.WorkingDirectory);
        Assert.AreEqual(TimeSpan.FromSeconds(4.5), command.Timeout);
        Assert.AreEqual("B", command.Environment["A"]);
        Assert.AreEqual("out/result.json", command.Environment["ALVOREYE_DEMO_RESULT_PATH"]);
    }

    /// <summary>Start commands choose a project-derived id and current directory when optional values are omitted.</summary>
    [TestMethod]
    public void Parse_StartCommandWithDefaults_ReturnsDerivedValues()
    {
        var command = (AlvorSenseStartCommand)AlvorSenseCommandLine.Parse(
            ["start", "--project", "demos/Game/Game.csproj"],
            new StringReader(""));

        StringAssert.StartsWith(command.Id, "Game-");
        Assert.AreEqual(Directory.GetCurrentDirectory(), command.WorkingDirectory);
        Assert.AreEqual(TimeSpan.FromSeconds(30), command.Timeout);
        Assert.AreEqual(0, command.Environment.Count);
    }

    /// <summary>Send commands read command text from standard input and ignore blanks and comments.</summary>
    [TestMethod]
    public void Parse_SendCommandFromInput_ReturnsCommands()
    {
        var command = (AlvorSenseSendCommand)AlvorSenseCommandLine.Parse(
            ["send", "--id", "game-1", "--timeout", "2"],
            new StringReader("""
                # move to the key
                mouse position 10 20

                updates 3 0.016
                key Space tap
                """));

        Assert.AreEqual("game-1", command.Id);
        Assert.AreEqual(TimeSpan.FromSeconds(2), command.Timeout);
        CollectionAssert.AreEqual(new[] { "mouse position 10 20", "updates 3 0.016", "key Space tap" }, command.Commands);
    }

    /// <summary>Send commands can receive repeated explicit command-line command values.</summary>
    [TestMethod]
    public void Parse_SendCommandFromCommandOptions_ReturnsCommands()
    {
        var command = (AlvorSenseSendCommand)AlvorSenseCommandLine.Parse(
            ["send", "--id", "game-1", "--command", "render", "--command", "update 0.016"],
            new StringReader(""));

        CollectionAssert.AreEqual(new[] { "render", "update 0.016" }, command.Commands);
    }

    /// <summary>Send commands treat non-option trailing arguments as one interactive command line.</summary>
    [TestMethod]
    public void Parse_SendCommandFromTrailingArguments_ReturnsCommand()
    {
        var command = (AlvorSenseSendCommand)AlvorSenseCommandLine.Parse(
            ["send", "--id", "game-1", "mouse", "Left", "down"],
            new StringReader(""));

        CollectionAssert.AreEqual(new[] { "mouse Left down" }, command.Commands);
    }

    /// <summary>Send command parsing strips BOM prefixes introduced by redirected shell input.</summary>
    [TestMethod]
    public void Parse_SendCommandWithBomPrefixes_ReturnsCleanCommands()
    {
        var command = (AlvorSenseSendCommand)AlvorSenseCommandLine.Parse(
            ["send", "--id", "game-1"],
            new StringReader("\uFEFFrender\n\u00EF\u00BB\u00BFstate\n\u2229\u2557\u2510quit\n"));

        CollectionAssert.AreEqual(new[] { "render", "state", "quit" }, command.Commands);
    }

    /// <summary>Send commands can read command text from a UTF-8 file.</summary>
    [TestMethod]
    public void Parse_SendCommandFromFile_ReturnsCommands()
    {
        var fileCommands = ParseFileCommands("--commands", "render\nscreenshot out/shot.png\n").Commands;

        CollectionAssert.AreEqual(new[] { "render", "screenshot out/shot.png" }, fileCommands.ToArray());
    }

    /// <summary>Send commands accept --file as a clearer alias for command files.</summary>
    [TestMethod]
    public void Parse_SendCommandFromFileAlias_ReturnsCommands()
    {
        var fileAliasCommands = ParseFileCommands("--file", "render\nstate\n").Commands;

        CollectionAssert.AreEqual(new[] { "render", "state" }, fileAliasCommands.ToArray());
    }

    /// <summary>Parses send commands from a temporary command file.</summary>
    private static AlvorSenseSendCommand ParseFileCommands(string option, string contents)
    {
        using var workspace = TempWorkspace.Create();
        var commandsFile = workspace.Write("commands.txt", contents);

        return (AlvorSenseSendCommand)AlvorSenseCommandLine.Parse(
            ["send", "--id", "game-1", option, commandsFile],
            new StringReader("ignored"));
    }

    /// <summary>Stop commands parse the session id and timeout.</summary>
    [TestMethod]
    public void Parse_StopCommand_ReturnsValues()
    {
        var command = (AlvorSenseStopCommand)AlvorSenseCommandLine.Parse(
            ["stop", "--id", "game-1", "--timeout", "1.25"],
            new StringReader(""));

        Assert.AreEqual("game-1", command.Id);
        Assert.AreEqual(TimeSpan.FromSeconds(1.25), command.Timeout);
    }

    /// <summary>Host commands parse the private session directory option.</summary>
    [TestMethod]
    public void Parse_HostCommand_ReturnsValues()
    {
        var command = (AlvorSenseHostCommand)AlvorSenseCommandLine.Parse(
            ["host", "--session-dir", "out/session"],
            new StringReader(""));

        Assert.AreEqual("out/session", command.SessionDir);
    }

    /// <summary>List, status, and help commands parse without requiring a running session.</summary>
    [TestMethod]
    public void Parse_LocalUtilityCommands_ReturnsCommands()
    {
        var list = AlvorSenseCommandLine.Parse(["list"], new StringReader(""));
        var status = (AlvorSenseStatusCommand)AlvorSenseCommandLine.Parse(["status", "--id", "game-1"], new StringReader(""));
        var help = AlvorSenseCommandLine.Parse(["send", "--help"], new StringReader(""));

        Assert.IsInstanceOfType<AlvorSenseListCommand>(list);
        Assert.AreEqual("game-1", status.Id);
        Assert.IsInstanceOfType<AlvorSenseHelpCommand>(help);
    }

    /// <summary>Help command aliases preserve the requested command context for generated help.</summary>
    [TestMethod]
    public void Parse_HelpCommandAlias_ReturnsContextualHelpCommand()
    {
        var help = (AlvorSenseHelpCommand)AlvorSenseCommandLine.Parse(["help", "send"], new StringReader(""));

        CollectionAssert.AreEqual(new[] { "send", "--help" }, help.Args);
    }

    /// <summary>Generated help can be written for a contextual subcommand.</summary>
    [TestMethod]
    public void WriteHelp_ForSendCommand_WritesGeneratedHelp()
    {
        var output = new StringWriter();

        AlvorSenseCommandLine.WriteHelp(["send", "--help"], output);

        var text = output.ToString();
        StringAssert.Contains(text, "send");
        StringAssert.Contains(text, "--command");
        StringAssert.Contains(text, "--file");
    }

    /// <summary>Missing required values are rejected before filesystem or process work starts.</summary>
    [TestMethod]
    public void Parse_MissingRequiredValue_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => AlvorSenseCommandLine.Parse(["start"], new StringReader("")));
        Assert.ThrowsExactly<ArgumentException>(() => AlvorSenseCommandLine.Parse(["send"], new StringReader("")));
        Assert.ThrowsExactly<ArgumentException>(() => AlvorSenseCommandLine.Parse(["stop"], new StringReader("")));
        Assert.ThrowsExactly<ArgumentException>(() => AlvorSenseCommandLine.Parse(["host"], new StringReader("")));
    }

    /// <summary>Environment values must use the NAME=VALUE shape.</summary>
    [TestMethod]
    public void Parse_InvalidEnvironmentValue_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => AlvorSenseCommandLine.Parse(
            ["start", "--project", "game.csproj", "--env", "BROKEN"],
            new StringReader("")));
    }

    /// <summary>Environment variable names cannot be empty.</summary>
    [TestMethod]
    public void Parse_EmptyEnvironmentName_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => AlvorSenseCommandLine.Parse(
            ["start", "--project", "game.csproj", "--env", "=value"],
            new StringReader("")));
    }

    /// <summary>Explicit send command sources must include a value.</summary>
    [TestMethod]
    public void Parse_MissingSendCommandValue_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => AlvorSenseCommandLine.Parse(
            ["send", "--id", "game-1", "--command"],
            new StringReader("")));
    }

    /// <summary>Timeout values must be finite positive seconds.</summary>
    [TestMethod]
    public void Parse_InvalidTimeout_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => AlvorSenseCommandLine.Parse(
            ["send", "--id", "game-1", "--timeout", "0"],
            new StringReader("state")));
        Assert.ThrowsExactly<ArgumentException>(() => AlvorSenseCommandLine.Parse(
            ["stop", "--id", "game-1", "--timeout", "NaN"],
            new StringReader("")));
    }

    /// <summary>Session ids must stay inside the session root directory.</summary>
    [TestMethod]
    public void Parse_UnsafeSessionId_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => AlvorSenseCommandLine.Parse(
            ["status", "--id", ".."],
            new StringReader("")));
        Assert.ThrowsExactly<ArgumentException>(() => AlvorSenseCommandLine.Parse(
            ["send", "--id", "../escape"],
            new StringReader("state")));
    }

    /// <summary>Unknown commands are rejected with usage guidance.</summary>
    [TestMethod]
    public void Parse_UnknownCommand_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => AlvorSenseCommandLine.Parse(["dance"], new StringReader("")));
    }

    /// <summary>Empty command lines are rejected with usage guidance.</summary>
    [TestMethod]
    public void Parse_NoArguments_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => AlvorSenseCommandLine.Parse([], new StringReader("")));
    }

    /// <summary>Start commands can be converted into persisted session manifests.</summary>
    [TestMethod]
    public void ToManifest_ReturnsPersistableValues()
    {
        var command = new AlvorSenseStartCommand(
            "game-1",
            "game.csproj",
            "C:/repo",
            new Dictionary<string, string> { ["A"] = "B" },
            TimeSpan.FromSeconds(5));

        var manifest = command.ToManifest();

        Assert.AreEqual("game-1", manifest.Id);
        Assert.AreEqual("game.csproj", manifest.Project);
        Assert.AreEqual("C:/repo", manifest.WorkingDirectory);
        Assert.AreEqual("B", manifest.Environment["A"]);
        Assert.AreNotSame(command.Environment, manifest.Environment);
    }
}
