namespace AlvorKit;

/// <summary>Protects platform-dependent native records and portable managed function contracts.</summary>
[TestClass]
public class RuntimeBindingContractTest
{
    /// <summary>Parses pthread storage using each target's actual public libc ABI dimensions.</summary>
    [TestMethod]
    [DataRow("linux-x64", 40, 48)]
    [DataRow("linux-arm64", 48, 48)]
    [DataRow("linux-arm", 24, 48)]
    [DataRow("osx-x64", 64, 48)]
    [DataRow("osx-arm64", 64, 48)]
    public void PthreadRecordsUseTargetAbi(string runtime, int mutexSize, int conditionSize)
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            #include <pthread.h>
            typedef pthread_mutex_t test_mutex;
            typedef pthread_cond_t test_condition;
            void test_take(test_mutex* mutex, test_condition* condition);
            """);
        var config = CHeaderTestConfig.Create();
        config.TransparentStructs = ["test_mutex", "test_condition"];
        var model = new CHeaderBindingParser(config, config.ApiClass).Parse(
            translationUnit, source, source, source, BindingRuntimeTarget.Triple(runtime));
        Assert.AreEqual(mutexSize, model.Structs.Single(type => type.NativeName == "test_mutex").Size);
        Assert.AreEqual(conditionSize, model.Structs.Single(type => type.NativeName == "test_condition").Size);
    }

    /// <summary>Rejects a pointer projection that changes the callable API between runtime assemblies.</summary>
    [TestMethod]
    public void RejectsRuntimeSignatureDifference()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, "void test_take(int* pointer);");
        var original = CHeaderParserHarness.Parse(translationUnit, source);
        var changed = original with { Functions = [original.Functions[0] with { ReturnType = "nint" }] };
        Assert.ThrowsExactly<InvalidOperationException>(() => RuntimeBindingContract.Validate(
            new Dictionary<string, BindingModel> { ["win-x64"] = original, ["linux-x64"] = changed }));
    }

    /// <summary>Native probes check nested fields with real native paths and exclude synthesized native type names.</summary>
    [TestMethod]
    public void NativeAbiManifestChecksNestedFields()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            typedef struct test_outer { int prefix; struct { double value; } child; } test_outer;
            void test_take(test_outer* value);
            """);
        var model = CHeaderParserHarness.Parse(translationUnit, source);
        var entries = BindingAbiEmitter.Entries(model);
        Assert.IsTrue(entries.Any(entry => entry.Field == "Child.Value"
            && entry.NativeExpression == "offsetof(test_outer, child.value)"));
        Assert.IsFalse(entries.Any(entry => entry.NativeExpression.Contains("sizeof(test_outer_child)")));
    }
}
