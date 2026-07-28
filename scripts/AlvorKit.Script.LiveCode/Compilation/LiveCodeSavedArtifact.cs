namespace AlvorKit.Script.LiveCode;

/// <summary>Caller-side location and shape of one artifact returned by a LiveCode bridge.</summary>
internal sealed record LiveCodeSavedArtifact(
    string Name,
    string Path,
    string ContentType,
    int Bytes);
