namespace AlvorKit.OpenGL.Layer;

/// <summary>
/// A debug-time <see cref="Gl"/> wrapper that tracks resources and rejects misuse before forwarding
/// to the inner backend. Both this and a bare backend are a <see cref="Gl"/>, so switching between
/// them is a one-line change:
/// <code>
/// Gl gl = new GlValidationLayer(new GlBackend(loader)); // debug: validated
/// Gl gl = new GlBackend(loader);                        // release: bare, no overhead
/// </code>
/// Override only the calls you need to gate; everything else passes through <see cref="GlWrapper"/>.
/// </summary>
public class GlValidationLayer(Gl inner) : GlWrapper(inner)
{
    private readonly HashSet<uint> shaders = [];
    private readonly HashSet<uint> programs = [];

    // --- Resource tracking: needs the value GL returns, which only a wrapper can see. ---

    /// <inheritdoc/>
    public override uint CreateShader(ShaderType type)
    {
        var shader = base.CreateShader(type);
        shaders.Add(shader);
        return shader;
    }

    /// <inheritdoc/>
    public override uint CreateProgram()
    {
        var program = base.CreateProgram();
        programs.Add(program);
        return program;
    }

    // --- Rejecting misuse: validate first, then forward (so the bad call never reaches the driver). ---

    /// <inheritdoc/>
    public override void DeleteShader(uint shader)
    {
        if (shader != 0 && !shaders.Remove(shader))
            throw new GlValidationException($"DeleteShader: {shader} is not a live shader (double delete, or never created).");
        base.DeleteShader(shader);
    }

    /// <inheritdoc/>
    public override void AttachShader(uint program, uint shader)
    {
        if (!programs.Contains(program))
            throw new GlValidationException($"AttachShader: {program} is not a live program.");
        if (!shaders.Contains(shader))
            throw new GlValidationException($"AttachShader: {shader} is not a live shader.");
        base.AttachShader(program, shader);
    }

    // Port the rest of the GlwLayer validation here: bind/state tracking with conflict detection,
    // resource labels, leak checks. Override the raw virtuals you need; the convenience overloads
    // (GenBuffer(), BufferData<T>(span), ...) route through them automatically since they are
    // extension methods over the same virtuals.
}
