namespace AlvorKit.Script.LiveCode;

/// <summary>Uncommitted compiler generation retained until the target acknowledges its exact delta.</summary>
internal sealed record SourceUpdateDeltaProposal(
    int PreviousGeneration,
    string SourcePath,
    string ResultSource,
    Solution Solution,
    Compilation Compilation,
    EmitBaseline Baseline,
    SourceUpdateApplyRequest Request,
    string DiffSha256,
    string[] Diagnostics);
