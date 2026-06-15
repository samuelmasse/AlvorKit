namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnuseProgram"/>.</remarks>
    public override void UseProgram(GlProgramHandle program)
    {
        this.program.Bind(nameof(UseProgram), (uint)program);
        base.UseProgram(program);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindProgramPipeline"/>.</remarks>
    public override void BindProgramPipeline(GlProgramPipelineHandle pipeline)
    {
        programPipeline.Bind(nameof(BindProgramPipeline), (uint)pipeline);
        base.BindProgramPipeline(pipeline);
    }

    /// <summary>Layer: Stops using the current program. Must be paired with exactly one earlier call to <c>glUseProgram</c>.</summary>
    public void UnuseProgram() { program.Unbind(nameof(UseProgram)); base.UseProgram((GlProgramHandle)0u); }

    /// <summary>Layer: Unbinds the program pipeline. Must be paired with exactly one earlier call to <c>glBindProgramPipeline</c>.</summary>
    public void UnbindProgramPipeline() { programPipeline.Unbind(nameof(BindProgramPipeline)); base.BindProgramPipeline((GlProgramPipelineHandle)0u); }
}
