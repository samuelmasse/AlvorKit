namespace AlvorKit;

/// <summary>Tests for native build command-line parsing.</summary>
[TestClass]
public sealed class CliParserTest
{
    /// <summary>Parse-only requests require a concrete command.</summary>
    [TestMethod]
    public void Parse_NoArgs_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => CliParser.Parse([]));
    }

    /// <summary>Parse-only helpers reject generated help instead of writing it to test output.</summary>
    [TestMethod]
    public void Parse_Help_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => CliParser.Parse(["--help"]));
    }

    /// <summary>Build accepts --rid value syntax.</summary>
    [TestMethod]
    public void Parse_BuildWithRidValue_ReturnsBuildRequest()
    {
        var request = CliParser.Parse(["build", "fastnoise2", "--rid", "linux-x64"]);

        Assert.AreEqual(CliCommand.Build, request.Command);
        Assert.AreEqual("fastnoise2", request.Selection);
        Assert.AreEqual("linux-x64", request.Rid);
    }

    /// <summary>Build accepts --rid=value syntax.</summary>
    [TestMethod]
    public void Parse_BuildWithRidEquals_ReturnsBuildRequest()
    {
        var request = CliParser.Parse(["build", "all", "--rid=osx-arm64"]);

        Assert.AreEqual("all", request.Selection);
        Assert.AreEqual("osx-arm64", request.Rid);
    }

    /// <summary>Verify requires one library and a target runtime identifier.</summary>
    [TestMethod]
    public void Parse_VerifyWithRid_ReturnsVerifyRequest()
    {
        var request = CliParser.Parse(["verify", "fastnoise2", "--rid", "linux-arm"]);

        Assert.AreEqual(CliCommand.Verify, request.Command);
        Assert.AreEqual("fastnoise2", request.Selection);
        Assert.AreEqual("linux-arm", request.Rid);
    }

    /// <summary>Commands with missing positional arguments produce useful errors.</summary>
    [TestMethod]
    public void Parse_VersionWithoutLibrary_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => CliParser.Parse(["version"]));
    }

    /// <summary>List and version commands parse without target runtime options.</summary>
    [TestMethod]
    public void Parse_ListAndVersion_ReturnRequests()
    {
        Assert.AreEqual(CliCommand.List, CliParser.Parse(["list"]).Command);

        var version = CliParser.Parse(["version", "fastnoise2"]);

        Assert.AreEqual(CliCommand.Version, version.Command);
        Assert.AreEqual("fastnoise2", version.Selection);
    }

    /// <summary>Invalid commands and missing option values produce argument errors.</summary>
    [TestMethod]
    public void Parse_InvalidInput_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => CliParser.Parse(["build"]));
        Assert.ThrowsExactly<ArgumentException>(() => CliParser.Parse(["build", "fastnoise2", "--rid"]));
        Assert.ThrowsExactly<ArgumentException>(() => CliParser.Parse(["verify", "fastnoise2"]));
        Assert.ThrowsExactly<ArgumentException>(() => CliParser.Parse(["publish"]));
    }
}
