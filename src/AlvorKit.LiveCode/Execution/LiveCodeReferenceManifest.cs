namespace AlvorKit.LiveCode;

/// <summary>Compilation references and imports reported by a running LiveCode target.</summary>
public sealed record LiveCodeReferenceManifest(
    string[] AssemblyPaths,
    string[] GlobalUsings);
