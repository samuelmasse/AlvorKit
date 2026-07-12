namespace AlvorKit.Windowing;

/// <summary>Exposes native file dialogs without platform handles or binding types.</summary>
public class FileDialogs(IFileDialogHost host)
{
    /// <summary>Opens one existing file, returning <see langword="null"/> when cancelled.</summary>
    public string? OpenFile(ReadOnlySpan<FileDialogFilter> filters, string? defaultPath = null) => host.OpenFile(filters, defaultPath);

    /// <summary>Opens existing files, returning <see langword="null"/> when cancelled.</summary>
    public string[]? OpenFiles(ReadOnlySpan<FileDialogFilter> filters, string? defaultPath = null) => host.OpenFiles(filters, defaultPath);

    /// <summary>Selects a save path, returning <see langword="null"/> when cancelled.</summary>
    public string? SaveFile(ReadOnlySpan<FileDialogFilter> filters, string? defaultPath = null, string? defaultName = null) =>
        host.SaveFile(filters, defaultPath, defaultName);

    /// <summary>Selects one folder, returning <see langword="null"/> when cancelled.</summary>
    public string? PickFolder(string? defaultPath = null) => host.PickFolder(defaultPath);

    /// <summary>Selects folders, returning <see langword="null"/> when cancelled.</summary>
    public string[]? PickFolders(string? defaultPath = null) => host.PickFolders(defaultPath);
}
