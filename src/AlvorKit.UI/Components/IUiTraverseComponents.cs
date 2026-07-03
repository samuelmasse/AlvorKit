namespace AlvorKit.UI;

[Components(SkipBuilder = true)]
public interface IUiTraverseComponents
{
    /// <summary>Whether this node is marked for deletion.</summary>
    UiProp<bool> IsDeletedFV { get; set; }
    /// <summary>Whether this node is disabled and excluded from traversal.</summary>
    UiProp<bool> IsDisabledFV { get; set; }
    /// <summary>Whether child nodes should be sorted by their order value.</summary>
    UiProp<bool> IsOrderedFV { get; set; }
    /// <summary>Sort priority when the parent has ordering enabled.</summary>
    UiProp<float> OrderValueFV { get; set; }
    /// <summary>Number of additional UI tree traversals to perform when this node is first added.</summary>
    UiValue<int> RenderDelayFV { get; set; }

    /// <summary>Companion node that renders behind this menu while it's on the stack.</summary>
    UiValue<EntMut> CompanionFV { get; set; }

    /// <summary>Compiled list of active child nodes after traversal.</summary>
    Memory<EntMut> NodesR { get; internal set; }
    /// <summary>Resolved alignment snap for this node.</summary>
    float SnapR { get; internal set; }
}
