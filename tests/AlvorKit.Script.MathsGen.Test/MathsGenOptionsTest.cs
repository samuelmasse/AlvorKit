namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Tests command-line parsing for the math source generator.</summary>
[TestClass]
public sealed class MathsGenOptionsTest
{
    /// <summary>No arguments generate into the repository default output directory.</summary>
    [TestMethod]
    public void Parse_DefaultsToRepositoryOutputRoot()
    {
        var options = MathsGenOptions.Parse([], "C:/repo");

        Assert.AreEqual(Path.GetFullPath("C:/repo/out/mathgen"), options.OutputRoot);
    }

    /// <summary>The long output option overrides the generated output directory.</summary>
    [TestMethod]
    public void Parse_OutputRoot_ReturnsFullPath()
    {
        var options = MathsGenOptions.Parse(["--output-root", "out/custom"], "C:/repo");

        Assert.AreEqual(Path.GetFullPath("out/custom"), options.OutputRoot);
    }

    /// <summary>The short output alias shares the same parsing path as the long option.</summary>
    [TestMethod]
    public void Parse_OutputAlias_ReturnsFullPath()
    {
        var options = MathsGenOptions.Parse(["--output", "out/custom"], "C:/repo");

        Assert.AreEqual(Path.GetFullPath("out/custom"), options.OutputRoot);
    }

    /// <summary>Generated help is handled by the command tree rather than parsed generator options.</summary>
    [TestMethod]
    public void Parse_Help_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => MathsGenOptions.Parse(["--help"], "C:/repo"));
    }

    /// <summary>Unknown options fail fast so typos do not generate into the wrong location.</summary>
    [TestMethod]
    public void Parse_UnknownOption_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => MathsGenOptions.Parse(["--wat"], "C:/repo"));
    }

    /// <summary>Output options require a directory value.</summary>
    [TestMethod]
    public void Parse_MissingOutputRoot_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => MathsGenOptions.Parse(["--output-root"], "C:/repo"));
    }
}
