namespace AlvorKit.Script.AlvorEye;

/// <summary>Identifies the top-level AlvorEye CLI command.</summary>
internal enum AlvorEyeCommandKind
{
    /// <summary>Print usage text.</summary>
    Help,

    /// <summary>Execute a complete scenario file.</summary>
    Run,

    /// <summary>Launch or attach and then accept JSONL actions from standard input.</summary>
    Session,

    /// <summary>Freeze an existing session target and capture its current frame.</summary>
    Handoff,

    /// <summary>Resume an existing frozen session target.</summary>
    Resume,
}
