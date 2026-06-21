namespace AlvorKit.Script.Bindgen;

/// <summary>Represents temporary XML code markers that survive text escaping.</summary>
internal static class XmlDocCode
{
    /// <summary>Placeholder written before native code text before XML escaping.</summary>
    internal const string Start = "\uE000";

    /// <summary>Placeholder written after native code text before XML escaping.</summary>
    internal const string End = "\uE001";

    /// <summary>Wraps native code text in placeholders consumed by <see cref="XmlDocText.Escape"/>.</summary>
    internal static string Wrap(string value) => Start + value + End;
}
