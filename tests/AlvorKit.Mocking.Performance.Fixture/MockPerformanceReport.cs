namespace AlvorKit;

/// <summary>Combines environment, method, measurements, and explicit deferrals in one artifact.</summary>
internal sealed record MockPerformanceReport(
    MockPerformanceEnvironment Environment,
    MockPerformanceOptions Options,
    MockPerformanceResult[] Results,
    string[] MeasurementBoundaries);
