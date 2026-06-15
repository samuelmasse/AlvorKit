namespace AlvorKit.Script.Bindgen;

/// <summary>Maps OpenGL registry object classes to generated strongly typed handle structs.</summary>
internal static class GlRegistryHandleTypes
{
    /// <summary>Registry object class to generated handle type map.</summary>
    private static readonly IReadOnlyDictionary<string, string> Map = new Dictionary<string, string>
    {
        ["buffer"] = "GlBufferHandle",
        ["texture"] = "GlTextureHandle",
        ["program"] = "GlProgramHandle",
        ["shader"] = "GlShaderHandle",
        ["framebuffer"] = "GlFramebufferHandle",
        ["renderbuffer"] = "GlRenderbufferHandle",
        ["sampler"] = "GlSamplerHandle",
        ["vertex array"] = "GlVertexArrayHandle",
        ["query"] = "GlQueryHandle",
        ["transform feedback"] = "GlTransformFeedbackHandle",
        ["program pipeline"] = "GlProgramPipelineHandle",
    };

    /// <summary>Returns and records the configured strongly typed handle for a native object class.</summary>
    public static string? Resolve(string? handleClass, ISet<string> handleTypes)
    {
        if (handleClass is null || !Map.TryGetValue(handleClass, out var type))
            return null;
        handleTypes.Add(type);
        return type;
    }
}
