namespace AlvorKit.Windowing;

/// <summary>Parses text, float, and enum tokens for agent command-line arguments.</summary>
internal static class AgentWindowCommandArgumentText
{
    /// <summary>Parses a required finite single-precision token.</summary>
    /// <param name="result">Argument result containing parsed tokens.</param>
    /// <param name="name">Argument name used in validation errors.</param>
    /// <returns>The parsed value, or zero after recording a parse error.</returns>
    internal static float ParseFloat(ArgumentResult result, string name)
    {
        var token = RequiredToken(result, name);
        if (token is null)
            return 0;

        if (float.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) && float.IsFinite(value))
            return value;

        result.AddError($"{name} must be a finite single-precision number.");
        return 0;
    }

    /// <summary>Parses an optional finite single-precision token.</summary>
    /// <param name="result">Argument result containing parsed tokens.</param>
    /// <param name="name">Argument name used in validation errors.</param>
    /// <returns>The parsed value, or <see langword="null" /> when omitted or invalid.</returns>
    internal static float? ParseOptionalFloat(ArgumentResult result, string name)
    {
        var token = OptionalToken(result, name);
        if (token is null)
            return null;

        if (float.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) && float.IsFinite(value))
            return value;

        result.AddError($"{name} must be a finite single-precision number.");
        return null;
    }

    /// <summary>Parses a case-insensitive enum token.</summary>
    /// <param name="result">Argument result containing parsed tokens.</param>
    /// <param name="name">Argument name used in validation errors.</param>
    /// <typeparam name="T">Enum type accepted by the argument.</typeparam>
    /// <returns>The parsed enum value, or default after recording a parse error.</returns>
    internal static T ParseEnum<T>(ArgumentResult result, string name)
        where T : struct, Enum
    {
        var token = RequiredToken(result, name);
        if (token is not null && System.Enum.TryParse<T>(token, true, out var value) && System.Enum.IsDefined(value))
            return value;

        result.AddError($"{name} must be one of: {string.Join(", ", System.Enum.GetNames<T>())}.");
        return default;
    }

    /// <summary>Reads a required single token.</summary>
    /// <param name="result">Argument result containing parsed tokens.</param>
    /// <param name="name">Argument name used in validation errors.</param>
    /// <returns>The token, or <see langword="null" /> after recording a missing-value error.</returns>
    internal static string? RequiredToken(ArgumentResult result, string name)
    {
        var token = OptionalToken(result, name);
        if (token is null)
            result.AddError($"{name} is required.");
        return token;
    }

    /// <summary>Reads an optional single token and rejects multiple values.</summary>
    /// <param name="result">Argument result containing parsed tokens.</param>
    /// <param name="name">Argument name used in validation errors.</param>
    /// <returns>The token, or <see langword="null" /> when omitted or invalid.</returns>
    internal static string? OptionalToken(ArgumentResult result, string name)
    {
        string? token = null;
        var count = 0;
        foreach (var item in result.Tokens)
        {
            token = item.Value;
            count++;
        }

        if (count <= 1)
            return token;

        result.AddError($"{name} expects one value.");
        return null;
    }
}
