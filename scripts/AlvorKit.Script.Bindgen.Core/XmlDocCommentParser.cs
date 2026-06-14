namespace AlvorKit.Script.Bindgen;

/// <summary>Routes raw native comments to the supported XML documentation parsers.</summary>
internal static class XmlDocCommentParser
{
    /// <summary>Parses a raw native comment into XML documentation fields.</summary>
    internal static XmlDocComment? Parse(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var nonEmptyLines = raw.Split('\n')
            .Select(XmlDocText.CleanLine)
            .Where(line => line.Length > 0)
            .ToList();

        return HasSectionTags(nonEmptyLines)
            ? FreeTypeDocParser.Parse(nonEmptyLines)
            : DoxygenDocParser.Parse(raw);
    }

    /// <summary>Parses a short member-level comment into escaped prose.</summary>
    internal static string? Member(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var text = string.Join(' ', raw.Split('\n')
            .Select(XmlDocText.CleanLine)
            .Where(line => line.Length > 0))
            .Trim();

        text = XmlDocText.StripInlineMarkup(text);
        return text.Length == 0 ? null : XmlDocText.Escape(text);
    }

    /// <summary>Detects FreeType-style comments that label sections with @description and friends.</summary>
    private static bool HasSectionTags(IEnumerable<string> lines) =>
        lines.Any(line => Regex.IsMatch(line, @"^@\w+:"));
}
