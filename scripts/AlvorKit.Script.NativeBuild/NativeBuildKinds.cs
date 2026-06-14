namespace AlvorKit.Script.NativeBuild;

/// <summary>Known native build strategies used by manifests.</summary>
internal static class NativeBuildKinds
{
    /// <summary>Builds one local C translation unit directly with the platform compiler.</summary>
    public const string SingleC = "single-c";

    /// <summary>Configures and builds an upstream CMake project.</summary>
    public const string CMake = "cmake";
}
