namespace AlvorKit.UI.Root;

[Components]
internal interface IUiNodesComponents
{
    [ComponentToString] internal long UiId { get; set; }
    internal RootUi UiRoot { get; set; }
    internal long UiToken { get; set; }
    internal NodeArray UiNodes { get; set; }
    internal NodeArray UiNodeStack { get; set; }
}
