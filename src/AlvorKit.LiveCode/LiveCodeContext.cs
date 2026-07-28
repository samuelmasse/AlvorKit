namespace AlvorKit.LiveCode;

/// <summary>Collects stable text output from one live command without redirecting process-wide console streams.</summary>
public sealed class LiveCodeContext
{
    private readonly List<string> lines = [];
    private readonly Dictionary<string, string> values = [];

    /// <summary>Appends one explanatory output line.</summary>
    public void WriteLine(string line) => lines.Add(line);

    /// <summary>Records or replaces one named invariant-culture value.</summary>
    public void Value(string name, object? value)
    {
        values[name] = value switch
        {
            null => "null",
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
    }

    /// <summary>Returns the accumulated lines as an execution-owned array.</summary>
    internal string[] Lines() => [.. lines];

    /// <summary>Returns the accumulated values as an execution-owned dictionary.</summary>
    internal Dictionary<string, string> Values() => new(values);
}
