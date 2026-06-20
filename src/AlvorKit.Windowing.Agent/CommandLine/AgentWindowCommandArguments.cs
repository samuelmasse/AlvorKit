namespace AlvorKit.Windowing;

/// <summary>Creates validated System.CommandLine arguments for agent window commands.</summary>
internal static class AgentWindowCommandArguments
{
    /// <summary>Creates a finite non-negative double argument.</summary>
    /// <param name="name">Argument name used in help and validation errors.</param>
    /// <returns>The configured argument.</returns>
    internal static Argument<double> NonNegativeDouble(string name) =>
        new(name) { CustomParser = result => ParseDouble(result, name, true) };

    /// <summary>Creates a finite optional non-negative double argument.</summary>
    /// <param name="name">Argument name used in help and validation errors.</param>
    /// <returns>The configured argument.</returns>
    internal static Argument<double?> OptionalNonNegativeDouble(string name) =>
        new(name) { Arity = ArgumentArity.ZeroOrOne, CustomParser = result => ParseOptionalDouble(result, name, true) };

    /// <summary>Creates a non-negative integer argument.</summary>
    /// <param name="name">Argument name used in help and validation errors.</param>
    /// <returns>The configured argument.</returns>
    internal static Argument<int> NonNegativeInt(string name) =>
        new(name) { CustomParser = result => ParseInt(result, name, true) };

    /// <summary>Creates a finite single-precision argument.</summary>
    /// <param name="name">Argument name used in help and validation errors.</param>
    /// <returns>The configured argument.</returns>
    internal static Argument<float> Float(string name) =>
        new(name) { CustomParser = result => AgentWindowCommandArgumentText.ParseFloat(result, name) };

    /// <summary>Creates an optional finite single-precision argument.</summary>
    /// <param name="name">Argument name used in help and validation errors.</param>
    /// <returns>The configured argument.</returns>
    internal static Argument<float?> OptionalFloat(string name) =>
        new(name) { Arity = ArgumentArity.ZeroOrOne, CustomParser = result => AgentWindowCommandArgumentText.ParseOptionalFloat(result, name) };

    /// <summary>Creates a case-insensitive enum argument.</summary>
    /// <param name="name">Argument name used in help and validation errors.</param>
    /// <typeparam name="T">Enum type accepted by the argument.</typeparam>
    /// <returns>The configured argument.</returns>
    internal static Argument<T> Enum<T>(string name)
        where T : struct, Enum =>
        new(name) { CustomParser = result => AgentWindowCommandArgumentText.ParseEnum<T>(result, name) };

    /// <summary>Creates an argument that accepts one or more words.</summary>
    /// <param name="name">Argument name used in help and validation errors.</param>
    /// <returns>The configured argument.</returns>
    internal static Argument<string[]> Words(string name) =>
        new(name) { Arity = ArgumentArity.OneOrMore };

    /// <summary>Parses a required finite double token.</summary>
    private static double ParseDouble(ArgumentResult result, string name, bool nonNegative)
    {
        var token = AgentWindowCommandArgumentText.RequiredToken(result, name);
        if (token is null)
            return 0;

        if (!double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) || !double.IsFinite(value))
        {
            result.AddError($"{name} must be a finite number.");
            return 0;
        }

        if (nonNegative && value < 0)
        {
            result.AddError($"{name} must be non-negative.");
            return 0;
        }

        return value;
    }

    /// <summary>Parses an optional finite double token.</summary>
    private static double? ParseOptionalDouble(ArgumentResult result, string name, bool nonNegative)
    {
        var token = AgentWindowCommandArgumentText.OptionalToken(result, name);
        if (token is null)
            return null;

        if (!double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) || !double.IsFinite(value))
        {
            result.AddError($"{name} must be a finite number.");
            return null;
        }

        if (nonNegative && value < 0)
        {
            result.AddError($"{name} must be non-negative.");
            return null;
        }

        return value;
    }

    /// <summary>Parses a required integer token.</summary>
    private static int ParseInt(ArgumentResult result, string name, bool nonNegative)
    {
        var token = AgentWindowCommandArgumentText.RequiredToken(result, name);
        if (token is null)
            return 0;

        if (!int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            result.AddError($"{name} must be an integer.");
            return 0;
        }

        if (nonNegative && value < 0)
        {
            result.AddError($"{name} must be non-negative.");
            return 0;
        }

        return value;
    }
}
