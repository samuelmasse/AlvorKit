using AlvorKit.Script.Bindgen;

namespace AlvorKit.Script.Bindgen.CHeaders.Test;

[TestClass]
public sealed class CHeaderBindingParserStructTest
{
    [TestMethod]
    public void Parse_TransparentStructEmitsFieldsAndInlineBuffers()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            typedef struct test_point {
                int x;
                int values[4];
            } test_point;
            void test_take(test_point point);
            """);
        var config = CHeaderTestConfig.Create();
        config.TransparentStructs = ["test_point"];

        var model = CHeaderParserHarness.Parse(translationUnit, source, config);
        var point = model.Structs.Single(type => type.NativeName == "test_point");

        Assert.AreEqual("TestPoint", point.ManagedName);
        Assert.AreEqual("int", point.Fields.Single(field => field.ManagedName == "X").ManagedType);
        Assert.AreEqual("ValuesBuffer", point.Fields.Single(field => field.ManagedName == "Values").ManagedType);
        Assert.AreEqual(4, point.NestedBuffers.Single().Count);
        Assert.AreEqual("TestPoint", model.Functions.Single().Parameters.Single().ManagedType);
    }

    [TestMethod]
    public void Parse_AnonymousNestedStructSynthesizesManagedStruct()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            typedef struct test_outer {
                struct { int x; } child;
            } test_outer;
            """);
        var config = CHeaderTestConfig.Create();
        config.TransparentStructs = ["test_outer"];

        var model = CHeaderParserHarness.Parse(translationUnit, source, config);
        var outer = model.Structs.Single(type => type.NativeName == "test_outer");

        Assert.AreEqual("TestOuterChild", outer.Fields.Single().ManagedType);
        Assert.IsTrue(model.Structs.Any(type => type.NativeName == "test_outer_child"));
    }

    [TestMethod]
    public void Parse_ThrowsForBitfieldLayout()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            typedef struct test_flags {
                unsigned int bits:1;
            } test_flags;
            """);
        var ex = Assert.ThrowsExactly<InvalidOperationException>(() =>
            CHeaderBindingParser.ValidateNaturalLayout(
                CHeaderTestConfig.Create(),
                translationUnit,
                includeDirectory: source,
                filterRoot: source,
                libraryDirectory: source,
                targetTriple: "x86_64-pc-windows-msvc",
                nativeStructNames: ["test_flags"]));

        StringAssert.Contains(ex.Message, "bitfield");
    }
}
