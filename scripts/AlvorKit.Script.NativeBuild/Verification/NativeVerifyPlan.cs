namespace AlvorKit.Script.NativeBuild;

/// <summary>Resolved paths and process commands for one native verification run.</summary>
/// <param name="LibraryPath">Native runtime library loaded by the verifier.</param>
/// <param name="SourcePath">C source file for the verifier program.</param>
/// <param name="ArtifactDirectory">Directory that receives the verifier executable and report.</param>
/// <param name="ExecutablePath">Verifier executable path.</param>
/// <param name="ReportPath">Report file path written by the verifier.</param>
/// <param name="CompileCommand">Command that compiles the verifier program for the target RID.</param>
/// <param name="RunCommand">Command that runs the verifier against the runtime library.</param>
internal sealed record NativeVerifyPlan(
    string LibraryPath,
    string SourcePath,
    string ArtifactDirectory,
    string ExecutablePath,
    string ReportPath,
    CommandSpec CompileCommand,
    CommandSpec RunCommand);
