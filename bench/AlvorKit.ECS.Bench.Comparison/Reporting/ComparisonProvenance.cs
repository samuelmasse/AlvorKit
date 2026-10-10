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

    internal static JsonObject Capture()
    {
        var root = ProjectRoot.FindFromCurrentProcess(typeof(ComparisonProvenance), requireResDirectory: true);
        var cpu = OperatingSystem.IsWindows()
            ? Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\CentralProcessor\0",
                "ProcessorNameString", null)?.ToString()?.Trim()
            : null;
        return new JsonObject
        {
            ["schemaVersion"] = 2,
            ["cpu"] = cpu,
            ["revision"] = Git(root, "rev-parse", "HEAD").Trim(),
            ["workingTree"] = Git(root, "status", "--short"),
            ["packages"] = Packages(),
            ["approaches"] = JsonSerializer.SerializeToNode(ComparisonCatalog.Cases.Select(entry => new
            {
                entry.Scenario,
                entry.Framework,
                entry.Storage,
                entry.Variant,
                entry.Mode,
                entry.Representative,
            }), ComparisonReport.Options),
        };
    }
}
