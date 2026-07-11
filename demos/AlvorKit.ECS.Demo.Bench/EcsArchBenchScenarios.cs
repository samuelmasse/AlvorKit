namespace AlvorKit.ECS.Demo.Bench;

/// <summary>Builds the stable AFR-02 archetypal scenario catalog.</summary>
internal static class EcsArchBenchScenarios
{
    internal static EcsArchBenchScenario[] Create(ReadOnlySpan<int> widths)
    {
        var scenarios = new List<EcsArchBenchScenario>();
        foreach (int width in widths)
        {
            string suffix = $"k{width:00}";
            scenarios.Add(new($"arch-get-present-{suffix}", "point", "op", width));
            scenarios.Add(new($"arch-get-absent-{suffix}", "point", "op", width));
            scenarios.Add(new($"arch-has-present-{suffix}", "point", "op", width));
            scenarios.Add(new($"arch-has-absent-{suffix}", "point", "op", width));
            scenarios.Add(new($"arch-set-existing-{suffix}", "point", "op", width));
        }

        scenarios.AddRange(
        [
            new("arch-get-wide-k08", "value-shape", "op", 8),
            new("arch-set-wide-k08", "value-shape", "op", 8),
            new("arch-get-reference-k08", "value-shape", "op", 8),
            new("arch-set-reference-k08", "value-shape", "op", 8),
            new("arch-get-ref-struct-k08", "value-shape", "op", 8),
            new("arch-set-ref-struct-k08", "value-shape", "op", 8),
            new("arch-add-cached-k08", "structural", "move", 8),
            new("arch-add-growth-k08", "growth", "move", 8),
            new("arch-add-unknown-k08", "structural", "move", 8),
            new("arch-remove-cached-k08", "structural", "move", 8),
            new("arch-remove-unknown-k08", "structural", "move", 8),
            new("arch-compact-first-k08", "compaction", "move", 8),
            new("arch-compact-middle-k08", "compaction", "move", 8),
            new("arch-compact-last-k08", "compaction", "move", 8),
            new("arch-create-unique-gray", "catalog", "arch"),
            new("arch-low-occupancy", "footprint", "arch"),
            new("arch-high-occupancy", "footprint", "row"),
            new("arch-concurrent-get-a01", "concurrency", "op", 8),
            new("arch-concurrent-get-many", "concurrency", "op", 8),
            new("arch-concurrent-set-a01", "concurrency", "op", 8),
            new("arch-concurrent-set-many", "concurrency", "op", 8),
            new("arch-concurrent-resolve-many", "concurrency", "move", 8),
        ]);

        return [.. scenarios];
    }

    /// <summary>Builds the opt-in AFR-21 membership-kernel study cases.</summary>
    internal static EcsArchBenchScenario[] CreateMembershipStudy(ReadOnlySpan<int> widths)
    {
        var scenarios = new List<EcsArchBenchScenario>(widths.Length * 16);
        foreach (int width in widths)
        {
            string suffix = $"k{width:00}";
            AddMembershipCases(scenarios, "indexof", suffix, width);
            AddMembershipCases(scenarios, "binary", suffix, width);
            AddMembershipCases(scenarios, "ordinalhash", suffix, width);
            AddMembershipCases(scenarios, "ideal-direct", suffix, width);
        }

        return [.. scenarios];
    }

    /// <summary>Builds the opt-in AFR-24 point-call and attribution study cases.</summary>
    internal static EcsArchBenchScenario[] CreateHotPathStudy()
    {
        string[] shapes = ["scalar", "wide", "reference", "refstruct"];
        string[] operations = ["get", "set"];
        string[] callSites = ["concrete", "generic"];
        string[] groups = ["class", "struct"];
        string[] workingSets = ["one", "r1024"];
        var scenarios = new List<EcsArchBenchScenario>(74);

        foreach (string shape in shapes)
            foreach (string operation in operations)
                foreach (string callSite in callSites)
                    foreach (string group in groups)
                        foreach (string workingSet in workingSets)
                        {
                            scenarios.Add(new(
                                $"arch-hot-full-{shape}-{operation}-{callSite}-{group}-{workingSet}",
                                "hot-path",
                                "op"));
                        }

        foreach (string callSite in callSites)
            foreach (string group in groups)
            {
                scenarios.Add(new($"arch-hot-stage-loc-{callSite}-{group}-r1024", "hot-path-stage", "op"));
                scenarios.Add(new(
                    $"arch-hot-stage-directory-{callSite}-{group}-r1024",
                    "hot-path-stage",
                    "op"));
            }

        foreach (string operation in operations)
            scenarios.Add(new($"arch-hot-stage-row-{operation}-r1024", "hot-path-stage", "op"));

        return [.. scenarios];
    }

    private static void AddMembershipCases(List<EcsArchBenchScenario> scenarios, string algorithm, string suffix, int width)
    {
        scenarios.Add(new($"arch-membership-{algorithm}-present-first-{suffix}", "membership-kernel", "lookup", width));
        scenarios.Add(new($"arch-membership-{algorithm}-present-rotating-{suffix}", "membership-kernel", "lookup", width));
        scenarios.Add(new($"arch-membership-{algorithm}-absent-interior-{suffix}", "membership-kernel", "lookup", width));
        scenarios.Add(new($"arch-membership-{algorithm}-absent-high-{suffix}", "membership-kernel", "lookup", width));
    }
}
