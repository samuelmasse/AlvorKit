namespace AlvorKit.Script.Bindgen;

/// <summary>OpenGL registry and documentation configuration for one native library binding.</summary>
public sealed partial class BindgenConfig
{
    /// <summary>OpenGL registry API name used for feature selection.</summary>
    public string GlApi { get; set; } = "gl";

    /// <summary>OpenGL registry profile used for feature selection.</summary>
    public string GlProfile { get; set; } = "core";

    /// <summary>OpenGL registry version used for feature selection.</summary>
    public string? GlVersion { get; set; }

    /// <summary>OpenGL registry extensions explicitly included after core feature selection.</summary>
    public string[] GlExtensions { get; set; } = [];

    /// <summary>Optional OpenGL ES registry API used for availability annotations.</summary>
    public string GlEsApi { get; set; } = "gles2";

    /// <summary>Optional Khronos reference-page archive URL used for generated documentation.</summary>
    public string? DocUrl { get; set; }

    /// <summary>Optional Khronos reference-page tag or commit used for documentation URL replacement.</summary>
    public string? DocTag { get; set; }

    /// <summary>Documentation archive extraction directory under <see cref="WorkDir"/>.</summary>
    public string DocDir { get; set; } = "";

    /// <summary>Documentation subdirectory parsed inside <see cref="DocDir"/>.</summary>
    public string DocSubdir { get; set; } = "";
}
