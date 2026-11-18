namespace AlvorKit;

[TestClass]
public class StandaloneBlendTest
{
    /// <summary>The standalone host can deploy Blend and its dependency closure without any engine assemblies.</summary>
    [TestMethod]
    public void StandaloneDeployment_ContainsNoEngineLibraries()
    {
        var manifest = Path.ChangeExtension(typeof(StandaloneBlendTest).Assembly.Location, ".deps.json");
        using var dependencies = JsonDocument.Parse(File.ReadAllText(manifest));
        var libraries = dependencies.RootElement.GetProperty("libraries").EnumerateObject();
        var engine = libraries.Where(lib => lib.Name.StartsWith("AlvorKit.Engine.", StringComparison.Ordinal));
        Assert.AreEqual("", string.Join(", ", engine.Select(lib => lib.Name)));
    }

    /// <summary>A window host can click and draw a textured Blend button using ordinary UI and font objects.</summary>
    [TestMethod]
    public void Button_ReceivesWindowInputAndSubmitsSpriteDraws()
    {
        using var h = new UiTestHarness();
        using var context = new FontContext(h.Gl, new FtBackend(), h.Batch);
        var fonts = new BlendFonts(options => new Font(context, options));
        using var regular = fonts.Regular;
        using var semiBold = fonts.SemiBold;
        var blend = new BlendUi(h.Runtime, fonts, h.Gl);
        var clicks = 0;
        Node(h.Ui, out var button)
            .Mutate(blend.S.Button)
            .TextV("Apply")
            .SizeV((100, 40))
            .OnClickF(() => clicks++);

        h.MoveMouse((10, 10));
        h.Update();
        h.Host.RaiseMouseDown(MouseButton.Left);
        h.Update();
        h.Host.RaiseMouseUp(MouseButton.Left);
        h.Update();
        h.Draw();

        Assert.AreEqual(1, clicks);
        Assert.IsNotNull(Nodes(button)[0].TextureFV.Resolve());
        Assert.IsTrue(h.Backend.DrawCount > 0);
    }
}
