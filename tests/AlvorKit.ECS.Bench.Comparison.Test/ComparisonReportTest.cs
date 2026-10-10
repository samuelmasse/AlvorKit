namespace AlvorKit;

[TestClass]
public class ComparisonReportTest
{
    /// <summary>Every comparable group has exactly one predetermined overview representative.</summary>
    [TestMethod]
    public void RepresentativesCoverEverySupportedGroupExactlyOnce()
    {
        var cases = ComparisonCatalog.Cases;
        Assert.AreEqual(134, cases.Length);
        Assert.AreEqual(200, cases.Sum(entry => entry.Scenario.StartsWith("Update") ? 2 : 1));
        string[] frameworks =
        [
            "AlvorKit", "Arch", "DefaultEcs", "Entitas", "Fennecs", "FlecsNet", "Frent", "FrifloEngineEcs", "Morpeh", "SveltoECS",
        ];
        CollectionAssert.AreEquivalent(frameworks, cases.Select(entry => entry.Framework).Distinct().ToArray());
        var catalog = new BenchCatalogBuilder().Create(new ComparisonBenchmarks().Create());
        Assert.AreEqual(200, catalog.Cases.Select(entry => entry.Id).Distinct().Count());

        foreach (var entry in cases)
        {
            Assert.IsTrue(catalog.Cases.Any(item => item.Id == entry.Id(100000, 0)));

            if (entry.Scenario.StartsWith("Update"))
                Assert.IsTrue(catalog.Cases.Any(item => item.Id == entry.Id(100000, 10)));
        }

        foreach (var group in cases.GroupBy(entry => (entry.Scenario, entry.Framework, entry.Storage, entry.Mode)))
            Assert.AreEqual(1, group.Count(entry => entry.Representative), group.Key.ToString());

        Assert.IsTrue(cases.All(entry => entry.Mode is "Scalar" or "Simd"));

        foreach (var group in cases.GroupBy(entry => (entry.Scenario, entry.Mode)))
        {
            Assert.IsTrue(group.Any(entry => entry.Framework == "AlvorKit" && entry.Representative));
            Assert.IsTrue(group.Any(entry => entry.Framework != "AlvorKit" && entry.Representative));
        }
    }

    /// <summary>Embedded JSON cannot close its script element, and retains the exact metadata after parsing.</summary>
    [TestMethod]
    public void ReportEscapesEmbeddedDataWithoutChangingIt()
    {
        var document = Document();
        const string text = "</script><script>alert('benchmark')</script>";
        document["note"] = text;
        var html = ComparisonReport.Render(document.ToJsonString());
        Assert.IsFalse(html.Contains(text));
        const string start = "<script type=\"application/json\" id=\"data\">";
        var offset = html.IndexOf(start, StringComparison.Ordinal) + start.Length;
        var end = html.IndexOf("</script>", offset, StringComparison.Ordinal);
        var embedded = JsonNode.Parse(html[offset..end])!;
        Assert.AreEqual(text, (string?)embedded["note"]);
        Assert.AreEqual(100, (int)embedded["benchmarks"]![0]!["samples"]![0]!["elapsedNanoseconds"]!);
    }

    /// <summary>Every source link reaches the timed workload called by its registered measurement method.</summary>
    [TestMethod]
    public void SourceLinksMatchEveryRegisteredMeasurement()
    {
        var approaches = ComparisonProvenance.Capture()["approaches"]!.AsArray();
        var sources = ComparisonSources.Capture(approaches);
        var root = (string)sources["root"]!;
        var recorded = sources["approaches"]!.AsObject();
        Assert.AreEqual(134, recorded.Count);

        foreach (var entry in ComparisonCatalog.Cases)
        {
            var source = recorded[$"{entry.Scenario}/{entry.Mode}/{entry.Framework}/{entry.Variant}"]!;
            var workload = source["workload"]!;
            var measurement = source["measurement"]!;
            var workloadLines = File.ReadAllLines(Path.Combine(root, (string)workload["path"]!));
            StringAssert.Contains(workloadLines[(int)workload["line"]! - 1], " Run(");
            var measurementLines = File.ReadAllLines(Path.Combine(root, (string)measurement["path"]!));
            var start = (int)measurement["line"]! - 1;
            StringAssert.Contains(measurementLines[start], $" BenchResult {entry.Measure.Method.Name}(");
            var body = string.Join('\n', measurementLines.Skip(start + 1).TakeWhile(line => line != "    }"));
            var timedCall = body.IndexOf($"{entry.Workload.Name}.Run(", StringComparison.Ordinal);
            var timingStart = body.IndexOf("BenchTimer.Start()", StringComparison.Ordinal);
            var timingEnd = body.IndexOf("timer.Stop(", StringComparison.Ordinal);
            Assert.IsTrue(timingStart >= 0 && timedCall > timingStart && timedCall < timingEnd, entry.Id(100000, 0));
        }
    }

    /// <summary>Report source navigation is embedded separately and preserves the original saved observations.</summary>
    [TestMethod]
    public void ReportEmbedsSourceLocationsSeparatelyFromMeasurements()
    {
        var document = Document();
        var html = ComparisonReport.Render(document.ToJsonString());
        const string marker = "<script type=\"application/json\" id=\"sources\">";
        var start = html.IndexOf(marker, StringComparison.Ordinal) + marker.Length;
        var end = html.IndexOf("</script>", start, StringComparison.Ordinal);
        var source = JsonNode.Parse(html[start..end])!;
        var workload = source["approaches"]!["Create1/Scalar/Arch/Default"]!["workload"]!;
        Assert.AreEqual("bench/AlvorKit.ECS.Bench.Comparison/Workloads/Create1/Scalar/RaceArchCreate1Default.cs",
            (string)workload["path"]!);
        Assert.IsTrue((int)workload["line"]! > 0);
        Assert.IsTrue(Path.IsPathFullyQualified((string)source["root"]!));
        const string dataMarker = "<script type=\"application/json\" id=\"data\">";
        var dataStart = html.IndexOf(dataMarker, StringComparison.Ordinal) + dataMarker.Length;
        var dataEnd = html.IndexOf("</script>", dataStart, StringComparison.Ordinal);
        Assert.IsTrue(JsonNode.DeepEquals(document, JsonNode.Parse(html[dataStart..dataEnd])));
    }

    /// <summary>Both storage models have fixed scalar representatives in all seven workloads, with explicit SIMD gaps.</summary>
    [TestMethod]
    public void AlvorKitStorageModelsHaveIndependentRepresentatives()
    {
        var cases = ComparisonCatalog.Cases.Where(entry => entry.Framework == "AlvorKit").ToArray();

        foreach (var scenario in cases.Select(entry => entry.Scenario).Distinct())
        {
            var scalar = cases.Where(entry => entry.Scenario == scenario && entry.Mode == "Scalar").ToArray();
            Assert.AreEqual(2, scalar.Count(entry => entry.Representative));
            Assert.AreEqual(1, scalar.Count(entry => entry.Storage == "Sparse" && entry.Representative));
            Assert.AreEqual(1, scalar.Count(entry => entry.Storage == "Archetypal" && entry.Representative));

            if (scenario.StartsWith("Create"))
            {
                Assert.IsTrue(scalar.Single(entry => entry.Variant == "ArchetypalReusedBuilder").Representative);
                Assert.IsFalse(scalar.Single(entry => entry.Variant == "ArchetypalSetters").Representative);
                Assert.IsFalse(scalar.Single(entry => entry.Variant == "SparseMutator").Representative);
            }
        }

        Assert.IsFalse(cases.Any(entry => entry.Storage == "Sparse" && entry.Mode == "Simd"));
        var recorded = ComparisonProvenance.Capture()["approaches"]!.AsArray();
        Assert.AreEqual(11, recorded.Select(entry => $"{entry!["framework"]}/{entry["storage"]}").Distinct().Count());
        Assert.AreEqual(7, recorded.Count(entry =>
            (string?)entry!["storage"] == "Sparse" && (string?)entry["mode"] == "Scalar" && (bool)entry["representative"]!));
    }

    /// <summary>Build metadata records package versions and identifies each external framework without separate manifests.</summary>
    [TestMethod]
    public void PackageVersionsCoverEveryExternalFramework()
    {
        var packages = ComparisonProvenance.Capture()["packages"]!.AsArray();
        Assert.AreEqual(packages.Count, packages.Select(entry => (string)entry!["package"]!).Distinct().Count());

        foreach (var package in packages)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace((string?)package!["package"]));
            Assert.IsFalse(string.IsNullOrWhiteSpace((string?)package["version"]));
        }

        foreach (var framework in ComparisonCatalog.Cases.Select(entry => entry.Framework).Distinct())
        {
            if (framework == "AlvorKit")
                continue;
            Assert.AreEqual(1, packages.Count(entry => (string?)entry!["framework"] == framework), framework);
        }
    }

    /// <summary>Invalid or incomparable observations fail before producing a misleading chart.</summary>
    [TestMethod]
    [DataRow("schema")]
    [DataRow("provenance")]
    [DataRow("operations")]
    [DataRow("elapsed")]
    [DataRow("empty")]
    [DataRow("unit")]
    [DataRow("duplicate")]
    [DataRow("unknown")]
    [DataRow("count")]
    [DataRow("mixed_counts")]
    [DataRow("padding")]
    [DataRow("normalization")]
    [DataRow("no_cases")]
    [DataRow("storage")]
    [DataRow("representative")]
    [DataRow("parallel")]
    [DataRow("identity")]
    [DataRow("no_alvorkit")]
    [DataRow("no_external")]
    public void RejectsInvalidMeasurements(string defect)
    {
        var document = Document();
        var entry = document["benchmarks"]![0]!;

        switch (defect)
        {
            case "no_alvorkit": document["ecsComparison"]!["approaches"]!.AsArray().RemoveAt(1); break;
            case "no_external": document["ecsComparison"]!["approaches"]!.AsArray().RemoveAt(0); break;
            case "parallel": document["ecsComparison"]!["approaches"]![0]!["mode"] = "Parallel"; break;
            case "identity": document["ecsComparison"]!["approaches"]![0]!["mode"] = "Identity"; break;
            case "storage": document["ecsComparison"]!["approaches"]![0]!["storage"] = "Sparse"; break;
            case "representative": document["ecsComparison"]!["approaches"]![0]!["representative"] = false; break;
            case "schema": document["schemaVersion"] = 1; break;
            case "provenance": document["ecsComparison"]!["schemaVersion"] = 0; break;
            case "operations": entry["samples"]![0]!["operationCount"] = 0; break;
            case "elapsed": entry["samples"]![0]!["elapsedNanoseconds"] = 0; break;
            case "empty": entry["samples"] = new JsonArray(); break;
            case "unit": entry["unit"] = "batch"; break;
            case "duplicate": document["benchmarks"]!.AsArray().Add(entry.DeepClone()); break;
            case "unknown": entry["id"] = "Create1/N100000/Padding0/Scalar/Unknown/Default"; break;
            case "count": entry["id"] = "Create1/N0/Padding0/Scalar/Arch/Default"; break;
            case "mixed_counts":
                var differentCount = entry.DeepClone();
                differentCount["id"] = "Create1/N200000/Padding0/Scalar/Arch/Default";
                differentCount["samples"]![0]!["operationCount"] = 200000;
                document["benchmarks"]!.AsArray().Add(differentCount);
                break;
            case "padding": entry["id"] = "Create1/N100000/Padding10/Scalar/Arch/Default"; break;
            case "normalization": entry["samples"]![0]!["operationCount"] = 1; break;
            case "no_cases": document["benchmarks"] = new JsonArray(); break;
        }

        Assert.ThrowsExactly<InvalidDataException>(() => ComparisonReport.Render(document.ToJsonString()));
    }

    /// <summary>Rendering a saved run cannot accidentally replace its source measurements.</summary>
    [TestMethod]
    public void RejectsOverwritingInput()
    {
        using var workspace = TempWorkspace.Create();
        var path = Path.Combine(workspace.Root, "results.json");
        File.WriteAllText(path, Document().ToJsonString());
        Assert.ThrowsExactly<ArgumentException>(() => ComparisonReport.Write(path, path));
        Assert.AreEqual(9, (int)JsonNode.Parse(File.ReadAllText(path))!["schemaVersion"]!);
    }

    private static JsonObject Document() => new()
    {
        ["schemaVersion"] = 9,
        ["suite"] = "AlvorKit.ECS.Bench.Comparison",
        ["ecsComparison"] = new JsonObject
        {
            ["schemaVersion"] = 2,
            ["approaches"] = new JsonArray(
                new JsonObject
                {
                    ["scenario"] = "Create1",
                    ["framework"] = "Arch",
                    ["mode"] = "Scalar",
                    ["variant"] = "Default",
                    ["storage"] = "",
                    ["representative"] = true,
                },
                new JsonObject
                {
                    ["scenario"] = "Create1",
                    ["framework"] = "AlvorKit",
                    ["mode"] = "Scalar",
                    ["variant"] = "ArchetypalReusedBuilder",
                    ["storage"] = "Archetypal",
                    ["representative"] = true,
                }),
        },
        ["benchmarks"] = new JsonArray(new JsonObject
        {
            ["id"] = "Create1/N100000/Padding0/Scalar/Arch/Default",
            ["unit"] = "Ent",
            ["samples"] = new JsonArray(new JsonObject { ["operationCount"] = 100000, ["elapsedNanoseconds"] = 100 }),
        }),
    };
}
