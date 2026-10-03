namespace AlvorKit;

[Components]
internal interface IUiNodesComponents
{
    [ComponentToString] internal long UiId { get; set; }
    internal RootUi UiRoot { get; set; }
    internal long UiToken { get; set; }
    internal NodeArray UiNodes { get; set; }
    internal NodeArray UiNodeStack { get; set; }
    internal EntMut UiParent { get; set; }
    internal EntMut UiStackEntry { get; set; }
    internal bool UiRefreshActive { get; set; }
    internal double UiRefreshTime { get; set; }
}
