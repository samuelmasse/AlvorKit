namespace AlvorKit.ECS.Generator;

internal static class ComponentTemplate
{
        private static readonly Regex PlaceholderPattern = new(@"\{\{(?<name>[A-Za-z0-9_.-]+)\}\}", RegexOptions.Compiled);

        private const string ResourcePrefix = "AlvorKit.ECS.Generator.Templates.";

        internal static string Render(string templateName, params (string Name, string Value)[] values)
    {
        var byName = values.ToDictionary(value => value.Name, value => value.Value, StringComparer.Ordinal);
        return PlaceholderPattern.Replace(
            Read(templateName),
            match => byName.TryGetValue(match.Groups["name"].Value, out var value)
                ? value
                : throw new InvalidOperationException($"Template '{templateName}' has no value for placeholder '{match.Value}'."));
    }

        internal static string RenderFragment(string templateName, params (string Name, string Value)[] values) =>
        Render(templateName, values).TrimEnd('\r', '\n') + "\n\n";

        private static string Read(string templateName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = ResourcePrefix + templateName;
        using var stream = assembly.GetManifestResourceStream(resourceName) ??
            throw new FileNotFoundException($"Embedded template '{templateName}' was not found.", resourceName);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}
