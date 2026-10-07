namespace AlvorKit;

/// <summary>Console width, wrapping, and color operations at the terminal boundary.</summary>
[ExcludeFromCodeCoverage(Justification = "Console width and color depend on the attached terminal.")]
internal class BenchTerminal
{
    /// <summary>Maximum human-readable console report width.</summary>
    private const int MaximumLineWidth = 120;

    /// <summary>Wraps descriptions at the available width and aligns continuation lines.</summary>
    internal void WriteWrapped(string text, int continuationColumn)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var column = continuationColumn;
        var lineWidth = AvailableLineWidth();

        for (var index = 0; index < words.Length; index++)
        {
            var separatorWidth = index == 0 ? 0 : 1;

            if (index != 0 && column + separatorWidth + words[index].Length > lineWidth)
            {
                Console.WriteLine();
                Console.Write(new string(' ', continuationColumn));
                column = continuationColumn;
                separatorWidth = 0;
            }

            if (separatorWidth != 0)
            {
                Console.Write(' ');
                column++;
            }

            Console.Write(words[index]);
            column += words[index].Length;
        }

        Console.WriteLine();
    }

    /// <summary>Uses terminal width for interactive output and the report width for redirection.</summary>
    private int AvailableLineWidth()
    {
        if (Console.IsOutputRedirected)
            return MaximumLineWidth;

        try
        {
            var consoleWidth = Console.WindowWidth;
            return consoleWidth > 0 ? Math.Min(consoleWidth, MaximumLineWidth) : MaximumLineWidth;
        }
        catch (IOException)
        {
            return MaximumLineWidth;
        }
    }

    /// <summary>Colors interactive hierarchy labels and leaves redirected output uncolored.</summary>
    internal void WriteLabel(string label, int depth)
    {
        if (Console.IsOutputRedirected)
        {
            Console.Write(label);
            return;
        }

        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = (depth % 4) switch
        {
            0 => ConsoleColor.Cyan,
            1 => ConsoleColor.Yellow,
            2 => ConsoleColor.Green,
            _ => ConsoleColor.Magenta,
        };
        Console.Write(label);
        Console.ForegroundColor = previousColor;
    }

}
