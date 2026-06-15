namespace AlvorKit.Script.Lint.Test;

/// <summary>Tests command display formatting for lint process logs.</summary>
[TestClass]
public sealed class CommandTextTest
{
    /// <summary>Quotes arguments with whitespace while leaving simple arguments alone.</summary>
    [TestMethod]
    public void DisplayQuotesWhitespaceArguments()
    {
        var command = new CommandSpec("tool", ["plain", "two words"], "repo", "tool");

        var display = CommandText.Display(command);

        Assert.AreEqual("tool plain \"two words\"", display);
    }
}
