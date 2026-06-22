namespace AlvorKit.Script.AlvorSense;

/// <summary>Creates and validates common AlvorSense command-line options.</summary>
internal static class AlvorSenseCliOptions
{
    /// <summary>Default wait used by start, send, and stop commands when no timeout is supplied.</summary>
    internal static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    /// <summary>Creates a required option that must receive non-empty text.</summary>
    /// <param name="name">Option name, including leading dashes.</param>
    /// <param name="description">Help description for the option.</param>
    /// <returns>The configured option.</returns>
    internal static Option<string> RequiredStringOption(string name, string description) =>
        new(name) { Description = description, Required = true };

    /// <summary>Creates the common session id option.</summary>
    /// <returns>The configured session id option.</returns>
    internal static Option<string> SessionIdOption() =>
        RequiredStringOption("--id", "Stable session id selected by start.");

    /// <summary>Creates the common timeout option in seconds.</summary>
    /// <returns>The configured timeout option.</returns>
    internal static Option<string> TimeoutOption() =>
        new("--timeout") { Description = "Maximum wait in seconds. Defaults to 30." };

    /// <summary>Creates the send diagnostic option that includes recent stderr after target exit.</summary>
    /// <returns>The configured stderr tail option.</returns>
    internal static Option<string> StderrTailOption() =>
        new("--stderr-tail") { Description = "Include the last N stderr lines when a failed send observes target exit." };

    /// <summary>Creates an option that accepts repeated text values for validation and generated help.</summary>
    /// <param name="name">Primary option name, including leading dashes.</param>
    /// <param name="description">Help description for the option.</param>
    /// <param name="aliases">Additional option aliases.</param>
    /// <returns>The configured option.</returns>
    internal static Option<string[]> RepeatableTextOption(string name, string description, params string[] aliases) =>
        new(name, aliases)
        {
            Arity = ArgumentArity.ZeroOrMore,
            Description = description,
            CustomParser = result => [.. result.Tokens.Select(static token => token.Value)]
        };

    /// <summary>Parses a timeout in seconds and rejects non-finite or non-positive values.</summary>
    /// <param name="value">Option value supplied by the caller, or <see langword="null" /> for the default.</param>
    /// <returns>The parsed timeout.</returns>
    internal static TimeSpan Timeout(string? value)
    {
        if (value is null)
            return DefaultTimeout;

        if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds) ||
            !double.IsFinite(seconds) ||
            seconds <= 0)
        {
            throw new ArgumentException("--timeout must be a finite positive number of seconds.");
        }

        return TimeSpan.FromSeconds(seconds);
    }

    /// <summary>Parses an optional positive stderr line count.</summary>
    /// <param name="value">Option value supplied by the caller, or <see langword="null" /> when omitted.</param>
    /// <returns>The requested line count, or zero when stderr tails are disabled.</returns>
    internal static int StderrTailLines(string? value)
    {
        if (value is null)
            return 0;

        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var count) || count <= 0)
            throw new ArgumentException("--stderr-tail must be a positive integer.");

        return count;
    }

    /// <summary>Reads an option value and advances the parser index over it.</summary>
    /// <param name="args">Command-line arguments being parsed.</param>
    /// <param name="index">Current option index, advanced to the value index on success.</param>
    /// <param name="name">Option name used in error messages.</param>
    /// <returns>The option value.</returns>
    internal static string OptionValue(string[] args, ref int index, string name)
    {
        if (index + 1 >= args.Length)
            throw new ArgumentException($"Missing {name} value.");
        return args[++index];
    }

    /// <summary>Rejects empty required text options.</summary>
    /// <param name="value">Option value supplied by the caller.</param>
    /// <param name="name">Option name used in error messages.</param>
    /// <returns>The validated value.</returns>
    internal static string RequiredText(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException($"{name} must not be empty.") : value;

    /// <summary>Rejects session ids that would escape or confuse the session root directory.</summary>
    /// <param name="id">Session id supplied by the caller or generated from a project name.</param>
    /// <returns>The validated session id.</returns>
    internal static string ValidateSessionId(string id)
    {
        id = RequiredText(id, "--id");
        if (id is "." or ".." ||
            Path.IsPathRooted(id) ||
            id.Contains(Path.DirectorySeparatorChar) ||
            id.Contains(Path.AltDirectorySeparatorChar) ||
            id.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new ArgumentException("--id must be a single safe directory name.");
        }

        return id;
    }
}
