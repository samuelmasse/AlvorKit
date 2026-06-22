namespace AlvorKit.Engine;

/// <summary>Base class for shader programs with view and projection matrix uniforms.</summary>
[ExcludeFromCodeCoverage(Justification = "Writes uniforms to a live OpenGL program.")]
public abstract class RenderProgram3D<T> : RenderProgram<T>, IRenderProgram3D where T : IVertex
{
    private readonly int matView;
    private readonly int matProjection;

    /// <summary>Creates a 3D program from vertex and fragment shader source.</summary>
    protected RenderProgram3D(GlLayer gl, string vertCode, string fragCode) : base(gl, vertCode, fragCode)
    {
        matView = gl.GetUniformLocation(Id, nameof(matView));
        matProjection = gl.GetUniformLocation(Id, nameof(matProjection));
    }

    /// <inheritdoc />
    public Mat4 View { set => SetMatrix(matView, value); }

    /// <inheritdoc />
    public Mat4 Projection { set => SetMatrix(matProjection, value); }

    private void SetMatrix(int location, Mat4 value)
    {
        Span<float> values = stackalloc float[16];
        value.CopyToColumnMajor(values);
        gl.ProgramUniformMatrix4fv(Id, location, false, values);
    }
}
