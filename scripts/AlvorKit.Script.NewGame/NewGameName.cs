namespace AlvorKit.Script.NewGame;

/// <summary>Validated game name values used for C# identifiers and display text.</summary>
/// <param name="Identifier">PascalCase value safe for project names, namespaces, and type references.</param>
/// <param name="Title">Human-facing title derived from the same input.</param>
internal sealed record NewGameName(string Identifier, string Title)
{
    /// <summary>Creates a PascalCase identifier from a user-supplied game name.</summary>
    public static NewGameName Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Game name must not be blank.", nameof(value));

        var words = new List<string>();
        var word = new StringBuilder(value.Length);
        foreach (var c in value)
        {
            if (!char.IsLetterOrDigit(c))
            {
                FlushWord();
                continue;
            }

            word.Append(c);
        }

        FlushWord();

        var identifier = string.Concat(words.Select(TitleCase));
        if (identifier.Length == 0 || !char.IsLetter(identifier[0]))
            throw new ArgumentException("Game name must start with a letter after normalization.", nameof(value));
        if (identifier.Any(c => !char.IsLetterOrDigit(c)))
            throw new ArgumentException("Game name must contain only letters, digits, spaces, hyphens, or underscores.", nameof(value));

        return new(identifier, string.Join(' ', words.Select(TitleCase)));

        void FlushWord()
        {
            if (word.Length == 0)
                return;

            words.Add(word.ToString());
            word.Clear();
        }
    }

    /// <summary>Uppercases the first character of a parsed name word.</summary>
    private static string TitleCase(string word) =>
        char.ToUpperInvariant(word[0]) + word[1..];
}
