namespace AlvorKit.Script.NativeBuild;

/// <summary>Command verbs accepted by the native build CLI.</summary>
internal enum CliCommand
{
    /// <summary>Lists libraries with native build manifests.</summary>
    List,

    /// <summary>Prints the native package version for one library.</summary>
    Version,

    /// <summary>Builds one or more native runtime binaries.</summary>
    Build,

    /// <summary>Verifies one native runtime binary with a native smoke test.</summary>
    Verify
}
