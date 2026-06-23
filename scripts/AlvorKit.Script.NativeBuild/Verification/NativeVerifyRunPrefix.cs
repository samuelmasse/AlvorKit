namespace AlvorKit.Script.NativeBuild;

/// <summary>Optional executable prefix used when the verifier needs an emulator or launcher.</summary>
/// <param name="FileName">Launcher executable name or path.</param>
/// <param name="Arguments">Arguments passed to the launcher before the verifier executable path.</param>
internal sealed record NativeVerifyRunPrefix(string FileName, IReadOnlyList<string> Arguments);
