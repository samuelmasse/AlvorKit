namespace AlvorKit.Script.Bindgen.CHeaders.Test;

/// <summary>Covers generated FastNoise2 span overloads.</summary>
[TestClass]
public sealed class BindingFastNoise2OverloadEmitterTest
{
    /// <summary>FastNoise2 convenience emits span overloads for uniform grids and position arrays.</summary>
    [TestMethod]
    public void Emit_FastNoise2ConvenienceAddsSpanOverloads()
    {
        using var workspace = TempWorkspace.Create();
        var config = CHeaderTestConfig.Create();
        config.ApiClass = "Fn";
        config.FastNoise2Convenience = true;
        var model = new BindingModel(
            Enums: [],
            Structs: [],
            Handles: [],
            Delegates: [],
            Functions: [UniformGrid3D(), PositionArray3D()],
            SkippedFunctions: [],
            SizeofTypes: []);

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0", "1.0.0");

        var overloads = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "FnOverloads.cs"));
        StringAssert.Contains(overloads, "public unsafe partial class Fn");
        StringAssert.Contains(overloads, "public unsafe void GenUniformGrid3D(FnNode node, Span<float> noiseOut");
        Assert.IsFalse(overloads.Contains("ArgumentOutOfRangeException", StringComparison.Ordinal));
        Assert.IsFalse(overloads.Contains("ArgumentException", StringComparison.Ordinal));
        Assert.IsFalse(overloads.Contains("var required = checked", StringComparison.Ordinal));
        StringAssert.Contains(overloads, "fixed (float* noiseOutPtr = noiseOut)");
        StringAssert.Contains(overloads, "GenUniformGrid3D(node, (nint)noiseOutPtr");
        StringAssert.Contains(overloads, "seed, 0);");
        StringAssert.Contains(overloads, "int seed, Span<float> outputMinMax)");
        StringAssert.Contains(overloads, "fixed (float* outputMinMaxPtr = outputMinMax)");
        StringAssert.Contains(overloads, "seed, (nint)outputMinMaxPtr);");
        StringAssert.Contains(overloads, "ReadOnlySpan<float> xPosArray");
        StringAssert.Contains(overloads, "ReadOnlySpan<float> zPosArray");
        StringAssert.Contains(overloads, "fixed (float* zPosArrayPtr = zPosArray)");
        Assert.AreEqual(2, Count(overloads, "public unsafe void GenUniformGrid3D("));
        Assert.AreEqual(2, Count(overloads, "public unsafe void GenPositionArray3D("));
    }

    /// <summary>Builds a FastNoise2 3D uniform grid function fixture.</summary>
    private static BindingFunction UniformGrid3D() =>
        Function(
            "fnGenUniformGrid3D",
            "GenUniformGrid3D",
            [
                Param("node", "FnNode"),
                Param("noiseOut", "nint"),
                Param("xOffset", "float"),
                Param("yOffset", "float"),
                Param("zOffset", "float"),
                Param("xCount", "int"),
                Param("yCount", "int"),
                Param("zCount", "int"),
                Param("xStepSize", "float"),
                Param("yStepSize", "float"),
                Param("zStepSize", "float"),
                Param("seed", "int"),
                Param("outputMinMax", "nint")
            ]);

    /// <summary>Builds a FastNoise2 3D position array function fixture.</summary>
    private static BindingFunction PositionArray3D() =>
        Function(
            "fnGenPositionArray3D",
            "GenPositionArray3D",
            [
                Param("node", "FnNode"),
                Param("noiseOut", "nint"),
                Param("count", "int"),
                Param("xPosArray", "nint"),
                Param("yPosArray", "nint"),
                Param("zPosArray", "nint"),
                Param("xOffset", "float"),
                Param("yOffset", "float"),
                Param("zOffset", "float"),
                Param("seed", "int"),
                Param("outputMinMax", "nint")
            ]);

    /// <summary>Builds a void-returning fixture binding function.</summary>
    private static BindingFunction Function(string nativeName, string managedName, List<BindingParameter> parameters) =>
        new(nativeName, managedName, "void", "void", parameters, Documentation: null);

    /// <summary>Builds a fixture parameter with the same native and managed type.</summary>
    private static BindingParameter Param(string name, string type) =>
        new(name, type, type, "", HasStringConvenience: false);

    /// <summary>Counts non-overlapping text occurrences.</summary>
    private static int Count(string source, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = source.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}
