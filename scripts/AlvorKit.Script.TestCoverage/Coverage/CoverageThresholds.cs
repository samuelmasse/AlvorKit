namespace AlvorKit.Script.TestCoverage;

/// <summary>Required coverage percentages for each metric.</summary>
/// <param name="Line">Required line coverage percentage.</param>
/// <param name="Branch">Required branch coverage percentage.</param>
/// <param name="Method">Required method coverage percentage.</param>
internal sealed record CoverageThresholds(double Line, double Branch, double Method)
{
    /// <summary>Repository default coverage gate: 95% line/method and 85% branch.</summary>
    public static CoverageThresholds Default { get; } = new(95.0, 85.0, 95.0);

    /// <summary>Returns true when every metric threshold is disabled.</summary>
    public bool IsDisabled => Line <= 0 && Branch <= 0 && Method <= 0;

    /// <summary>Formats thresholds for human-readable reports.</summary>
    public string Format() =>
        $"{Format(Line)}% line, {Format(Branch)}% branch, and {Format(Method)}% method coverage";

    /// <summary>Builds thresholds with one required percentage for every metric.</summary>
    public static CoverageThresholds All(double threshold) => new(threshold, threshold, threshold);

    /// <summary>Rejects thresholds outside percentage bounds.</summary>
    public void Validate()
    {
        Validate(Line);
        Validate(Branch);
        Validate(Method);
    }

    /// <summary>Rejects one threshold outside percentage bounds.</summary>
    private static void Validate(double threshold)
    {
        if (threshold is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold must be between 0 and 100.");
    }

    /// <summary>Formats one threshold value using invariant culture.</summary>
    private static string Format(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);
}
