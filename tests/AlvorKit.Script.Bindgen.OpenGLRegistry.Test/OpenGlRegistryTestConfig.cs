namespace AlvorKit.Script.Bindgen.OpenGLRegistry.Test;

/// <summary>Factory for OpenGL registry bindgen configs used by tests.</summary>
internal static class OpenGlRegistryTestConfig
{
    /// <summary>Creates a minimal OpenGL registry bindgen config for tests.</summary>
    public static BindgenConfig Create(string glVersion = "1.0") => new()
    {
        Kind = BindgenConfig.GlRegistryKind,
        Namespace = "AlvorKit.Bindgen.OpenGLFixture",
        ApiClass = "Gl",
        ApiSummary = "Fixture GL API.",
        BackendClass = "GlBackend",
        Prefix = "GL_",
        WorkDir = "fixture-work",
        SourceDir = "fixture-source",
        Header = "gl.xml",
        ApiProject = "generated/Gl",
        BackendProject = "generated/Gl.Backend",
        GlVersion = glVersion
    };
}
