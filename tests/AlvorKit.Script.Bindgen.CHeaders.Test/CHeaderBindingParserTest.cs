namespace AlvorKit.Script.Bindgen.CHeaders.Test;

[TestClass]
public sealed class CHeaderBindingParserTest
{
    /// <summary>Parser scope follows the selected source root when sibling paths share the same prefix.</summary>
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

        var model = CHeaderParserHarness.Parse(translationUnit, source);

        CollectionAssert.Contains(model.Functions.Select(function => function.NativeName).ToList(), "test_visible");
        CollectionAssert.DoesNotContain(model.Functions.Select(function => function.NativeName).ToList(), "test_hidden");
        CollectionAssert.Contains(model.Constants.Select(constant => constant.ManagedName).ToList(), "VisibleConstant");
        CollectionAssert.DoesNotContain(model.Constants.Select(constant => constant.ManagedName).ToList(), "HiddenConstant");
    }

    /// <summary>Only const char pointer returns are surfaced as generated C string conveniences.</summary>
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

        var model = CHeaderParserHarness.Parse(translationUnit, source);

        Assert.IsTrue(model.Functions.Single(function => function.NativeName == "test_const_string").ReturnsCString);
        Assert.IsFalse(model.Functions.Single(function => function.NativeName == "test_mutable_string").ReturnsCString);
    }

    /// <summary>The configured implementation file directory is added to parser include roots.</summary>
    [TestMethod]
    public void Parse_AddsImplFileDirectoryToIncludeRoots()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var implDirectory = Directory.CreateDirectory(Path.Combine(source, "src")).FullName;
        var header = Path.Combine(source, "fixture.h");
        var translationUnit = Path.Combine(workspace.Root, "fixture.c");
        var siblingImpl = Path.Combine(implDirectory, "alvorkit.c");

        File.WriteAllText(header, "void test_visible(void);");
        File.WriteAllText(siblingImpl, "int alvorkit_value(void) { return 1; }");
        File.WriteAllText(translationUnit, """
            #include "fixture.h"
            #include "alvorkit.c"
            """);

        var config = CHeaderTestConfig.Create();
        config.ImplFile = "src/miniaudio.c";
        var model = CHeaderParserHarness.Parse(translationUnit, source, config);

        CollectionAssert.Contains(model.Functions.Select(function => function.NativeName).ToList(), "test_visible");
    }

    /// <summary>Configured pointer directions replace raw pointer parameters with in and out shapes.</summary>
    [TestMethod]
    public void Parse_ConfiguredInAndOutPointerParametersUsePointeeTypes()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            void test_copy(const int* input, int* output);
            """);
        var config = CHeaderTestConfig.Create();
        config.InParams = new() { ["test_copy"] = ["input"] };
        config.OutParams = new() { ["test_copy"] = ["output"] };

        var function = CHeaderParserHarness.Parse(translationUnit, source, config).Functions.Single();

        Assert.AreEqual("in", function.Parameters[0].Modifier);
        Assert.AreEqual("int", function.Parameters[0].ManagedType);
        Assert.AreEqual("out", function.Parameters[1].Modifier);
        Assert.AreEqual("int", function.Parameters[1].ManagedType);
    }

    /// <summary>Configured boolean shapes keep raw interop types while exposing managed bools.</summary>
    [TestMethod]
    public void Parse_ConfiguredBoolReturnsAndParametersKeepRawInteropTypes()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            int test_is_ready(void);
            void test_set_enabled(int enabled);
            """);
        var config = CHeaderTestConfig.Create();
        config.BoolReturns = ["test_is_ready"];
        config.BoolParams = new() { ["test_set_enabled"] = ["enabled"] };

        var model = CHeaderParserHarness.Parse(translationUnit, source, config);
        var ready = model.Functions.Single(function => function.NativeName == "test_is_ready");
        var set = model.Functions.Single(function => function.NativeName == "test_set_enabled");

        Assert.AreEqual("bool", ready.ReturnType);
        Assert.AreEqual("int", ready.ReturnInteropType);
        Assert.AreEqual("bool", set.Parameters.Single().ManagedType);
        Assert.AreEqual("int", set.Parameters.Single().InteropType);
    }

    /// <summary>Opaque pointer records with type renames become generated handle wrappers.</summary>
    [TestMethod]
    public void Parse_OpaquePointerWithTypeRenameBecomesHandle()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            typedef struct test_handle test_handle;
            void test_use(test_handle* handle);
            """);
        var config = CHeaderTestConfig.Create();
        config.TypeRenames = new() { ["test_handle"] = "TestHandle" };

        var model = CHeaderParserHarness.Parse(translationUnit, source, config);

        Assert.AreEqual("TestHandle", model.Handles.Single().ManagedName);
        Assert.AreEqual("TestHandle", model.Functions.Single().Parameters.Single().ManagedType);
    }

    /// <summary>Configured macro groups synthesize enum members from discovered constants.</summary>
    [TestMethod]
    public void Parse_SynthesizesConfiguredEnumGroupsFromMacroConstants()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            #define test_MODE_A 1
            #define test_MODE_B 2
            #define test_OTHER 99
            """);
        var config = CHeaderTestConfig.Create();
        config.EnumGroups = new()
        {
            ["TestMode"] = new EnumGroup { Prefix = "test_MODE_", Flags = true }
        };

        var group = CHeaderParserHarness.Parse(translationUnit, source, config).Enums.Single(binding => binding.ManagedName == "TestMode");

        Assert.IsTrue(group.IsFlags);
        CollectionAssert.AreEqual(new[] { "A", "B" }, group.Members.Select(member => member.ManagedName).ToArray());
    }

    /// <summary>Configured native constants participate in macro evaluation and enum synthesis.</summary>
    [TestMethod]
    public void Parse_ConfiguredNativeConstantsSeedEnumGroups()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            #define test_MODE_A 1
            #define test_MODE_C UNKNOWN
            #define test_MODE_BAD UNKNOWN
            #define test_MODE_ALL ( test_MODE_A | test_MODE_B | test_MODE_C )
            #define test_EMPTY
            #define test_COLLIDE 11
            int test_collide(void);
            """);
        var config = CHeaderTestConfig.Create();
        config.Constants = new()
        {
            ["test_MODE_B"] = 2,
            ["test_MODE_C"] = 4,
            ["ManagedOnly"] = 9
        };
        config.EnumGroups = new()
        {
            ["TestMode"] = new EnumGroup { Prefix = "test_MODE_", Flags = true }
        };

        var model = CHeaderParserHarness.Parse(translationUnit, source, config);
        var group = model.Enums.Single(binding => binding.ManagedName == "TestMode");
        var members = group.Members.ToDictionary(member => member.ManagedName, member => member.Value);

        Assert.AreEqual(1, members["A"]);
        Assert.AreEqual(2, members["B"]);
        Assert.AreEqual(4, members["C"]);
        Assert.AreEqual(7, members["All"]);
        CollectionAssert.DoesNotContain(group.Members.Select(member => member.ManagedName).ToList(), "Bad");
        CollectionAssert.Contains(model.Constants.Select(constant => constant.ManagedName).ToList(), "ModeB");
        CollectionAssert.Contains(model.Constants.Select(constant => constant.ManagedName).ToList(), "ManagedOnly");
        CollectionAssert.DoesNotContain(model.Constants.Select(constant => constant.ManagedName).ToList(), "Empty");
        CollectionAssert.DoesNotContain(model.Constants.Select(constant => constant.ManagedName).ToList(), "Collide");
    }

}
