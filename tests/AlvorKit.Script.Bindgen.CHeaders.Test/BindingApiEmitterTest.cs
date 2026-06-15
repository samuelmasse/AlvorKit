namespace AlvorKit.Script.Bindgen.CHeaders.Test;

[TestClass]
public sealed class BindingApiEmitterTest
{
    /// <summary>The API class remains partial and does not emit constant fields.</summary>
    [TestMethod]
    public void Emit_DoesNotEmitConstantFields()
    {
        using var workspace = TempWorkspace.Create();
        var config = CHeaderTestConfig.Create();
        var model = new BindingModel([], [], [], [], [], [], []);

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0", "1.0.0");

        var api = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "Test.cs"));
        StringAssert.Contains(api, "public partial class Test");
        Assert.IsFalse(api.Contains("public const", StringComparison.Ordinal));
    }
}
