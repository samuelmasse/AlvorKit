namespace AlvorKit.UI;

[Components(SkipBuilder = true)]
public interface IUiMouseComponents
{
    /// <summary>Whether this node can be hovered by the mouse.</summary>
    UiProp<bool> IsSelectableFV { get; set; }
    /// <summary>Whether this node can receive scroll events.</summary>
    UiProp<bool> IsScrollableFV { get; set; }
    /// <summary>Mouse cursor to display when hovering this node.</summary>
    UiProp<CursorShape?> CursorFV { get; set; }
    /// <summary>Callback invoked on left click.</summary>
    UiCallback<Action?> OnClickFV { get; set; }
    /// <summary>Callback invoked on double left click.</summary>
    UiCallback<Action?> OnDoubleClickFV { get; set; }
    /// <summary>Callback invoked on left mouse press.</summary>
    UiCallback<Action?> OnPressFV { get; set; }
    /// <summary>Callback invoked on right click.</summary>
    UiCallback<Action?> OnSecondaryClickFV { get; set; }
    /// <summary>Callback invoked on right mouse press.</summary>
    UiCallback<Action?> OnSecondaryPressFV { get; set; }
    /// <summary>Callback invoked on scroll with the scroll delta.</summary>
    UiCallback<Action<Vec2>?> OnScrollFV { get; set; }

    /// <summary>Whether this node is currently hovered.</summary>
    bool IsHoveredR { get; internal set; }
    /// <summary>Whether this node is currently pressed with the left mouse button.</summary>
    bool IsPressedR { get; internal set; }
    /// <summary>Whether this node is currently pressed with the right mouse button.</summary>
    bool IsSecondaryPressedR { get; internal set; }
}
