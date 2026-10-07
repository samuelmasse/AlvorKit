namespace AlvorKit;

/// <summary>Prints captured allocation trees without retaining live allocation owners.</summary>
internal class BenchMemoryConsole
{
    /// <summary>Shared numeric column width for the allocation tree.</summary>
    private const int MemoryValueWidth = 12;

    /// <summary>Prints payload and overhead alongside usable bytes for each tree node.</summary>
    public void Write(BenchMemoryUsage memory, int depth, bool mean)
    {
        var heading = mean ? "mean memory" : "memory";
        var nameWidth = Math.Max(heading.Length, MemoryNameWidth(memory, 0));

        Console.Write(BenchFormatting.Indent(depth));
        Console.Write(heading.PadRight(nameWidth));
        Console.Write("  ");
        Console.Write("self".PadLeft(MemoryValueWidth));
        Console.Write("  ");
        Console.Write("total".PadLeft(MemoryValueWidth));
        Console.Write("  ");
        Console.Write("payload".PadLeft(MemoryValueWidth));
        Console.Write("  ");
        Console.WriteLine("overhead".PadLeft(MemoryValueWidth));
        WriteMemoryNode(memory, depth, nameWidth, "", "");
    }

    /// <summary>Prints one node and preserves sibling order during recursive rendering.</summary>
    private void WriteMemoryNode(
        BenchMemoryUsage memory,
        int depth,
        int nameWidth,
        string displayPrefix,
        string childPrefix)
    {
        Console.Write(BenchFormatting.Indent(depth));
        Console.Write(displayPrefix);
        Console.Write(memory.Name.PadRight(nameWidth - displayPrefix.Length));
        Console.Write("  ");
        Console.Write(BenchFormatting.FormatBytes(memory.SelfBytes).PadLeft(MemoryValueWidth));
        Console.Write("  ");
        Console.Write(BenchFormatting.FormatBytes(memory.TotalBytes).PadLeft(MemoryValueWidth));
        Console.Write("  ");
        Console.Write(BenchFormatting.FormatBytes(memory.TotalRequestedBytes).PadLeft(MemoryValueWidth));
        Console.Write("  ");
        Console.WriteLine(BenchFormatting.FormatBytes(memory.TotalOverheadBytes).PadLeft(MemoryValueWidth));

        for (var index = 0; index < memory.Children.Length; index++)
        {
            var last = index == memory.Children.Length - 1;
            var branch = last ? "└─ " : "├─ ";
            var continuation = last ? "   " : "│  ";
            WriteMemoryNode(
                memory.Children[index],
                depth,
                nameWidth,
                childPrefix + branch,
                childPrefix + continuation);
        }
    }

    /// <summary>Measures the deepest labeled branch before aligning numeric columns.</summary>
    private static int MemoryNameWidth(BenchMemoryUsage memory, int prefixLength)
    {
        var width = prefixLength + memory.Name.Length;

        foreach (var child in memory.Children)
            width = Math.Max(width, MemoryNameWidth(child, prefixLength + 3));

        return width;
    }
}
