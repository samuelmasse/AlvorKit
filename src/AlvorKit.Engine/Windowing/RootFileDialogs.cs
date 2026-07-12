namespace AlvorKit.Engine;

/// <summary>Root-scoped native file dialogs for application and menu composition.</summary>
[Root]
public sealed class RootFileDialogs(IFileDialogHost host) : FileDialogs(host);
