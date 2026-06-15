namespace AlvorKit.Script.TestCoverage;

/// <summary>Shared JSON formatting for generated coverage reports.</summary>
internal static class CoverageJson
{
    /// <summary>Serializer settings used for agent-readable reports.</summary>
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };
}
