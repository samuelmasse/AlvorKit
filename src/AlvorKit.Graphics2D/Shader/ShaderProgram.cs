namespace AlvorKit.Graphics2D;

/// <summary>Owns a linked OpenGL shader program.</summary>
public class ShaderProgram : IDisposable
{
    /// <summary>The strict OpenGL command surface that owns the program.</summary>
    private readonly GlLayer gl;

    /// <summary>The tracked program handle.</summary>
    private readonly GlProgramHandle id;

    /// <summary>Gets the tracked program handle.</summary>
    public GlProgramHandle Id => id;

    /// <summary>Links the supplied compiled stages into a program and throws with the driver log on failure.</summary>
    public ShaderProgram(GlLayer gl, string? label, params ReadOnlySpan<ShaderStage> stages)
    {
        this.gl = gl;
        var program = gl.CreateProgram();

        foreach (var stage in stages)
            gl.AttachShader(program, stage.Id);

        gl.LinkProgram(program);
        gl.GetProgramiv(program, GlProgramProperty.LinkStatus, out var linked);

        if (linked == 0)
        {
            var info = gl.GetProgramInfoLog(program);
            gl.DeleteProgram(program);
            throw new InvalidOperationException($"Failed to link program {label}:\n{info}");
        }

        id = program;
    }

    /// <summary>Links the supplied compiled stages into an unlabeled program.</summary>
    public ShaderProgram(GlLayer gl, ReadOnlySpan<ShaderStage> stages) : this(gl, null, stages) { }

    /// <summary>Deletes the tracked program handle.</summary>
    public void Dispose() => gl.DeleteProgram(id);
}
