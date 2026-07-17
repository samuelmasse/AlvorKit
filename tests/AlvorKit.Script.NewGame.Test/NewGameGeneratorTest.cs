namespace AlvorKit.Script.NewGame.Test;

/// <summary>Tests the new-game repository scaffolder.</summary>
[TestClass]
public class NewGameGeneratorTest
{
    /// <summary>Generates the expected starter repository files from a friendly game name.</summary>
    [TestMethod]
    public void GenerateCreatesStarterRepository()
    {
        using var workspace = TempWorkspace.Create("AlvorKit.Script.NewGame");
        var output = workspace.CreateDirectory("HelloAlvor");
        var options = NewGameOptions.Parse(["hello-alvor", "--output", output], AlvorKitRoot);

        var result = new NewGameGenerator().Generate(options);

        Assert.AreEqual(output, result.OutputPath);
        Assert.AreEqual(21, result.FileCount);
        AssertFile(output, "AGENTS.md");
        StringAssert.Contains(Read(output, "AGENTS.md"), "../AlvorKit/docs/GameRepositoryInstructions.md");
        AssertFile(output, "HelloAlvor.slnx");
        AssertFile(output, "src/Directory.Build.props");
        AssertFile(output, "src/HelloAlvor/Program.cs");
        AssertFile(output, "src/HelloAlvor.App/HelloAlvor.App.csproj");
        AssertFile(output, "src/HelloAlvor.App/AppScope.cs");
        AssertFile(output, "src/HelloAlvor.App/AppCounter.cs");
        AssertFile(output, "src/HelloAlvor.App.Frontend/HelloAlvor.App.Frontend.csproj");
        AssertFile(output, "src/HelloAlvor.App.Frontend/AppGlTriangle.cs");
        AssertFile(output, "src/HelloAlvor.App.Frontend/AppSpriteScene.cs");
        AssertFile(output, "src/HelloAlvor.Menus/HelloAlvor.Menus.csproj");
        AssertFile(output, "src/HelloAlvor.Menus/AppStyle.cs");
        AssertFile(output, "src/HelloAlvor.Menus/AppMainMenu.cs");
        Assert.IsFalse(File.Exists(Path(output, "AlvorStarter.slnx.template")));
        StringAssert.Contains(Read(output, "src/HelloAlvor.Menus/AppMainMenu.cs"), "Hello Alvor");
    }

    /// <summary>Leaves no unresolved placeholders and includes the requested starter features.</summary>
    [TestMethod]
    public void GenerateReplacesPlaceholdersAndIncludesRenderingExamples()
    {
        using var workspace = TempWorkspace.Create("AlvorKit.Script.NewGame");
        var output = workspace.CreateDirectory("SampleGame");
        var options = NewGameOptions.Parse(["SampleGame", "--output", output], AlvorKitRoot);

        new NewGameGenerator().Generate(options);

        var files = Directory.GetFiles(output, "*", SearchOption.AllDirectories);
        Assert.IsFalse(files.Select(File.ReadAllText).Any(text => text.Contains("{{", StringComparison.Ordinal)));
        Assert.IsFalse(files.Select(File.ReadAllText).Any(text => text.Contains("AlvorStarter", StringComparison.Ordinal)));
        var solution = Read(output, "SampleGame.slnx");
        StringAssert.Contains(solution, "<Project Path=\"src/SampleGame/SampleGame.csproj\" DefaultStartup=\"true\" />");
        StringAssert.Contains(solution, "<Project Path=\"src/SampleGame.App/SampleGame.App.csproj\" />");
        StringAssert.Contains(solution, "<Project Path=\"src/SampleGame.App.Frontend/SampleGame.App.Frontend.csproj\" />");
        StringAssert.Contains(solution, "<Project Path=\"src/SampleGame.Menus/SampleGame.Menus.csproj\" />");
        StringAssert.Contains(Read(output, "src/SampleGame.App.Frontend/AppGlTriangle.cs"), "gl.DrawArrays");
        StringAssert.Contains(Read(output, "src/SampleGame.App.Frontend/AppSpriteScene.cs"), "sprites.Batch.Draw");
        var menu = Read(output, "src/SampleGame.Menus/AppMainMenu.cs");
        StringAssert.Contains(menu, "OnClickF(counter.Increment)");
        Assert.IsFalse(menu.Contains("ActiveButton", StringComparison.Ordinal));
    }

    /// <summary>Keeps generated project references aligned with the pure, frontend, and menu package split.</summary>
    [TestMethod]
    public void GenerateKeepsProjectSplitDependenciesClean()
    {
        using var workspace = TempWorkspace.Create("AlvorKit.Script.NewGame");
        var output = workspace.CreateDirectory("SplitGame");
        var options = NewGameOptions.Parse(["SplitGame", "--output", output], AlvorKitRoot);

        new NewGameGenerator().Generate(options);

        var app = Read(output, "src/SplitGame.App/SplitGame.App.csproj");
        AssertDoesNotContain(app, "AlvorKit.Engine.Loop");
        AssertDoesNotContain(app, "AlvorKit.UI");
        AssertDoesNotContain(app, "AlvorKit.OpenGL");
        AssertDoesNotContain(app, "AlvorKit.Windowing");
        AssertDoesNotContain(app, "SplitGame.App.Frontend");
        AssertDoesNotContain(app, "SplitGame.Menus");

        var frontend = Read(output, "src/SplitGame.App.Frontend/SplitGame.App.Frontend.csproj");
        StringAssert.Contains(frontend, "AlvorKit.Engine.csproj");
        AssertDoesNotContain(frontend, "AlvorKit.Engine.Loop");
        AssertDoesNotContain(frontend, "SplitGame.Menus");
    }

    /// <summary>Emits the documented sibling AlvorKit path and an actionable missing-clone hint.</summary>
    [TestMethod]
    public void GenerateEmitsSiblingAlvorKitRootAndCloneHint()
    {
        using var workspace = TempWorkspace.Create("AlvorKit.Script.NewGame");
        var output = workspace.CreateDirectory("SampleInvaders");
        var options = NewGameOptions.Parse(["SampleInvaders", "--output", output], AlvorKitRoot);

        new NewGameGenerator().Generate(options);

        var props = Read(output, "src/Directory.Build.props");
        StringAssert.Contains(
            props,
            "<AlvorKitRoot>$([System.IO.Path]::GetFullPath('$(SampleInvadersRepositoryRoot)..\\AlvorKit\\'))</AlvorKitRoot>");
        StringAssert.Contains(
            props,
            "Text=\"AlvorKit sibling clone not found at '$(AlvorKitRoot)'. Clone AlvorKit next to the SampleInvaders repository.\"");
    }

    /// <summary>Keeps the starter source as a concrete project rather than a set of template fragments.</summary>
    [TestMethod]
    public void StarterSourceIsConcreteProject()
    {
        var root = Path(AlvorKitRoot(), "res/templates/new-game/source");

        Assert.IsFalse(File.Exists(Path(root, "AlvorStarter.slnx")));
        AssertFile(root, "AlvorStarter.slnx.template");
        AssertFile(root, "src/AlvorStarter/Program.cs");
        AssertFile(root, "src/AlvorStarter.App/AppScope.cs");
        AssertFile(root, "src/AlvorStarter.App/AppCounter.cs");
        AssertFile(root, "src/AlvorStarter.App.Frontend/AppGlTriangle.cs");
        AssertFile(root, "src/AlvorStarter.App.Frontend/AppSpriteScene.cs");
        AssertFile(root, "src/AlvorStarter.Menus/AppStarterState.cs");
        AssertFile(root, "src/AlvorStarter.Menus/AppStyle.cs");
        AssertFile(root, "src/AlvorStarter.Menus/AppMainMenu.cs");
        StringAssert.Contains(Read(root, "src/AlvorStarter.Menus/AppStarterState.cs"), "namespace AlvorStarter.Menus;");
    }

    /// <summary>Ignores local build output from the starter source project.</summary>
    [TestMethod]
    public void StarterSourceIgnoresBuildOutput()
    {
        using var workspace = TempWorkspace.Create("AlvorKit.Script.NewGame");
        var output = workspace.CreateDirectory("BuildOutputGame");
        var options = NewGameOptions.Parse(["BuildOutputGame", "--output", output], AlvorKitRoot);

        new NewGameGenerator().Generate(options);

        Assert.IsFalse(Directory.Exists(Path(output, "bin")));
        Assert.IsFalse(Directory.Exists(Path(output, "obj")));
        Assert.AreEqual(21, Directory.GetFiles(output, "*", SearchOption.AllDirectories).Length);
    }

    /// <summary>Rejects names that cannot become a C# namespace and project name.</summary>
    [TestMethod]
    public void ParseRejectsInvalidName()
    {
        var ex = Assert.ThrowsExactly<ArgumentException>(() =>
            NewGameOptions.Parse(["123"], AlvorKitRoot));

        StringAssert.Contains(ex.Message, "start with a letter");
    }

    /// <summary>Rejects non-empty output directories to avoid clobbering existing repositories.</summary>
    [TestMethod]
    public void GenerateRejectsNonEmptyOutput()
    {
        using var workspace = TempWorkspace.Create("AlvorKit.Script.NewGame");
        var output = workspace.CreateDirectory("ExistingGame");
        File.WriteAllText(System.IO.Path.Combine(output, "README.md"), "existing");
        var options = NewGameOptions.Parse(["ExistingGame", "--output", output], AlvorKitRoot);

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() =>
            new NewGameGenerator().Generate(options));

        StringAssert.Contains(ex.Message, "already exists");
    }

    private static Func<string> AlvorKitRoot => () =>
        AlvorKit.Script.Workspace.ProjectRoot.FindFromCurrentProcess(typeof(NewGameGeneratorTest));

    private static void AssertFile(string root, string relativePath) =>
        Assert.IsTrue(File.Exists(Path(root, relativePath)), $"Expected generated file '{relativePath}'.");

    private static string Read(string root, string relativePath) =>
        File.ReadAllText(Path(root, relativePath));

    private static void AssertDoesNotContain(string text, string unexpected) =>
        Assert.IsFalse(text.Contains(unexpected, StringComparison.Ordinal), $"Did not expect '{unexpected}'.");

    private static string Path(string root, string relativePath) =>
        System.IO.Path.Combine(root, relativePath.Replace('/', System.IO.Path.DirectorySeparatorChar));
}
