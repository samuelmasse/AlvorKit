using AlvorKit.Script.Bindgen;

namespace AlvorKit.Script.Bindgen.CHeaders.Test;

[TestClass]
public sealed class CHeaderBindingParserTest
{
    [TestMethod]
    public void Parse_IgnoresDeclarationsFromSiblingDirectoriesWithSamePrefix()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var sibling = workspace.CreateDirectory("source-other");
        var header = Path.Combine(source, "fixture.h");
        var siblingHeader = Path.Combine(sibling, "fixture_sibling.h");
        var translationUnit = Path.Combine(workspace.Root, "fixture.c");

        File.WriteAllText(header, """
            #define test_VISIBLE_CONSTANT 7
            int test_visible(void);
            const char* test_const_string(void);
            char* test_mutable_string(void);
            """);
        File.WriteAllText(siblingHeader, """
            #define test_HIDDEN_CONSTANT 9
            int test_hidden(void);
            """);
        File.WriteAllText(translationUnit, """
            #include "source/fixture.h"
            #include "source-other/fixture_sibling.h"
            """);

        var model = Parse(translationUnit, source);

        CollectionAssert.Contains(model.Functions.Select(function => function.NativeName).ToList(), "test_visible");
        CollectionAssert.DoesNotContain(model.Functions.Select(function => function.NativeName).ToList(), "test_hidden");
        CollectionAssert.Contains(model.Constants.Select(constant => constant.ManagedName).ToList(), "VisibleConstant");
        CollectionAssert.DoesNotContain(model.Constants.Select(constant => constant.ManagedName).ToList(), "HiddenConstant");
    }

    [TestMethod]
    public void Parse_OnlyConstCharPointerReturnsAreCStringConveniences()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var header = Path.Combine(source, "fixture.h");
        var translationUnit = Path.Combine(workspace.Root, "fixture.c");

        File.WriteAllText(header, """
            const char* test_const_string(void);
            char* test_mutable_string(void);
            """);
        File.WriteAllText(translationUnit, """#include "source/fixture.h" """);

        var model = Parse(translationUnit, source);

        Assert.IsTrue(model.Functions.Single(function => function.NativeName == "test_const_string").ReturnsCString);
        Assert.IsFalse(model.Functions.Single(function => function.NativeName == "test_mutable_string").ReturnsCString);
    }

    [TestMethod]
    public void Parse_ConfiguredInAndOutPointerParametersUsePointeeTypes()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = WriteHeader(workspace, source, """
            void test_copy(const int* input, int* output);
            """);
        var config = TestConfig();
        config.InParams = new() { ["test_copy"] = ["input"] };
        config.OutParams = new() { ["test_copy"] = ["output"] };

        var function = Parse(translationUnit, source, config).Functions.Single();

        Assert.AreEqual("in", function.Parameters[0].Modifier);
        Assert.AreEqual("int", function.Parameters[0].ManagedType);
        Assert.AreEqual("out", function.Parameters[1].Modifier);
        Assert.AreEqual("int", function.Parameters[1].ManagedType);
    }

    [TestMethod]
    public void Parse_ConfiguredBoolReturnsAndParametersKeepRawInteropTypes()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = WriteHeader(workspace, source, """
            int test_is_ready(void);
            void test_set_enabled(int enabled);
            """);
        var config = TestConfig();
        config.BoolReturns = ["test_is_ready"];
        config.BoolParams = new() { ["test_set_enabled"] = ["enabled"] };

        var model = Parse(translationUnit, source, config);
        var ready = model.Functions.Single(function => function.NativeName == "test_is_ready");
        var set = model.Functions.Single(function => function.NativeName == "test_set_enabled");

        Assert.AreEqual("bool", ready.ReturnType);
        Assert.AreEqual("int", ready.ReturnInteropType);
        Assert.AreEqual("bool", set.Parameters.Single().ManagedType);
        Assert.AreEqual("int", set.Parameters.Single().InteropType);
    }

    [TestMethod]
    public void Parse_OpaquePointerWithTypeRenameBecomesHandle()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = WriteHeader(workspace, source, """
            typedef struct test_handle test_handle;
            void test_use(test_handle* handle);
            """);
        var config = TestConfig();
        config.TypeRenames = new() { ["test_handle"] = "TestHandle" };

        var model = Parse(translationUnit, source, config);

        Assert.AreEqual("TestHandle", model.Handles.Single().ManagedName);
        Assert.AreEqual("TestHandle", model.Functions.Single().Parameters.Single().ManagedType);
    }

    [TestMethod]
    public void Parse_SynthesizesConfiguredEnumGroupsFromMacroConstants()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = WriteHeader(workspace, source, """
            #define test_MODE_A 1
            #define test_MODE_B 2
            #define test_OTHER 99
            """);
        var config = TestConfig();
        config.EnumGroups = new()
        {
            ["TestMode"] = new EnumGroup { Prefix = "test_MODE_", Flags = true }
        };

        var group = Parse(translationUnit, source, config).Enums.Single(binding => binding.ManagedName == "TestMode");

        Assert.IsTrue(group.IsFlags);
        CollectionAssert.AreEqual(new[] { "A", "B" }, group.Members.Select(member => member.ManagedName).ToArray());
    }

    private static string WriteHeader(TempWorkspace workspace, string source, string contents)
    {
        var header = Path.Combine(source, "fixture.h");
        var translationUnit = Path.Combine(workspace.Root, "fixture.c");
        File.WriteAllText(header, contents);
        File.WriteAllText(translationUnit, """#include "source/fixture.h" """);
        return translationUnit;
    }

    private static BindingModel Parse(string translationUnit, string source) =>
        Parse(translationUnit, source, TestConfig());

    private static BindingModel Parse(string translationUnit, string source, BindgenConfig config) =>
        new CHeaderBindingParser(config, config.ApiClass).Parse(
            translationUnit,
            includeDirectory: source,
            filterRoot: source,
            libraryDirectory: source,
            targetTriple: "x86_64-pc-windows-msvc");

    private static BindgenConfig TestConfig() => new()
    {
        Namespace = "AlvorKit.Bindgen.Fixture",
        ApiClass = "Test",
        ApiSummary = "Fixture API.",
        BackendClass = "TestBackend",
        NativeClass = "TestNative",
        NativeLibrary = "test",
        Prefix = "test_",
        WorkDir = "fixture-work",
        SourceDir = "fixture-source",
        Header = "fixture.h",
        ApiProject = "generated/Fixture",
        BackendProject = "generated/Fixture.Backend"
    };
}
