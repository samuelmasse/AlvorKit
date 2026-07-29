namespace AlvorKit.Script.LiveCode;

/// <summary>Applies one strict single-file unified diff to an acknowledged in-memory source snapshot.</summary>
internal static class SourceUpdateDiff
{
    private static readonly Regex HunkHeader = new(
        "^@@ -(\\d+)(?:,(\\d+))? \\+(\\d+)(?:,(\\d+))? @@",
        RegexOptions.CultureInvariant);

    internal static SourceUpdateDiffResult Apply(
        string previousSource,
        string diff,
        string expectedSourcePath,
        string repositoryRoot)
    {
        var diffLines = Split(diff);
        if (diffLines.Count < 3 ||
            !diffLines[0].StartsWith("--- ", StringComparison.Ordinal) ||
            !diffLines[1].StartsWith("+++ ", StringComparison.Ordinal))
        {
            throw new InvalidDataException("Source Update diff must start with one ---/+++ file header.");
        }
        if (diffLines.Skip(2).Any(static line =>
            line.StartsWith("--- ", StringComparison.Ordinal) ||
            line.StartsWith("+++ ", StringComparison.Ordinal)))
        {
            throw new InvalidDataException("Source Update diff must contain exactly one file.");
        }

        var oldPath = HeaderPath(diffLines[0][4..]);
        var newPath = HeaderPath(diffLines[1][4..]);
        ValidatePath(oldPath, expectedSourcePath, repositoryRoot);
        ValidatePath(newPath, expectedSourcePath, repositoryRoot);

        var sourceLines = Split(previousSource);
        var newline = previousSource.Contains("\r\n", StringComparison.Ordinal)
            ? "\r\n"
            : "\n";
        var finalNewline = previousSource.EndsWith('\n');
        var output = new List<string>(sourceLines.Count);
        var sourceIndex = 0;
        var index = 2;
        while (index < diffLines.Count)
        {
            if (diffLines[index].Length == 0)
            {
                index++;
                continue;
            }
            var match = HunkHeader.Match(diffLines[index]);
            if (!match.Success)
                throw new InvalidDataException($"Malformed unified diff hunk header: {diffLines[index]}");

            var oldStart = Parse(match.Groups[1].Value);
            while (sourceIndex < oldStart - 1)
                output.Add(sourceLines[sourceIndex++]);
            index++;
            while (index < diffLines.Count && !diffLines[index].StartsWith("@@ ", StringComparison.Ordinal))
            {
                var line = diffLines[index];
                if (line == "\\ No newline at end of file")
                {
                    finalNewline = false;
                    index++;
                    continue;
                }
                if (line.Length == 0)
                    throw new InvalidDataException("Unified diff body lines must have a prefix.");

                var content = line[1..];
                switch (line[0])
                {
                    case ' ':
                        RequireSourceLine(sourceLines, sourceIndex, content);
                        output.Add(sourceLines[sourceIndex++]);
                        break;
                    case '-':
                        RequireSourceLine(sourceLines, sourceIndex, content);
                        sourceIndex++;
                        break;
                    case '+':
                        output.Add(content);
                        break;
                    default:
                        throw new InvalidDataException($"Unsupported unified diff line prefix '{line[0]}'.");
                }
                index++;
            }
        }

        while (sourceIndex < sourceLines.Count)
            output.Add(sourceLines[sourceIndex++]);
        var result = string.Join(newline, output);
        if (finalNewline)
            result += newline;
        return new(
            oldPath,
            newPath,
            result,
            Hash(diff),
            Hash(previousSource),
            Hash(result));
    }

    private static List<string> Split(string value)
    {
        var normalized = value.Replace("\r\n", "\n", StringComparison.Ordinal);
        var lines = normalized.Split('\n').ToList();
        if (lines.Count > 0 && lines[^1].Length == 0)
            lines.RemoveAt(lines.Count - 1);
        return lines;
    }

    private static string HeaderPath(string header)
    {
        var path = header.Split('\t', 2)[0].Trim();
        if (path.StartsWith("a/", StringComparison.Ordinal) ||
            path.StartsWith("b/", StringComparison.Ordinal))
        {
            path = path[2..];
        }
        return path.Replace('/', Path.DirectorySeparatorChar);
    }

    private static void ValidatePath(
        string diffPath,
        string expectedSourcePath,
        string repositoryRoot)
    {
        if (Path.IsPathRooted(diffPath) ||
            diffPath.Split(Path.DirectorySeparatorChar).Contains("..", StringComparer.Ordinal))
        {
            throw new InvalidDataException("Source Update diff paths must stay inside the repository.");
        }
        var resolved = Path.GetFullPath(diffPath, repositoryRoot);
        if (!string.Equals(resolved, Path.GetFullPath(expectedSourcePath), StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Source Update diff targets '{diffPath}', not the selected source file.");
    }

    private static void RequireSourceLine(
        IReadOnlyList<string> source,
        int index,
        string expected)
    {
        if (index >= source.Count || source[index] != expected)
            throw new InvalidDataException($"Unified diff context does not match source line {index + 1}.");
    }

    private static int Parse(string value) =>
        int.Parse(value, NumberStyles.None, CultureInfo.InvariantCulture);

    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}
