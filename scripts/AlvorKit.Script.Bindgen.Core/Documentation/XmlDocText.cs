namespace AlvorKit.Script.Bindgen;

/// <summary>Text cleanup helpers shared by native comment parsers.</summary>
internal static class XmlDocText
{
    /// <summary>Strips native comment delimiters and leading decoration from one raw line.</summary>
    internal static string CleanLine(string line)
    {
        line = line.Trim();
        foreach (var marker in new[] { "/**!", "/*!<", "/*!", "/**", "/*" })
            if (line.StartsWith(marker))
                line = line[marker.Length..];
        if (line.EndsWith("*/"))
            line = line[..^2];
        line = Regex.Replace(line, @"^[\s*!]+", "");
        if (line.StartsWith('<'))
            line = line[1..];
        line = Regex.Replace(line, @"^--+\s*", "");
        line = Regex.Replace(line, @"\s*\*{2,}\s*$", "");
        return line.Trim();
    }

    /// <summary>Escapes text so it can be embedded in XML documentation.</summary>
    internal static string Escape(string text) =>
        text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

    /// <summary>Removes inline Doxygen and Markdown markup that is noisy in generated C# docs.</summary>
    internal static string StripInlineMarkup(string text)
    {
        text = Regex.Replace(text, @"\[([^\]]+)\]\([^)]*\)", "$1");
        text = Regex.Replace(text, @"\[([^\]]+)\]\[[^\]]*\]", "$1");
        text = Regex.Replace(text, @"@(?:ref|p|c|a)\s+(\S+)", "$1");
        const string noiseTags =
            "win32|macos|linux|x11|wayland|egl|wgl|glx|osmesa|nsgl|posix|unix|"
            + "note|remarks?|warning|attention|thread_safety|errors?|sa|see|since|"
            + "pointer_lifetime|reentrancy|analysis|par|callback_signature|glfw3|link|endlink";
        text = Regex.Replace(text,
            @"@(?:" + noiseTags + @")\b\s*",
            "");
        text = text.Replace("__", "").Replace("`", "");
        return Regex.Replace(text, @"\s{2,}", " ").Trim();
    }

    /// <summary>Strips inline markup, escapes the result, and returns null when no prose remains.</summary>
    internal static string? CleanText(StringBuilder value)
    {
        if (value.Length == 0)
            return null;
        var cleaned = StripInlineMarkup(value.ToString());
        return cleaned.Length == 0 ? null : Escape(cleaned);
    }

    /// <summary>Tags that should terminate the current prose section without producing XML docs.</summary>
    internal static bool IsNoiseTag(string line)
    {
        const string noiseTags =
            "sa|see|since|thread_safety|errors?|remarks?|note|warning|attention|"
            + "deprecated|pointer_lifetime|reentrancy|analysis|par|callback_signature|"
            + "glfw3|code|endcode|verbatim|endverbatim|internal|pre|post|invariant|"
            + "win32|macos|linux|x11|wayland|egl|wgl|glx|osmesa|nsgl|posix|unix";
        return Regex.IsMatch(line, @"^@(" + noiseTags + @")\b");
    }

    /// <summary>Converts FreeType's lightweight markup into plain prose.</summary>
    internal static string ConvertFreeTypeMarkup(string text) =>
        Regex.Replace(Regex.Replace(text.Replace('~', ' '), @"@(?=[A-Za-z_])", ""), @"\s{2,}", " ").Trim();

    /// <summary>Appends a prose fragment with sentence spacing.</summary>
    internal static void AppendSentence(StringBuilder builder, string text)
    {
        if (builder.Length > 0)
            builder.Append(' ');
        builder.Append(text);
    }
}
