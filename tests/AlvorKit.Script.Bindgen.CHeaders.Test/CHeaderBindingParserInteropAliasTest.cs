namespace AlvorKit.Script.Bindgen.CHeaders.Test;

[TestClass]
public sealed class CHeaderBindingParserInteropAliasTest
{
    [TestMethod]
    public void Parse_UsesConfiguredInteropTypeAliasesForPrimitiveRawSignatures()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            unsigned int test_read(unsigned int value);
            """);
        var config = CHeaderTestConfig.Create();
        config.TypeAliases = new() { ["unsigned int"] = "TestUInt" };
        config.InteropTypeAliases = new() { ["unsigned int"] = "uint" };

        var model = CHeaderParserHarness.Parse(translationUnit, source, config);
        var function = model.Functions.Single();
        var parameter = function.Parameters.Single();

        Assert.AreEqual("TestUInt", function.ReturnType);
        Assert.AreEqual("uint", function.ReturnInteropType);
        Assert.AreEqual("TestUInt", parameter.ManagedType);
        Assert.AreEqual("uint", parameter.InteropType);
    }

    [TestMethod]
    public void Parse_UsesConfiguredInteropTypeAliasesForRawSignatures()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            typedef struct test_hash128 {
                unsigned long long low64;
                unsigned long long high64;
            } test_hash128;
            test_hash128 test_hash_data(test_hash128 seed);
            """);
        var config = CHeaderTestConfig.Create();
        config.TypeAliases = new() { ["test_hash128"] = "UInt128" };
        config.InteropTypeAliases = new() { ["test_hash128"] = "TestHash128" };

        var model = CHeaderParserHarness.Parse(translationUnit, source, config);
        var function = model.Functions.Single();
        var parameter = function.Parameters.Single();

        Assert.AreEqual("UInt128", function.ReturnType);
        Assert.AreEqual("TestHash128", function.ReturnInteropType);
        Assert.AreEqual("UInt128", parameter.ManagedType);
        Assert.AreEqual("TestHash128", parameter.InteropType);
        Assert.AreEqual("TestHash128", model.Structs.Single().ManagedName);
    }

    [TestMethod]
    public void Parse_SkipsInteropTypeAliasWhenRecordCannotBeGenerated()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            typedef struct test_hash128 {
                long double value;
            } test_hash128;
            test_hash128 test_hash_data(void);
            void test_take_hash(test_hash128 hash);
            """);
        var config = CHeaderTestConfig.Create();
        config.TypeAliases = new() { ["test_hash128"] = "UInt128" };
        config.InteropTypeAliases = new() { ["test_hash128"] = "TestHash128" };

        var model = CHeaderParserHarness.Parse(translationUnit, source, config);

        Assert.AreEqual(0, model.Functions.Count);
        CollectionAssert.Contains(model.SkippedFunctions, "test_hash_data (return interop type: test_hash128)");
        CollectionAssert.Contains(model.SkippedFunctions, "test_take_hash (interop param hash: test_hash128)");
    }
}
