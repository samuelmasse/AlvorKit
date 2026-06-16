namespace AlvorKit.Script.AlvorEye.Test;

/// <summary>Writes tiny generated PNG fixtures without using platform image APIs.</summary>
internal static class TestPng
{
    /// <summary>Solid red 3x3 PNG.</summary>
    private const string Red = "iVBORw0KGgoAAAANSUhEUgAAAAMAAAADCAIAAADZSiLoAAAAEElEQVR4nGP4z8AAQQxYWACPjgj4kWPEuQAAAABJRU5ErkJggg==";

    /// <summary>Solid black 3x3 PNG.</summary>
    private const string Black = "iVBORw0KGgoAAAANSUhEUgAAAAMAAAADCAIAAADZSiLoAAAAC0lEQVR4nGNgwAcAAB4AAfb96ZYAAAAASUVORK5CYII=";

    /// <summary>Writes the red fixture to disk.</summary>
    public static void WriteRed(string path) => Write(path, Red);

    /// <summary>Writes the black fixture to disk.</summary>
    public static void WriteBlack(string path) => Write(path, Black);

    /// <summary>Writes one base64-encoded PNG to disk.</summary>
    private static void Write(string path, string base64)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, Convert.FromBase64String(base64));
    }
}
