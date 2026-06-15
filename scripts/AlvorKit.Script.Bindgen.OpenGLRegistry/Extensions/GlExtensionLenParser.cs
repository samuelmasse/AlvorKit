namespace AlvorKit.Script.Bindgen;

/// <summary>Parses OpenGL registry len metadata for convenience overload generation.</summary>
internal static class GlExtensionLenParser
{
    /// <summary>Parses a registry len expression for a command parameter.</summary>
    public static GlExtensionLenInfo Parse(GlCommand command, GlParameter parameter)
    {
        if (parameter.Len is null)
            return GlExtensionLenInfo.None;
        if (int.TryParse(parameter.Len, out var literal))
            return new(GlExtensionLenKind.Literal, -1, literal, []);
        if (parameter.Len.StartsWith("COMPSIZE(") && parameter.Len.EndsWith(")"))
            return new(GlExtensionLenKind.CompSize, -1, 1, parameter.Len["COMPSIZE(".Length..^1].Split(',', StringSplitOptions.RemoveEmptyEntries));

        var match = Regex.Match(parameter.Len, @"^(\w+)(?:\*(\d+))?$");
        if (!match.Success)
            return new(GlExtensionLenKind.Unknown, -1, 1, []);
        var index = command.Parameters.ToList().FindIndex(candidate => candidate.NativeName == match.Groups[1].Value);
        if (index < 0 || command.Parameters[index].PointerDepth != 0)
            return new(GlExtensionLenKind.Unknown, -1, 1, []);
        var divisor = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 1;
        return new(GlExtensionLenKind.ParamRef, index, divisor, []);
    }
}
