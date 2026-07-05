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
            typedef struct test_image { int width; } test_image;
            void test_write(void* data, size_t dataSize);
            void test_read(const void* data, size_t dataSize);
            void test_icons(int count, const test_image* images);
            void test_flag(int enabled);
            void test_native_bool(_Bool enabled);
            void test_unnamed(int);
            """);
        var config = CHeaderTestConfig.Create();
        config.BoolParams = new() { ["test_flag"] = ["enabled"] };

        var model = CHeaderParserHarness.Parse(translationUnit, source, config);
        var write = model.Functions.Single(function => function.NativeName == "test_write");
        var read = model.Functions.Single(function => function.NativeName == "test_read");
        var icons = model.Functions.Single(function => function.NativeName == "test_icons");
        var flag = model.Functions.Single(function => function.NativeName == "test_flag");
        var nativeBool = model.Functions.Single(function => function.NativeName == "test_native_bool");
        var unnamed = model.Functions.Single(function => function.NativeName == "test_unnamed");

        Assert.IsTrue(write.Parameters[0].IsUntypedPointer);
        Assert.IsFalse(write.Parameters[0].IsConstPointee);
        Assert.IsTrue(write.Parameters[1].IsSizeT);
        Assert.IsTrue(read.Parameters[0].IsConstPointee);
        Assert.IsTrue(icons.Parameters[1].IsConstPointee);
        Assert.AreEqual("bool", flag.Parameters.Single().ManagedType);
        Assert.AreEqual("int", flag.Parameters.Single().InteropType);
        Assert.AreEqual("bool", nativeBool.Parameters.Single().ManagedType);
        Assert.AreEqual("bool", nativeBool.Parameters.Single().InteropType);
        Assert.AreEqual("arg0", unnamed.Parameters.Single().ManagedName);
    }

    /// <summary>Unsupported parameter types skip only the affected function and record the reason.</summary>
    [TestMethod]
    public void Parse_SkipsUnsupportedParameterType()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            void test_bad(_Complex float value);
            void test_good(int value);
            """);

        var model = CHeaderParserHarness.Parse(translationUnit, source);

        CollectionAssert.AreEqual(new[] { "test_good" }, model.Functions.Select(function => function.NativeName).ToArray());
        Assert.IsTrue(model.SkippedFunctions.Single().StartsWith("test_bad (param value:", StringComparison.Ordinal));
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

    /// <summary>Configured enum return types become the public return while native imports keep the raw type.</summary>
    [TestMethod]
    public void Parse_AppliesConfiguredEnumReturnType()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            typedef int test_pointer_alias;
            int test_get_mode(void);
            test_pointer_alias test_get_aliased_pointer(void);
            char* test_get_mutable_name(void);
            const char* test_get_const_name(void);
            const unsigned char* test_get_bytes_name(void);
            """);
        var config = CHeaderTestConfig.Create();
        config.TypeAliases = new() { ["test_pointer_alias"] = "nint" };
        config.EnumOverloads = new()
        {
            Functions = { ["test_get_mode"] = new() { Return = "TestMode" } }
        };

        var functions = CHeaderParserHarness.Parse(translationUnit, source, config).Functions;
        var function = functions.Single(item => item.NativeName == "test_get_mode");
        var aliased = functions.Single(item => item.NativeName == "test_get_aliased_pointer");
        var mutableName = functions.Single(item => item.NativeName == "test_get_mutable_name");
        var constName = functions.Single(item => item.NativeName == "test_get_const_name");
        var bytesName = functions.Single(item => item.NativeName == "test_get_bytes_name");

        Assert.AreEqual("TestMode", function.ReturnType);
        Assert.AreEqual("int", function.ReturnInteropType);
        Assert.IsFalse(aliased.ReturnsCString);
        Assert.IsFalse(mutableName.ReturnsCString);
        Assert.IsTrue(constName.ReturnsCString);
        Assert.IsFalse(bytesName.ReturnsCString);
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
    public void Parse_MarksConfiguredPlatformFunctions()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            void test_win_only(void);
            void test_mac_only(void);
            void test_everywhere(void);
            """);
        var config = CHeaderTestConfig.Create();
        config.PlatformFunctions = new()
        {
            ["windows"] = ["test_win_only"],
            ["macos"] = ["test_mac_only"]
        };

        var model = CHeaderParserHarness.Parse(translationUnit, source, config);

        Assert.AreEqual("windows", model.Functions.Single(function => function.NativeName == "test_win_only").Platform);
        Assert.AreEqual("macos", model.Functions.Single(function => function.NativeName == "test_mac_only").Platform);
        Assert.IsNull(model.Functions.Single(function => function.NativeName == "test_everywhere").Platform);
    }

    [TestMethod]
    public void Parse_RejectsFunctionListedUnderMultiplePlatforms()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            void test_dual(void);
            """);
        var config = CHeaderTestConfig.Create();
        config.PlatformFunctions = new()
        {
            ["windows"] = ["test_dual"],
            ["linux"] = ["test_dual"]
        };

        var exception = Assert.ThrowsExactly<InvalidOperationException>(() =>
            CHeaderParserHarness.Parse(translationUnit, source, config));

        StringAssert.Contains(exception.Message, "test_dual");
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
