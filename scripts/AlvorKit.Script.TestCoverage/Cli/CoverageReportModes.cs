namespace AlvorKit.Script.TestCoverage;

/// <summary>Tracks which coverage report artifacts should be generated.</summary>
/// <param name="Html">Whether to generate browser-readable HTML output.</param>
/// <param name="Cobertura">Whether to emit Cobertura XML output.</param>
/// <param name="Lcov">Whether to emit LCOV output.</param>
internal sealed record CoverageReportModes(bool Html, bool Cobertura, bool Lcov)
{
    /// <summary>All report artifact formats used by the full local workflow.</summary>
    public static CoverageReportModes All { get; } = new(Html: true, Cobertura: true, Lcov: true);

    /// <summary>Minimal report artifact formats used by agent workflows.</summary>
    public static CoverageReportModes Agent { get; } = new(Html: false, Cobertura: false, Lcov: false);
}
