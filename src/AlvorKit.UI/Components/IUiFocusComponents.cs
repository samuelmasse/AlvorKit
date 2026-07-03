namespace AlvorKit.UI;

[Components(SkipBuilder = true)]
public interface IUiFocusComponents
{
    /// <summary>Whether input is disabled for this node.</summary>
    UiProp<bool> IsInputDisabledFV { get; set; }
    /// <summary>Whether this node can receive focus.</summary>
    UiProp<bool> IsFocusableFV { get; set; }
    /// <summary>Whether this node can receive focus via mouse but not keyboard Tab.</summary>
    UiProp<bool> IsSilentFocusableFV { get; set; }
    /// <summary>Whether this node should receive initial focus.</summary>
    UiProp<bool> IsInitialFocusFV { get; set; }
    /// <summary>Node subtree to preserve focus within when this node is focused.</summary>
    UiProp<EntMut> DeferFocusFV { get; set; }
    /// <summary>Focus group this node belongs to.</summary>
    UiProp<EntMut> FocusGroupFV { get; set; }
    /// <summary>Callback invoked when this node receives focus.</summary>
    UiCallback<Action?> OnFocusFV { get; set; }
    /// <summary>Callback invoked when this node is unselected.</summary>
    UiCallback<Action?> OnUnselectFV { get; set; }

    /// <summary>Whether this node currently has focus.</summary>
    bool IsFocusedR { get; internal set; }
    /// <summary>Whether this node is the selected node within its focus group.</summary>
    bool IsSelectedR { get; internal set; }
    /// <summary>The currently selected child node within this focus group.</summary>
    EntMut SelectedR { get; internal set; }
}
