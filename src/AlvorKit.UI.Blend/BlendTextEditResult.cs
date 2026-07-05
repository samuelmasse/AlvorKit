namespace AlvorKit.UI.Blend;

/// <summary>Outcome of one text-edit input pass.</summary>
public enum BlendTextEditResult
{
    /// <summary>Editing continues.</summary>
    None,

    /// <summary>The user accepted the text with Enter or Tab.</summary>
    Commit,

    /// <summary>The user rejected the edit with Escape; the pre-edit value should be kept.</summary>
    Cancel,
}
