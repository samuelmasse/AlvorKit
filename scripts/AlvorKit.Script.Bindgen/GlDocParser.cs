using System.Xml;
using System.Xml.Linq;

namespace AlvorKit.Script.Bindgen;

/// <summary>
/// Parses the Khronos OpenGL reference pages (DocBook XML, the gl4 directory) into doc comments,
/// keyed by GL function name. One page documents a whole family - glVertexAttribPointer.xml covers
/// the I and L variants, glUniform.xml covers every glUniform* - so its purpose and parameter
/// descriptions are attached to each function in its synopsis.
/// </summary>
public sealed class GlDocParser
{
    private static readonly XNamespace Db = "http://docbook.org/ns/docbook";
    private static readonly XNamespace MathML = "http://www.w3.org/1998/Math/MathML";
    private static readonly XName XmlId = XNamespace.Xml + "id";

    public Dictionary<string, XmlDocComment> Parse(string gl4Directory)
    {
        var byFunction = new Dictionary<string, XmlDocComment>();
        foreach (var path in Directory.EnumerateFiles(gl4Directory, "gl*.xml").Order(StringComparer.Ordinal))
        {
            if (LoadRefentry(path) is not { } root)
                continue;

            var functions = root.Descendants(Db + "refsynopsisdiv").Descendants(Db + "function")
                .Select(function => function.Value.Trim())
                .Where(name => name.Length > 0)
                .Distinct()
                .ToList();
            if (functions.Count == 0)
                continue;

            var purpose = Normalize(Flatten(root.Descendants(Db + "refpurpose").FirstOrDefault()));
            var parameters = ParseParameters(root);
            var doc = new XmlDocComment(
                purpose.Length > 0 ? XmlDocComment.Escape(purpose) : null,
                parameters,
                Returns: null,
                Remarks: null);
            foreach (var function in functions)
                byFunction[function] = doc;
        }
        return byFunction;
    }

    /// <summary>
    /// Reads a refpage, dropping the DOCTYPE (it pulls in an external MathML entity file) and any
    /// remaining non-standard named entities, which only appear inside the equation markup we skip.
    /// </summary>
    private static XElement? LoadRefentry(string path)
    {
        var text = File.ReadAllText(path);
        text = Regex.Replace(text, @"<!DOCTYPE.*?\]>", "", RegexOptions.Singleline);
        text = Regex.Replace(text, @"<!DOCTYPE[^>\[]*>", "");
        text = Regex.Replace(text, @"&(?!(?:amp|lt|gt|quot|apos);)[a-zA-Z][a-zA-Z0-9]*;", " ");
        try
        {
            return XDocument.Parse(text).Root;
        }
        catch (XmlException)
        {
            return null;
        }
    }

    /// <summary>
    /// The parameter descriptions from the Parameters section(s): each varlistentry maps every
    /// parameter named in its terms to the shared listitem text.
    /// </summary>
    private static Dictionary<string, string> ParseParameters(XElement root)
    {
        var sections = root.Descendants(Db + "refsect1").Where(section =>
            ((string?)section.Attribute(XmlId))?.StartsWith("parameters", StringComparison.OrdinalIgnoreCase) == true
            || string.Equals(section.Element(Db + "title")?.Value.Trim(), "Parameters", StringComparison.OrdinalIgnoreCase));

        var parameters = new Dictionary<string, string>();
        foreach (var entry in sections.SelectMany(section => section.Descendants(Db + "varlistentry")))
        {
            var text = Normalize(Flatten(entry.Element(Db + "listitem")));
            if (text.Length == 0)
                continue;
            var names = entry.Elements(Db + "term")
                .SelectMany(term => term.Descendants(Db + "parameter"))
                .Select(parameter => parameter.Value.Trim())
                .Where(name => name.Length > 0);
            foreach (var name in names)
                parameters[name] = XmlDocComment.Escape(text);
        }
        return parameters;
    }

    /// <summary>Concatenates the descendant text, skipping equation markup and man-volume numbers.</summary>
    private static string Flatten(XElement? element)
    {
        if (element is null)
            return "";
        var output = new StringBuilder();
        foreach (var node in element.Nodes())
        {
            if (node is XText text)
                output.Append(text.Value);
            else if (node is XElement child && child.Name.Namespace != MathML && child.Name.LocalName != "manvolnum")
                output.Append(Flatten(child));
        }
        return output.ToString();
    }

    private static string Normalize(string text) => Regex.Replace(text, @"\s+", " ").Trim();
}
