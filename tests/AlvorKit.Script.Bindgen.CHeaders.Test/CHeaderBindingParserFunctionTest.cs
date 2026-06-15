namespace AlvorKit.Script.Bindgen.CHeaders.Test;

[TestClass]
public sealed class CHeaderBindingParserFunctionTest
{
    [TestMethod]
    public void Parse_SkipsConfiguredAndVariadicFunctions()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            void test_skip(void);
            void test_log(const char* format, ...);
            static void test_static(void);
            void test_keep(void);
            """);
        var config = CHeaderTestConfig.Create();
        config.Skip = new() { ["test_skip"] = "manual" };

        var model = CHeaderParserHarness.Parse(translationUnit, source, config);

        CollectionAssert.AreEqual(new[] { "test_keep" }, model.Functions.Select(function => function.NativeName).ToArray());
        CollectionAssert.Contains(model.SkippedFunctions, "test_skip (manual)");
        CollectionAssert.Contains(model.SkippedFunctions, "test_log (variadic)");
    }

    [TestMethod]
    public void Parse_DetectsUntypedPointerSpanCandidates()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            #include <stddef.h>
            void test_write(void* data, size_t dataSize);
            void test_read(const void* data, size_t dataSize);
            """);

        var model = CHeaderParserHarness.Parse(translationUnit, source);
        var write = model.Functions.Single(function => function.NativeName == "test_write");
        var read = model.Functions.Single(function => function.NativeName == "test_read");

        Assert.IsTrue(write.Parameters[0].IsUntypedPointer);
        Assert.IsFalse(write.Parameters[0].IsConstPointee);
        Assert.IsTrue(write.Parameters[1].IsSizeT);
        Assert.IsTrue(read.Parameters[0].IsConstPointee);
    }

    [TestMethod]
    public void Parse_AppliesConfiguredFunctionRenames()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            void test_hash3_64bits(void);
            void test_hash3_64bits_withSeed(unsigned long long seed);
            """);
        var config = CHeaderTestConfig.Create();
        config.FunctionRenames = new()
        {
            ["test_hash3_64bits"] = "Hash3To64",
            ["test_hash3_64bits_withSeed"] = "Hash3To64"
        };

        var model = CHeaderParserHarness.Parse(translationUnit, source, config);

        CollectionAssert.AreEqual(
            new[] { "Hash3To64", "Hash3To64" },
            model.Functions.Select(function => function.ManagedName).ToArray());
    }

    [TestMethod]
    public void Parse_MarksConfiguredAdvancedFunctions()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            void test_raw(void* data);
            void test_friendly(void);
            """);
        var config = CHeaderTestConfig.Create();
        config.AdvancedFunctions = ["test_raw"];

        var model = CHeaderParserHarness.Parse(translationUnit, source, config);

        Assert.IsTrue(model.Functions.Single(function => function.NativeName == "test_raw").IsAdvanced);
        Assert.IsFalse(model.Functions.Single(function => function.NativeName == "test_friendly").IsAdvanced);
    }

    [TestMethod]
    public void Parse_UsesConfiguredTypeAliasesWithoutEmittingStructs()
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

        var model = CHeaderParserHarness.Parse(translationUnit, source, config);
        var function = model.Functions.Single();

        Assert.AreEqual("UInt128", function.ReturnType);
        Assert.AreEqual("UInt128", function.Parameters.Single().ManagedType);
        Assert.AreEqual(0, model.Structs.Count);
    }

    [TestMethod]
    public void Parse_TracksSizeofCandidateForStructInitFunction()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            typedef struct test_context { int value; } test_context;
            void test_context_init(test_context* context);
            """);

        var model = CHeaderParserHarness.Parse(translationUnit, source);

        CollectionAssert.Contains(model.SizeofTypes, "test_context");
    }
}
