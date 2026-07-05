namespace AlvorKit.Script.DevSolution;

/// <summary>Formats generated .slnx XML consistently.</summary>
internal static class SlnxXml
{
    /// <summary>Formats a solution document without an XML declaration.</summary>
    /// <param name="document">Document to format.</param>
    public static string Format(XDocument document)
    {
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "    ",
            NewLineChars = Environment.NewLine,
            OmitXmlDeclaration = true
        };

        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        using (var xml = XmlWriter.Create(writer, settings))
            document.Save(xml);

        return writer.ToString() + Environment.NewLine;
    }
}
