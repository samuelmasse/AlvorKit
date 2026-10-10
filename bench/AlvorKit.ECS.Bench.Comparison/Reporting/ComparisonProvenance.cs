namespace AlvorKit;

internal static class ComparisonProvenance
{
    private static JsonArray Packages()
    {
        var packages = new JsonArray();

        foreach (var attribute in typeof(ComparisonProvenance).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
        {
            if (!attribute.Key.StartsWith("EcsPackage:"))
                continue;
            var value = attribute.Value!.Split('|');
            packages.Add(new JsonObject
            {
                ["package"] = attribute.Key["EcsPackage:".Length..],
                ["version"] = value[0],
                ["framework"] = value[1],
            });
        }
        return packages;
    }

    private static string Git(string root, params string[] args)
    {
        var start = new ProcessStartInfo("git")
        {
            WorkingDirectory = root,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        foreach (var argument in args)
            start.ArgumentList.Add(argument);
        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEndAsync();
        var errors = process.StandardError.ReadToEndAsync();

        if (!process.WaitForExit(10000))
        {
            process.Kill(true);
            throw new IOException("Git provenance capture timed out.");
        }
        if (process.ExitCode != 0)
            throw new IOException(errors.GetAwaiter().GetResult());
        return output.GetAwaiter().GetResult();
    }

    internal static JsonObject Capture(IEnumerable<ComparisonExperiment> experiments)
    {
        var root = ProjectRoot.FindFromCurrentProcess(typeof(ComparisonProvenance), requireResDirectory: true);
        var cpu = OperatingSystem.IsWindows()
            ? Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\CentralProcessor\0",
                "ProcessorNameString", null)?.ToString()?.Trim()
            : null;
        return new JsonObject
        {
            ["schemaVersion"] = 3,
            ["cpu"] = cpu,
            ["revision"] = Git(root, "rev-parse", "HEAD").Trim(),
            ["workingTree"] = Git(root, "status", "--short"),
            ["packages"] = Packages(),
            ["experiments"] = JsonSerializer.SerializeToNode(experiments.Select(entry => entry.Metadata), ComparisonReport.Options),
            ["sources"] = ComparisonSources.Capture(experiments),
            ["sourceDiff"] = Git(root, "diff", "--binary", "HEAD", "--", "bench/AlvorKit.ECS.Bench.Comparison",
                "src/AlvorKit.Bench", "src/AlvorKit.ECS", "src/AlvorKit.ECS.Indexed", "src/AlvorKit.ECS.Generator",
                "res/templates/ecs", "res/templates/ecs-comparison", "tests/AlvorKit.ECS.Bench.Comparison.Test"),
            ["vector128"] = Vector128.IsHardwareAccelerated,
            ["vector256"] = Vector256.IsHardwareAccelerated,
            ["vector512"] = Vector512.IsHardwareAccelerated,
            ["assemblyModuleId"] = typeof(ComparisonProvenance).Module.ModuleVersionId.ToString(),
            ["ecsAssemblyModuleIds"] = new JsonObject
            {
                ["AlvorKit.ECS"] = typeof(Ent).Module.ModuleVersionId.ToString(),
                ["AlvorKit.ECS.Indexed"] = typeof(EntPtrIdx).Module.ModuleVersionId.ToString(),
            },
            ["runtimeOverrides"] = RuntimeOverrides(),
        };
    }

    private static JsonObject RuntimeOverrides()
    {
        var result = new JsonObject();

        foreach (var prefix in new[] { "DOTNET_", "COMPlus_" })
        {
            foreach (var name in new[]
                { "TieredCompilation", "TieredPGO", "ReadyToRun", "gcServer", "GCHeapHardLimit",
                    "EnableHWIntrinsic", "EnableAVX2", "EnableAVX512F" })
                result[prefix + name] = Environment.GetEnvironmentVariable(prefix + name);
        }

        return result;
    }
}
