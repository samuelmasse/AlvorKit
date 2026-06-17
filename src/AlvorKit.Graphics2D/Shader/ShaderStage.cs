namespace AlvorKit.Graphics2D;

/// <summary>Owns a compiled OpenGL shader stage.</summary>
public class ShaderStage : IDisposable
{
    /// <summary>The strict OpenGL command surface that owns the shader.</summary>
    private readonly GlLayer gl;

    /// <summary>The tracked shader handle.</summary>
    private readonly GlShaderHandle id;

    /// <summary>Gets the tracked shader handle.</summary>
    public GlShaderHandle Id => id;

    /// <summary>Compiles one shader stage and throws with the driver log if compilation fails.</summary>
    public ShaderStage(GlLayer gl, string? label, string code, GlShaderType type)
    {
        this.gl = gl;
        var shader = gl.CreateShader(type);
        gl.ShaderSource(shader, code);
        gl.CompileShader(shader);
        gl.GetShaderiv(shader, GlShaderParameterName.CompileStatus, out var compiled);

        if (compiled == 0)
        {
            var info = gl.GetShaderInfoLog(shader);
            gl.DeleteShader(shader);
            throw new InvalidOperationException($"Failed to compile shader {label} {type}:\n{info}");
        }

        id = shader;
    }

    /// <summary>Compiles an unlabeled shader stage and throws with the driver log if compilation fails.</summary>
    public ShaderStage(GlLayer gl, string code, GlShaderType type) : this(gl, null, code, type) { }

    /// <summary>Deletes the tracked shader handle.</summary>
    public void Dispose() => gl.DeleteShader(id);
}
