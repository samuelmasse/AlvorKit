namespace AlvorKit.Script.NativeBuild;

/// <summary>One text replacement applied to extracted upstream source before native build commands run.</summary>
internal sealed class SourcePatchConfig
{
    /// <summary>Path to the source file relative to the extracted source directory.</summary>
    public required string Path { get; init; }

    /// <summary>Text expected in the source file before the patch is applied.</summary>
    public required string Search { get; init; }

    /// <summary>Replacement text written to the source file.</summary>
    public required string Replace { get; init; }
}
