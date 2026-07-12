namespace AlvorKit.Windowing;

/// <summary>Names one native file-dialog filter and its comma-separated extensions without dots.</summary>
/// <param name="Name">Friendly filter name shown by platforms that support it.</param>
/// <param name="Extensions">Comma-separated extensions such as <c>png,jpg,jpeg</c>.</param>
public readonly record struct FileDialogFilter(string Name, string Extensions);
