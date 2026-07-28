namespace AlvorKit.LivePatch;

/// <summary>Receiver ownership rule for one managed live behavior.</summary>
public enum LivePatchSelectorKind
{
    /// <summary>Only one exact reference instance matches.</summary>
    ExactInstance,

    /// <summary>Instances owned by one exact active injector scope match.</summary>
    ExactScope,

    /// <summary>Instances owned by one scope or any of its active descendants match.</summary>
    ScopeAndDescendants,

    /// <summary>Every receiver matches; static methods require this selector.</summary>
    All
}
