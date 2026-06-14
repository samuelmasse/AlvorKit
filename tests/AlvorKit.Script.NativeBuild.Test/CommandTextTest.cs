using AlvorKit.Script.NativeBuild;

namespace AlvorKit.Script.NativeBuild.Test;

/// <summary>Tests for command and script text helpers.</summary>
[TestClass]
public sealed class CommandTextTest
{
    /// <summary>PowerShell quoting escapes embedded apostrophes.</summary>
    [TestMethod]
    public void PowerShellQuote_EscapesApostrophe()
    {
        Assert.AreEqual("'it''s fine'", CommandText.PowerShellQuote("it's fine"));
    }

    /// <summary>Display adds quotes only when log arguments contain whitespace.</summary>
    [TestMethod]
    public void Display_QuotesWhitespaceArguments()
    {
        var display = CommandText.Display(new("tool", ["plain", "two words"]));

        Assert.AreEqual("tool plain \"two words\"", display);
    }
}
