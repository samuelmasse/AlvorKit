namespace AlvorKit.Script.Bindgen.Core.Test;

/// <summary>Covers bindgen command-line option parsing.</summary>
[TestClass]
public sealed class BindgenOptionsTest
{
    /// <summary>No arguments generate all bindings without strict mode or an alternate output root.</summary>
    [TestMethod]
    public void Parse_DefaultsToAllLibraries()
    {
        var options = BindgenOptions.Parse([]);

        Assert.AreEqual("all", options.Selection);
        Assert.IsFalse(options.Strict);
        Assert.IsNull(options.OutputRoot);
    }

    /// <summary>Selection, strict mode, and generated-output root may be supplied together.</summary>
    [TestMethod]
    public void Parse_ReadsSelectionStrictModeAndOutputRoot()
    {
        var options = BindgenOptions.Parse(["xxhash", "--strict", "--output-root", "out/bindgen-review/after"]);

        Assert.AreEqual("xxhash", options.Selection);
        Assert.IsTrue(options.Strict);
        Assert.AreEqual("out/bindgen-review/after", options.OutputRoot);
    }

    /// <summary>The short output alias is accepted for local review commands.</summary>
    [TestMethod]
    public void Parse_ReadsOutputRootAlias()
    {
        var options = BindgenOptions.Parse(["--out", "out/bindgen-review/before"]);

        Assert.AreEqual("out/bindgen-review/before", options.OutputRoot);
    }

    /// <summary>Unknown options fail fast instead of being treated as library selections.</summary>
    [TestMethod]
    public void Parse_RejectsUnknownOption()
    {
        var exception = Assert.ThrowsException<ArgumentException>(() => BindgenOptions.Parse(["--mystery"]));

        StringAssert.Contains(exception.Message, "Unrecognized command or argument '--mystery'");
    }

    /// <summary>Only one positional library selection is accepted.</summary>
    [TestMethod]
    public void Parse_RejectsExtraSelection()
    {
        var exception = Assert.ThrowsException<ArgumentException>(() => BindgenOptions.Parse(["xxhash", "opengl"]));

        StringAssert.Contains(exception.Message, "Unrecognized command or argument 'opengl'");
    }

    /// <summary>Options that require values report a clear parser failure when the value is absent.</summary>
    [TestMethod]
    public void Parse_RejectsMissingOutputRoot()
    {
        var exception = Assert.ThrowsException<ArgumentException>(() => BindgenOptions.Parse(["--output-root"]));

        StringAssert.Contains(exception.Message, "Required argument missing for option: '--output-root'");
    }
}
