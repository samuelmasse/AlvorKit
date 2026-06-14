using AlvorKit.Script.Bindgen;

namespace AlvorKit.Script.Bindgen.CHeaders.Test;

[TestClass]
public sealed class CHeaderBindingParserCallbackTest
{
    [TestMethod]
    public void Parse_CallbackTypedefUsedByFunctionEmitsDelegate()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            typedef void (*test_callback)(int value);
            void test_set_callback(test_callback callback);
            """);

        var model = CHeaderParserHarness.Parse(translationUnit, source);
        var callback = model.Delegates.Single();
        var parameter = model.Functions.Single().Parameters.Single();

        Assert.AreEqual("TestCallback", callback.ManagedName);
        Assert.AreEqual("@value", callback.Parameters.Single().ManagedName);
        Assert.AreEqual("TestCallback", parameter.CallbackType);
        Assert.AreEqual("nint", parameter.ManagedType);
    }

    [TestMethod]
    public void Parse_UnusedCallbackTypedefIsNotEmitted()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            typedef void (*test_callback)(int value);
            int test_value(void);
            """);

        var model = CHeaderParserHarness.Parse(translationUnit, source);

        Assert.AreEqual(0, model.Delegates.Count);
        Assert.AreEqual("test_value", model.Functions.Single().NativeName);
    }
}
