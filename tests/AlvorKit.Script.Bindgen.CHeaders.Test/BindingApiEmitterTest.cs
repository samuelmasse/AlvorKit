namespace AlvorKit.Script.Bindgen.CHeaders.Test;

[TestClass]
public sealed class BindingApiEmitterTest
{
    /// <summary>The API class remains partial and emits constants with the smallest safe integral type.</summary>
    [TestMethod]
    public void Emit_UsesPartialClassAndSizedConstants()
    {
        using var workspace = TempWorkspace.Create();
        var config = CHeaderTestConfig.Create();
        var model = new BindingModel(
            [],
            [],
            [],
            [],
            [],
            [new("Small", 1), new("Large", (long)int.MaxValue + 1)],
            [],
            []);

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0", "1.0.0");

        var api = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "Test.cs"));
        StringAssert.Contains(api, "public partial class Test");
        StringAssert.Contains(api, "public const int Small = 1;");
        StringAssert.Contains(api, "public const long Large = 2147483648;");
    }
}
