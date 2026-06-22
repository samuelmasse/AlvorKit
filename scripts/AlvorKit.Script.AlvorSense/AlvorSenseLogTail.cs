namespace AlvorKit.Script.AlvorSense;

/// <summary>Reads compact tails from session log files for foreground diagnostics.</summary>
internal static class AlvorSenseLogTail
{
    /// <summary>Returns the last requested lines from a UTF-8 log file.</summary>
    /// <param name="path">Log path to read.</param>
    /// <param name="count">Maximum number of lines to return.</param>
    internal static string[] Read(string path, int count)
    {
        if (count <= 0 || !File.Exists(path))
            return [];

        Queue<string> lines = new(count);
        foreach (var line in File.ReadLines(path, Encoding.UTF8))
        {
            if (lines.Count == count)
                lines.Dequeue();
            lines.Enqueue(line);
        }
        return [.. lines];
    }
}
