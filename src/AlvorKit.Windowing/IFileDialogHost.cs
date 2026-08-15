namespace AlvorKit;

/// <summary>Platform file-dialog operations associated with a native application window.</summary>
public interface IFileDialogHost
{
    /// <summary>Opens one existing file, returning <see langword="null"/> when canceled.</summary>
    string? OpenFile(ReadOnlySpan<FileDialogFilter> filters, string? defaultPath = null);

    /// <summary>Opens existing files, returning <see langword="null"/> when canceled.</summary>
    string[]? OpenFiles(ReadOnlySpan<FileDialogFilter> filters, string? defaultPath = null);

    /// <summary>Selects a save path, returning <see langword="null"/> when canceled.</summary>
    string? SaveFile(ReadOnlySpan<FileDialogFilter> filters, string? defaultPath = null, string? defaultName = null);

    /// <summary>Selects one folder, returning <see langword="null"/> when canceled.</summary>
    string? PickFolder(string? defaultPath = null);

    /// <summary>Selects folders, returning <see langword="null"/> when canceled.</summary>
    string[]? PickFolders(string? defaultPath = null);
}
