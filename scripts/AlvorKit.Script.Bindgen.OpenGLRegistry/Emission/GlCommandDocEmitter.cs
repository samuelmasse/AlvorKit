namespace AlvorKit.Script.Bindgen;

/// <summary>Emits XML documentation for generated OpenGL commands.</summary>
internal static class GlCommandDocEmitter
{
    /// <summary>Emits searchable command docs from registry availability and optional refpage prose.</summary>
    public static void Emit(StringBuilder output, GlCommand command)
    {
        var availability = GlCodeEmissionContext.AvailabilityText(command.Availability);
        if (command.Documentation?.Summary is { } summary)
        {
            var purpose = Capitalize(summary);
            var terminated = purpose.EndsWith('.') || purpose.EndsWith('!') || purpose.EndsWith('?');
            output.AppendLine($"    /// <summary><c>{command.NativeName}</c> ({availability}) - {purpose}{(terminated ? "" : ".")}</summary>");
        }
        else
        {
            output.AppendLine($"    /// <summary><c>{command.NativeName}</c> ({availability})</summary>");
        }

        if (command.Documentation is { } documentation)
            foreach (var parameter in command.Parameters)
                if (documentation.Parameters.TryGetValue(parameter.NativeName, out var text))
                    output.AppendLine($"    /// <param name=\"{parameter.NativeName}\">{text}</param>");
    }

    /// <summary>Capitalizes the first ASCII letter for generated summary sentences.</summary>
    private static string Capitalize(string text) =>
        text.Length > 0 && char.IsAsciiLetterLower(text[0]) ? char.ToUpperInvariant(text[0]) + text[1..] : text;
}
