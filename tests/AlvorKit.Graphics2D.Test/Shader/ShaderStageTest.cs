namespace AlvorKit.Graphics2D.Test;

/// <summary>Tests shader stage compile behavior.</summary>
[TestClass]
public sealed class ShaderStageTest
{
    /// <summary>A successful compile returns a tracked shader handle that can be deleted.</summary>
    [TestMethod]
    public void Constructor_WithSuccessfulCompile_CreatesShader()
    {
        var (backend, gl) = Graphics2DTestHarness.CreateLayer();
        var stage = new ShaderStage(gl, "ok", "void main() {}", GlShaderType.FragmentShader);
        var id = (uint)stage.Id;

        stage.Dispose();

        CollectionAssert.Contains(backend.Deleted, id);
    }

    /// <summary>A failed compile deletes the temporary shader before throwing.</summary>
    [TestMethod]
    public void Constructor_WithCompileError_DeletesShaderAndThrows()
    {
        var (backend, gl) = Graphics2DTestHarness.CreateLayer();
        backend.ShaderCompileStatus = 0;

        Assert.Throws<InvalidOperationException>(() => new ShaderStage(gl, "bad", string.Empty, GlShaderType.FragmentShader));
        CollectionAssert.Contains(backend.Deleted, 1u);
    }

    /// <summary>The unlabeled constructor compiles and owns a shader stage.</summary>
    [TestMethod]
    public void Constructor_WithoutLabel_CreatesShader()
    {
        var (backend, gl) = Graphics2DTestHarness.CreateLayer();
        var stage = new ShaderStage(gl, "void main() {}", GlShaderType.FragmentShader);
        var id = (uint)stage.Id;

        stage.Dispose();

        CollectionAssert.Contains(backend.Deleted, id);
    }
}
