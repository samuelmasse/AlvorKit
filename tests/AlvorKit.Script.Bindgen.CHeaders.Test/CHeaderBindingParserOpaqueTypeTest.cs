namespace AlvorKit.Script.Bindgen.CHeaders.Test;

/// <summary>Covers configured opaque pointer handle projection.</summary>
[TestClass]
public sealed class CHeaderBindingParserOpaqueTypeTest
{
    /// <summary>Visible native records can still be surfaced as opaque handles.</summary>
    [TestMethod]
    public void Parse_UsesConfiguredOpaqueTypesForVisibleRecords()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            typedef struct test_state {
                int value;
            } test_state;
            test_state* test_create(void);
            void test_update(test_state* state);
            """);
        var config = CHeaderTestConfig.Create();
        config.OpaqueTypes = new() { ["test_state"] = "TestState" };

        var model = CHeaderParserHarness.Parse(translationUnit, source, config);
        var create = model.Functions.Single(function => function.NativeName == "test_create");
        var update = model.Functions.Single(function => function.NativeName == "test_update");

        Assert.AreEqual("TestState", model.Handles.Single().ManagedName);
        Assert.AreEqual("TestState", create.ReturnType);
        Assert.AreEqual("TestState", update.Parameters.Single().ManagedType);
        Assert.AreEqual(0, model.Structs.Count);
    }

    /// <summary>Explicit opaque mappings do not depend on Clang reporting a record canonical type.</summary>
    [TestMethod]
    public void Parse_UsesConfiguredOpaqueTypesWithoutRecordCanonicalTypes()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            typedef int test_state;
            test_state* test_create(void);
            """);
        var config = CHeaderTestConfig.Create();
        config.OpaqueTypes = new() { ["int"] = "TestState" };

        var model = CHeaderParserHarness.Parse(translationUnit, source, config);

        Assert.AreEqual("TestState", model.Handles.Single().ManagedName);
        Assert.AreEqual("TestState", model.Functions.Single().ReturnType);
    }
}
