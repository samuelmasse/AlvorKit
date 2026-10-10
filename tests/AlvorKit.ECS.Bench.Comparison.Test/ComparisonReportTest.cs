namespace AlvorKit;

[TestClass]
public class ComparisonReportTest
{
    /// <summary>Every case has unique explicit metadata and exactly one representative for its comparison row.</summary>
    [TestMethod]
    public void ContractsMatchRunnableCatalog()
    {
        var experiments = ComparisonExperiments.All;
        var catalog = new BenchCatalogBuilder().Create(new ComparisonBenchmarks().Create());
        Assert.AreEqual(experiments.Length, catalog.Cases.Length);
        Assert.AreEqual(experiments.Length, experiments.Select(entry => entry.Id).Distinct().Count());
        CollectionAssert.AreEquivalent(experiments.Select(entry => entry.Id).ToArray(), catalog.Cases.Select(entry => entry.Id).ToArray());
        Assert.IsTrue(experiments.All(entry => entry.Input.Count == 100000));
        var originalIds = ComparisonCatalog.Cases.SelectMany(entry =>
            (entry.Scenario.StartsWith("Update") ? new[] { 0, 10 } : [0]).Select(padding => entry.Id(100000, padding))).ToArray();
        CollectionAssert.AreEquivalent(originalIds, catalog.Cases.Select(entry => entry.Id).ToArray());
        CollectionAssert.AreEquivalent(new[] { "Create1", "Create2", "Create3", "Update1", "Update2", "Update3", "Mixed" },
            experiments.Select(entry => entry.Scenario).Distinct().ToArray());

        foreach (var group in experiments.GroupBy(entry => (entry.Scenario, entry.Framework, entry.Storage, entry.Mode, entry.Input)))
            Assert.AreEqual(1, group.Count(entry => entry.Representative), group.Key.ToString());

        Assert.IsTrue(experiments.All(entry => entry.Operations > 0));
    }

    /// <summary>Every source snapshot identifies the concrete workload between the timer boundaries.</summary>
    [TestMethod]
    public void SourceSnapshotsContainActualTimedCalls()
    {
        var experiments = ComparisonExperiments.All.DistinctBy(entry => entry.Workload).ToArray();
        var sources = ComparisonSources.Capture(experiments);

        foreach (var entry in experiments)
        {
            var source = sources["locations"]![entry.Id]!;
            var workload = source["workload"]!;
            var measurement = source["measurement"]!;
            var workloadText = (string)sources["files"]![(string)workload["path"]!]!;
            var measurementText = (string)sources["files"]![(string)measurement["path"]!]!;
            StringAssert.Contains(workloadText.Split('\n')[(int)workload["line"]! - 1], " Run(");
            var body = string.Join('\n', measurementText.Split('\n').Skip((int)measurement["line"]!)
                .TakeWhile(line => line.TrimEnd('\r') != "    }"));
            var call = body.IndexOf($"{entry.Workload.Name}.Run(", StringComparison.Ordinal);
            Assert.IsTrue(call > body.IndexOf("BenchTimer.Start()", StringComparison.Ordinal), entry.Id);
            Assert.IsTrue(call < body.IndexOf("timer.Stop(", StringComparison.Ordinal), entry.Id);
        }
    }

    /// <summary>Runtime and generator changes remain attributable even when the benchmark loop itself is unchanged.</summary>
    [TestMethod]
    public void ProvenanceIncludesEcsImplementation()
    {
        var provenance = ComparisonProvenance.Capture([]);
        var files = provenance["sources"]!["files"]!;
        var indexed = (string)files["src/AlvorKit.ECS.Indexed/Ent/EntMutIdx.cs"]!;
        StringAssert.Contains(indexed, "public T? GetArchetypal<T, N, A>()");
        Assert.IsNotNull(files["src/AlvorKit.ECS/Archetypal/EntMut.Archetypal.cs"]);
        Assert.IsNotNull(files["src/AlvorKit.ECS.Generator/ComponentSourceEmitter.cs"]);
        Assert.IsNotNull(files["res/templates/ecs/source-generator/get-property-archetypal.csfrag.tmpl"]);
        Assert.AreEqual(typeof(Ent).Module.ModuleVersionId.ToString(),
            (string)provenance["ecsAssemblyModuleIds"]!["AlvorKit.ECS"]!);
        Assert.AreEqual(typeof(EntPtrIdx).Module.ModuleVersionId.ToString(),
            (string)provenance["ecsAssemblyModuleIds"]!["AlvorKit.ECS.Indexed"]!);
    }

    /// <summary>Saved source and metadata cannot break out of their JSON script element.</summary>
    [TestMethod]
    public void EmbeddedDataIsEscapedAndPreserved()
    {
        var document = Document();
        const string injection = "</script><script>alert('benchmark')</script>";
        document["note"] = injection;
        var html = ComparisonReport.Render(document.ToJsonString());
        Assert.IsFalse(html.Contains(injection));
        const string marker = "<script type=\"application/json\" id=\"data\">";
        var start = html.IndexOf(marker, StringComparison.Ordinal) + marker.Length;
        var end = html.IndexOf("</script>", start, StringComparison.Ordinal);
        Assert.IsTrue(JsonNode.DeepEquals(document, JsonNode.Parse(html[start..end])));
    }

    /// <summary>HTML keeps exact samples, sources, and per-launch drift inputs without embedding duplicate measurements.</summary>
    [TestMethod]
    public void EmbeddedDataPreservesChartInputs()
    {
        var first = Document();
        var second = Document();
        var warmups = new JsonArray();

        for (var number = 1; number <= 7; number++)
        {
            for (var launch = 0; launch < 2; launch++)
                warmups.Add(new JsonObject { ["number"] = number, ["launch"] = launch, ["elapsedNanoseconds"] = number * 100 });
        }

        second["benchmarks"]![0]!["warmups"] = warmups;
        var combined = ComparisonReportCollection.Combine([first, second]);
        var original = combined.ToJsonString();
        var view = EmbeddedData(ComparisonReport.Render(original));
        var expected = warmups.Where(sample => (int)sample!["number"]! >= 5)
            .OrderBy(sample => (int)sample!["launch"]!).Select(sample => sample!.DeepClone()).ToArray();
        Assert.IsTrue(JsonNode.DeepEquals(new JsonArray(expected), view["benchmarks"]![0]!["warmups"]));
        Assert.IsTrue(JsonNode.DeepEquals(combined["benchmarks"]![0]!["samples"], view["benchmarks"]![0]!["samples"]));
        Assert.AreEqual(1, (int)view["benchmarks"]![0]!["reportRun"]!);

        for (var index = 0; index < 2; index++)
        {
            Assert.IsNull(view["reportRuns"]![index]!["benchmarks"]);
            Assert.IsTrue(JsonNode.DeepEquals(combined["reportRuns"]![index]!["ecsComparison"]!["sources"],
                view["reportRuns"]![index]!["ecsComparison"]!["sources"]));
        }

        Assert.AreEqual(original, combined.ToJsonString());
    }

    /// <summary>Writing a small HTML view leaves the full JSON intact and links to its actual relative path.</summary>
    [TestMethod]
    public void HtmlLinksUnmodifiedRawMeasurements()
    {
        using var workspace = TempWorkspace.Create();
        var raw = Path.Combine(workspace.Root, "measurements #1.json");
        var output = Path.Combine(workspace.Root, "view", "comparison.html");
        var json = Document().ToJsonString();
        File.WriteAllText(raw, json);
        ComparisonReport.Write(raw, output);
        Assert.AreEqual(json, File.ReadAllText(raw));
        Assert.AreEqual("../measurements #1.json", (string)EmbeddedData(File.ReadAllText(output))["rawDataPath"]!);
    }

    private static JsonNode EmbeddedData(string html)
    {
        const string marker = "<script type=\"application/json\" id=\"data\">";
        var start = html.IndexOf(marker, StringComparison.Ordinal) + marker.Length;
        var end = html.IndexOf("</script>", start, StringComparison.Ordinal);
        return JsonNode.Parse(html[start..end])!;
    }

    /// <summary>Update padding and operation counts use their recorded contracts without path parsing.</summary>
    [TestMethod]
    public void PaddedUpdateContractsRemainComparable()
    {
        var document = Document();
        var update = ComparisonExperiments.All.First(entry => entry.Scenario == "Update2" && entry.Input.Padding == 10);
        document["ecsComparison"]!["experiments"]!.AsArray().Add(JsonSerializer.SerializeToNode(update.Metadata, ComparisonReport.Options));
        document["benchmarks"]!.AsArray().Add(Measurement(update));
        ComparisonReport.Validate(document);
    }

    /// <summary>Invalid durations, normalization, identity, and unsupported schemas fail before chart generation.</summary>
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
    [DataRow("no_cases")]
    [DataRow("duplicate_contract")]
    public void RejectsInvalidMeasurements(string defect)
    {
        var document = Document();
        var entry = document["benchmarks"]![0]!;

        switch (defect)
        {
            case "schema": document["schemaVersion"] = 1; break;
            case "provenance": document["ecsComparison"]!["schemaVersion"] = 0; break;
            case "operations": entry["samples"]![0]!["operationCount"] = 1; break;
            case "elapsed": entry["samples"]![0]!["elapsedNanoseconds"] = 0; break;
            case "empty": entry["samples"] = new JsonArray(); break;
            case "unit": entry["unit"] = "batch"; break;
            case "duplicate": document["benchmarks"]!.AsArray().Add(entry.DeepClone()); break;
            case "unknown": entry["id"] = "unknown"; break;
            case "count": document["ecsComparison"]!["experiments"]![0]!["input"]!["count"] = 0; break;
            case "no_cases": document["benchmarks"] = new JsonArray(); break;
            case "duplicate_contract":
                var contracts = document["ecsComparison"]!["experiments"]!.AsArray();
                contracts.Add(contracts[0]!.DeepClone());
                break;
        }

        Assert.ThrowsExactly<InvalidDataException>(() => ComparisonReport.Render(document.ToJsonString()));
    }

    /// <summary>Rendering cannot overwrite the raw input artifact.</summary>
    [TestMethod]
    public void RejectsOverwritingInput()
    {
        using var workspace = TempWorkspace.Create();
        var path = Path.Combine(workspace.Root, "results.json");
        File.WriteAllText(path, Document().ToJsonString());
        Assert.ThrowsExactly<ArgumentException>(() => ComparisonReport.Write(path, path));
        Assert.ThrowsExactly<ArgumentException>(() => ComparisonReport.Write(path, path, path));
    }

    /// <summary>Independent-launch estimates must agree with complete, correctly identified raw launches.</summary>
    [TestMethod]
    [DataRow("identity")]
    [DataRow("missing")]
    [DataRow("estimate")]
    public void RejectsInvalidLaunchAggregation(string defect)
    {
        var document = Document();
        var entry = document["benchmarks"]![0]!;
        var sample = entry["samples"]![0]!;
        sample["launch"] = 0;
        var second = sample.DeepClone();
        second["launch"] = 1;
        entry["samples"]!.AsArray().Add(second);
        document["study"] = new JsonObject { ["launches"] = 2 };
        document["selection"] = new JsonObject { ["sampleCount"] = 1 };
        var mean = (double)sample["elapsedNanoseconds"]! / (int)sample["operationCount"]!;
        entry["launchEstimate"] = JsonSerializer.SerializeToNode(BenchStatistics.Estimate([mean, mean]), ComparisonReport.Options);
        ComparisonReport.Validate(document);

        switch (defect)
        {
            case "identity": second["launch"] = 2; break;
            case "missing": entry["samples"]!.AsArray().RemoveAt(1); break;
            case "estimate": entry["launchEstimate"]!["mean"] = 999d; break;
        }

        Assert.ThrowsExactly<InvalidDataException>(() => ComparisonReport.Validate(document));
    }

    /// <summary>Calibration changes work duration without changing the population, padding, or operation unit.</summary>
    [TestMethod]
    public void CalibrationPreservesComparableInput()
    {
        var experiment = ComparisonExperiments.All.First(entry => entry.Scenario == "Update2");
        var calibrated = experiment.WithPasses(experiment.Input.Passes * 2);
        Assert.AreEqual(experiment.Input with { Passes = experiment.Input.Passes * 2 }, calibrated.Input);
        Assert.AreEqual(experiment.Id, calibrated.Id);
        Assert.AreEqual(experiment.Operations * 2, calibrated.Operations);
        Assert.AreEqual(experiment.Unit, calibrated.Unit);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => experiment.WithPasses(0));
    }

    /// <summary>A near-empty query cannot inflate a full scan into hundreds of seconds of repeated warmup.</summary>
    [TestMethod]
    [DataRow(0.02, 0.1, 2, 1000, 100)]
    [DataRow(0.001, 1, 2, 1000, 20)]
    [DataRow(0.001, 1, 2, 7, 7)]
    [DataRow(3, 30, 2, 1000, 1)]
    public void CalibrationBoundsSlowStrategies(double fastest, double slowest, double target, int capacity, int expected)
    {
        Assert.AreEqual(expected, ComparisonCalibration.Multiplier(fastest, slowest, target, capacity));
    }

    /// <summary>Package provenance comes from the pinned build references for all external frameworks.</summary>
    [TestMethod]
    public void PackageVersionsCoverEveryExternalFramework()
    {
        var packages = ComparisonProvenance.Capture([])["packages"]!.AsArray();

        foreach (var framework in ComparisonCatalog.Cases.Select(entry => entry.Framework).Distinct().Where(name => name != "AlvorKit"))
            Assert.AreEqual(1, packages.Count(entry => (string?)entry!["framework"] == framework), framework);
    }

    private static JsonObject Document()
    {
        var entry = ComparisonExperiments.All.First(item => item.Id == "Create1/N100000/Padding0/Scalar/Arch/Default");
        return new JsonObject
        {
            ["schemaVersion"] = 9,
            ["suite"] = "AlvorKit.ECS.Bench.Comparison",
            ["ecsComparison"] = new JsonObject
            {
                ["schemaVersion"] = 3,
                ["experiments"] = new JsonArray(JsonSerializer.SerializeToNode(entry.Metadata, ComparisonReport.Options)),
                ["sources"] = ComparisonSources.Capture([entry]),
            },
            ["benchmarks"] = new JsonArray(Measurement(entry)),
        };
    }

    /// <summary>Combining keeps the selected run's raw samples and source instead of pooling duplicate measurements.</summary>
    [TestMethod]
    public void CollectionPreservesCaseOrigins()
    {
        var first = Document();
        var second = Document();
        second["benchmarks"]![0]!["samples"]![0]!["elapsedNanoseconds"] = 250d;
        second["ecsComparison"]!["sources"]!["note"] = "second source snapshot";
        var combined = ComparisonReportCollection.Combine([first, second]);
        Assert.AreEqual(1, combined["benchmarks"]!.AsArray().Count);
        Assert.AreEqual(1, combined["benchmarks"]![0]!["samples"]!.AsArray().Count);
        Assert.AreEqual(250d, (double)combined["benchmarks"]![0]!["samples"]![0]!["elapsedNanoseconds"]!);
        Assert.AreEqual(1, (int)combined["benchmarks"]![0]!["reportRun"]!);
        Assert.AreEqual("second source snapshot", (string)combined["reportRuns"]![1]!["ecsComparison"]!["sources"]!["note"]!);
        Assert.IsTrue(JsonNode.DeepEquals(first, combined["reportRuns"]![0]));
        ComparisonReport.Render(combined.ToJsonString());

        combined["benchmarks"]![0]!["samples"]![0]!["elapsedNanoseconds"] = 300d;
        Assert.ThrowsExactly<InvalidDataException>(() => ComparisonReport.Validate(combined));
    }

    /// <summary>Different environments and cold runs cannot be presented as one warmed comparison.</summary>
    [TestMethod]
    [DataRow("cpu")]
    [DataRow("packages")]
    [DataRow("runtimeOverrides")]
    public void CollectionRejectsDifferentEnvironment(string key)
    {
        var first = Document();
        var second = Document();
        second["ecsComparison"]![key] = "different";
        Assert.ThrowsExactly<InvalidDataException>(() => ComparisonReportCollection.Combine([first, second]));
    }

    /// <summary>Combined reports cannot recursively duplicate runs or overwrite their source JSON.</summary>
    [TestMethod]
    public void CollectionRejectsNestedInputAndOverwrite()
    {
        var first = Document();
        var second = Document();
        var combined = ComparisonReportCollection.Combine([first, second]);
        Assert.ThrowsExactly<InvalidDataException>(() => ComparisonReportCollection.Combine([first, combined]));
        second["study"] = new JsonObject { ["state"] = "ProcessCold", ["launches"] = 1 };
        second["selection"] = new JsonObject { ["sampleCount"] = 1 };
        var sample = second["benchmarks"]![0]!["samples"]![0]!;
        sample["launch"] = 0;
        var mean = (double)sample["elapsedNanoseconds"]! / (int)sample["operationCount"]!;
        second["benchmarks"]![0]!["launchEstimate"] =
            JsonSerializer.SerializeToNode(BenchStatistics.Estimate([mean]), ComparisonReport.Options);
        Assert.ThrowsExactly<InvalidDataException>(() => ComparisonReportCollection.Combine([first, second]));
        using var workspace = TempWorkspace.Create();
        var path = Path.Combine(workspace.Root, "results.json");
        File.WriteAllText(path, first.ToJsonString());
        Assert.ThrowsExactly<ArgumentException>(() => ComparisonReportCollection.Write([path, path], path));
    }

    private static JsonObject Measurement(ComparisonExperiment entry) => new()
    {
        ["id"] = entry.Id,
        ["unit"] = entry.Unit,
        ["samples"] = new JsonArray(new JsonObject { ["operationCount"] = entry.Operations, ["elapsedNanoseconds"] = 100d }),
    };
}
