namespace AlvorKit.Script.Bindgen.CHeaders.Test;

[TestClass]
public sealed class BindingApiEmitterTest
{
    /// <summary>The API class remains partial and does not emit constant fields.</summary>
    [TestMethod]
    public void Emit_DoesNotEmitConstantFields()
    {
        var api = EmitApi();

        StringAssert.Contains(api, "public partial class Test");
        Assert.IsFalse(api.Contains("public const", StringComparison.Ordinal));
    }

    /// <summary>Emits the minimal API contract used by tests.</summary>
    private static string EmitApi()
    {
        var config = CHeaderTestConfig.Create();
        var model = new BindingModel([], [], [], [], [], [], []);

        return new BindingApiEmitter(new(config, "1.0.0")).ApiContract(model);
    }
}
