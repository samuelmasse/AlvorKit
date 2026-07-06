namespace AlvorKit.Script.ZoneSolution;

/// <summary>Path text helpers for stable generated .slnx output.</summary>
internal static class PathText
{
    /// <summary>Returns true when two paths resolve to the same full path.</summary>
    public static bool SamePath(string left, string right) =>
        string.Equals(Path.GetFullPath(left), Path.GetFullPath(right), StringComparison.OrdinalIgnoreCase);

    /// <summary>Converts an operating-system path to the slash style used by .slnx files.</summary>
    public static string ToSlnxPath(string path) =>
        path.Replace('\\', '/');
}
