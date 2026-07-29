namespace AlvorKit.Script.LiveCode;

/// <summary>Validated existing definitions changed by one emitted delta.</summary>
internal sealed record SourceUpdateDeltaTokens(
    int MethodToken,
    int[] ChangedTypeTokens);
