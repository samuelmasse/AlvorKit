namespace AlvorKit.Script.Bindgen;

/// <summary>Normalizes Khronos OpenGL reference-page prose for generated XML documentation.</summary>
internal static class GlDocText
{
    private const string CodeStart = "\uE000";
    private const string CodeEnd = "\uE001";

    /// <summary>Escapes prose while preserving code tags for native OpenGL symbols.</summary>
    public static string Escape(string text) =>
        XmlDocComment.Escape(CodeNativeReferences(text))
            .Replace(CodeStart, "<c>")
            .Replace(CodeEnd, "</c>");

    /// <summary>Wraps native OpenGL command and token references in placeholders that survive XML escaping.</summary>
    private static string CodeNativeReferences(string text)
    {
        text = Regex.Replace(text, @"\b(GL_[A-Z0-9_]+)\$[A-Za-z]\$", "$1i");
        text = Regex.Replace(
            text,
            @"\bGL_[A-Z0-9_]*[a-z]?\b",
            match => Code(match.Value));
        return Regex.Replace(
            text,
            @"\bgl[A-Z][A-Za-z0-9_]*(?:\*[A-Za-z0-9_]*)?\b",
            match => Code(match.Value));
    }

    /// <summary>Wraps a native symbol with placeholders that survive XML escaping as code tags.</summary>
    private static string Code(string value) => CodeStart + value + CodeEnd;
}
