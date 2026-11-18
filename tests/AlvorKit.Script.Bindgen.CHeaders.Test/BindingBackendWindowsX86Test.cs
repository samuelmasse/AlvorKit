namespace AlvorKit;

/// <summary>Checks the generated boundary for Windows x86 cdecl return values.</summary>
[TestClass]
public class BindingBackendWindowsX86Test
{
    /// <summary>Only calls that can return a value across a single stack word need the isolated helper.</summary>
    [TestMethod]
    [DataRow("int", 0, null, true)]
    [DataRow("int", 1, "windows", true)]
    [DataRow("int", 2, null, false)]
    [DataRow("void", 1, null, false)]
    [DataRow("int", 1, "linux", false)]
    public void Emit_IsolatesWindowsX86Returns(string returnType, int count, string? platform, bool isolated)
    {
        using var workspace = TempWorkspace.Create();
        var config = CHeaderTestConfig.Create();
        List<BindingParameter> parameters = [];
        for (var i = 0; i < count; i++)
            parameters.Add(new($"value{i}", "int", "int", "", HasStringConvenience: false));

        var function = new BindingFunction("test_call", "Call", returnType, returnType, parameters, null, Platform: platform);
        var model = new BindingModel([], [], [], [], [function], [], []);
        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0", "1.0.0");

        var backend = File.ReadAllText(Path.Combine(workspace.Root, config.BackendProject, "TestBackend.cs"));
        Assert.AreEqual(isolated, backend.Contains("MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization", StringComparison.Ordinal));
        Assert.AreEqual(isolated, backend.Contains("IntPtr.Size == 4 && OperatingSystem.IsWindows()", StringComparison.Ordinal));
    }
}
