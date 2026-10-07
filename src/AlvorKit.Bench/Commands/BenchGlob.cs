namespace AlvorKit;

/// <summary>Matches case-insensitive benchmark paths with segment-aware wildcards.</summary>
public class BenchGlob(string pattern)
{
    /// <summary>Compiled matcher with bounded nonbacktracking behavior.</summary>
    private readonly Regex regex = new(
        CreateExpression(pattern),
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.NonBacktracking);

    /// <summary>Tests the whole path, ignoring case.</summary>
    public bool Matches(string value) => regex.IsMatch(value);

    /// <summary>Escapes literal characters and translates only supported path wildcards.</summary>
    private static string CreateExpression(string pattern)
    {
        StringBuilder expression = new("^");

        for (var index = 0; index < pattern.Length; index++)
        {
            var character = pattern[index];

            if (character == '*' && index + 2 < pattern.Length && pattern[index + 1] == '*' && pattern[index + 2] == '/')
            {
                expression.Append("(?:.*/)?");
                index += 2;
            }
            else if (character == '*' && index + 1 < pattern.Length && pattern[index + 1] == '*')
            {
                expression.Append(".*");
                index++;
            }
            else if (character == '*')
                expression.Append("[^/]*");
            else if (character == '?')
                expression.Append("[^/]");
            else expression.Append(Regex.Escape(character.ToString()));
        }

        return expression.Append('$').ToString();
    }
}
