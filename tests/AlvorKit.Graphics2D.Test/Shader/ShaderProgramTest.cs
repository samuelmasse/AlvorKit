namespace AlvorKit.Graphics2D.Test;

/// <summary>Tests shader program link behavior.</summary>
[TestClass]
public sealed class ShaderProgramTest
{
    /// <summary>A successful link returns a tracked program handle that can be deleted.</summary>
    [TestMethod]
    public void Constructor_WithSuccessfulLink_CreatesProgram()
    {
        var (backend, gl) = Graphics2DTestHarness.CreateLayer();
        using var stage = new ShaderStage(gl, "ok", "void main() {}", GlShaderType.FragmentShader);
        var program = new ShaderProgram(gl, "ok", [stage]);
        var id = (uint)program.Id;

        program.Dispose();

        CollectionAssert.Contains(backend.Deleted, id);
    }

    /// <summary>A failed link deletes the temporary program before throwing.</summary>
    [TestMethod]
    public void Constructor_WithLinkError_DeletesProgramAndThrows()
    {
        var (backend, gl) = Graphics2DTestHarness.CreateLayer();
        using var stage = new ShaderStage(gl, "ok", "void main() {}", GlShaderType.FragmentShader);
        backend.ProgramLinkStatus = 0;

        Assert.Throws<InvalidOperationException>(() => new ShaderProgram(gl, "bad", [stage]));
        CollectionAssert.Contains(backend.Deleted, 2u);
    }

    /// <summary>The unlabeled constructor links and owns a shader program.</summary>
    [TestMethod]
    public void Constructor_WithoutLabel_CreatesProgram()
    {
        var (backend, gl) = Graphics2DTestHarness.CreateLayer();
        using var stage = new ShaderStage(gl, "ok", "void main() {}", GlShaderType.FragmentShader);
        var program = new ShaderProgram(gl, [stage]);
        var id = (uint)program.Id;

        program.Dispose();

        CollectionAssert.Contains(backend.Deleted, id);
    }
}
