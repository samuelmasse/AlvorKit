namespace AlvorKit.Script.Bindgen.CHeaders.Test;

[TestClass]
public sealed class CHeaderBindingParserAnonymousRecordTest
{
    [TestMethod]
    public void Parse_AnonymousUnionFieldsGetStableSyntheticNames()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            typedef struct test_outer {
                int before;
                union {
                    int as_int;
                    float as_float;
                };
                union {
                    short code;
                };
            } test_outer;
            """);
        var config = CHeaderTestConfig.Create();
        config.TransparentStructs = ["test_outer"];

        var model = CHeaderParserHarness.Parse(translationUnit, source, config);
        var outer = model.Structs.Single(type => type.NativeName == "test_outer");
        var firstUnion = model.Structs.Single(type => type.NativeName == "test_outer_anonymous0");
        var secondUnion = model.Structs.Single(type => type.NativeName == "test_outer_anonymous1");

        Assert.AreEqual("TestOuterAnonymous0", outer.Fields.Single(field => field.ManagedName == "Anonymous0").ManagedType);
        Assert.AreEqual("TestOuterAnonymous1", outer.Fields.Single(field => field.ManagedName == "Anonymous1").ManagedType);
        Assert.IsTrue(firstUnion.IsUnion);
        Assert.IsTrue(secondUnion.IsUnion);
        CollectionAssert.AreEquivalent(
            new[] { "Before", "Anonymous0", "Anonymous1" },
            outer.Fields.Select(field => field.ManagedName).ToArray());
        CollectionAssert.AreEquivalent(
            new[] { "AsInt", "AsFloat" },
            firstUnion.Fields.Select(field => field.ManagedName).ToArray());
        CollectionAssert.AreEquivalent(
            new[] { "Code" },
            secondUnion.Fields.Select(field => field.ManagedName).ToArray());
    }
}
