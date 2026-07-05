namespace AlvorKit.Script.DevSolution.Test;

/// <summary>Tests command-line option parsing for the development solution generator.</summary>
[TestClass]
public sealed class DevSolutionOptionsTest
{
    /// <summary>Uses inferred solutions and derives a generated output beside the consumer solution.</summary>
    [TestMethod]
    public void ParseUsesDefaults()
    {
        var root = Path.Combine(Path.GetTempPath(), "Repos");
        var consumerSolution = Path.Combine(root, "AlvorPong", "AlvorPong.slnx");
        var engineSolution = Path.Combine(root, "AlvorKit", "AlvorKit.slnx");

        var options = DevSolutionOptions.Parse([], () => consumerSolution, () => engineSolution);

        Assert.AreEqual(Path.GetFullPath(consumerSolution), options.ConsumerSolutionPath);
        Assert.AreEqual(Path.GetFullPath(engineSolution), options.EngineSolutionPath);
        Assert.AreEqual(Path.GetFullPath(Path.Combine(root, "AlvorPong", "AlvorPong.Dev.slnx")), options.OutputPath);
        CollectionAssert.AreEqual(new[] { "Engine" }, options.EngineFolderSegments.ToArray());
    }

    /// <summary>Parses explicit paths and a nested engine solution folder.</summary>
    [TestMethod]
    public void ParseUsesExplicitPathsAndEngineFolder()
    {
        var options = DevSolutionOptions.Parse(
            [
                "--consumer-solution",
                "Game.slnx",
                "--engine-solution",
                "../AlvorKit/AlvorKit.slnx",
                "--output",
                "Game.Dev.slnx",
                "--engine-folder",
                "/Engine/Core/"
            ],
            () => "unused-consumer.slnx",
            () => "unused-engine.slnx");

        Assert.AreEqual(Path.GetFullPath("Game.slnx"), options.ConsumerSolutionPath);
        Assert.AreEqual(Path.GetFullPath("../AlvorKit/AlvorKit.slnx"), options.EngineSolutionPath);
        Assert.AreEqual(Path.GetFullPath("Game.Dev.slnx"), options.OutputPath);
        CollectionAssert.AreEqual(new[] { "Engine", "Core" }, options.EngineFolderSegments.ToArray());
    }

    /// <summary>Generated help is handled by the command tree rather than parsed generator options.</summary>
    [TestMethod]
    public void ParseRejectsHelpAsExecutionOptions()
    {
        Assert.ThrowsException<ArgumentException>(() => DevSolutionOptions.Parse(["--help"], () => "Game.slnx", () => "AlvorKit.slnx"));
    }
}
