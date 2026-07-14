namespace AlvorKit.Engine.Test;

/// <summary>Records one engine-facing file-dialog request.</summary>
internal sealed class EngineTestFileDialogHost : IFileDialogHost
{
    internal FileDialogFilter[] Filters { get; private set; } = [];

    internal string? DefaultPath { get; private set; }

    public string? OpenFile(ReadOnlySpan<FileDialogFilter> filters, string? defaultPath = null)
    {
        Filters = filters.ToArray();
        DefaultPath = defaultPath;
        return null;
    }

    public string[]? OpenFiles(ReadOnlySpan<FileDialogFilter> filters, string? defaultPath = null) =>
        throw new NotSupportedException();

    public string? SaveFile(ReadOnlySpan<FileDialogFilter> filters, string? defaultPath = null, string? defaultName = null) =>
        throw new NotSupportedException();

    public string? PickFolder(string? defaultPath = null) => throw new NotSupportedException();

    public string[]? PickFolders(string? defaultPath = null) => throw new NotSupportedException();
}
