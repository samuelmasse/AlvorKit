namespace AlvorKit;

internal record ComparisonCase(
    string Scenario, string Framework, string Storage, string Variant, string Mode,
    Type Workload, Func<int, int, BenchResult> Measure)
{
    private string RepresentativeVariant => (Framework, Mode) switch
    {
        ("AlvorKit", "Simd") => "Query_SIMD",
        ("AlvorKit", _) when Storage == "Sparse" => Scenario.StartsWith("Create") ? "Sparse" : "SparseHandles",
        ("AlvorKit", _) => Scenario.StartsWith("Create") ? "ArchetypalReusedBuilder" : "Rows",
        (_, "Simd") => Framework switch
        {
            "Frent" => "Simd",
            "FrifloEngineEcs" => "SIMD_Scalar",
            _ => throw new InvalidOperationException($"No SIMD representative for {Framework}."),
        },
        ("Morpeh", _) => "Stash",
        _ when Scenario.StartsWith("Create") => "Default",
        ("Arch", _) => Scenario == "Mixed" ? "Default" : "Scalar",
        ("DefaultEcs", _) => Scenario == "Update1" ? "ComponentSystem_Scalar" : "Scalar",
        ("Entitas", _) => "GroupDirect",
        ("Fennecs", _) => "ForEach",
        ("FlecsNet", _) => "Iter",
        ("Frent", _) => "QueryInline",
        ("FrifloEngineEcs", _) => "Scalar",
        ("SveltoECS", "Scalar") => "Default",
        _ => throw new InvalidOperationException($"No representative for {Scenario}/{Framework}/{Mode}."),
    };
    internal bool Representative => Variant == RepresentativeVariant;

    internal string Id(int count, int padding) => $"{Scenario}/N{count}/Padding{padding}/{Mode}/{Framework}/{Variant}";
}
