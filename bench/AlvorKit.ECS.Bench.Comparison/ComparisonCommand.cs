namespace AlvorKit;

internal static class ComparisonCommand
{
    private static int Execute(BenchCommand command)
    {
        if (command.Kind == BenchCommandKind.List)
            return BenchHost.Run<ComparisonBenchmarks>(command);
        var stdout = command.JsonPath == "-";
        var jsonPath = stdout || command.JsonPath == null
            ? Path.Combine("out", "bench", "ecs-comparison", DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss-fff"), "results.json")
            : command.JsonPath;
        var provenance = ComparisonProvenance.Capture();
        var display = stdout ? command.Display with { Quiet = true } : command.Display;
        var status = BenchHost.Run<ComparisonBenchmarks>(command with { JsonPath = jsonPath, Display = display });

        if (status != 0)
            return status;
        var document = JsonNode.Parse(File.ReadAllText(jsonPath))!.AsObject();
        document["ecsComparison"] = provenance;
        File.WriteAllText(jsonPath, document.ToJsonString(ComparisonReport.Options));
        var path = ComparisonReport.Write(jsonPath, null);

        if (stdout)
        {
            Console.WriteLine(document.ToJsonString(ComparisonReport.Options));
            Console.Error.WriteLine($"Report: {path}");
        }
        else Console.WriteLine($"Report: {path}");
        return 0;
    }

    internal static int Run(string[] args)
    {
        try
        {
            if (args.Length > 0 && args[0] == "report")
            {
                var input = new Argument<FileInfo>("results")
                {
                    Description = "Saved comparison JSON, including its provenance."
                };
                var output = new Option<FileInfo?>("--output")
                {
                    Description = "HTML path; defaults to the JSON filename with .html."
                };
                var command = new RootCommand("Render a saved ECS comparison without rerunning measurements.")
                {
                    input,
                    output
                };
                command.SetAction(result =>
                {
                    var path = ComparisonReport.Write(result.GetValue(input)!.FullName, result.GetValue(output)?.FullName);
                    Console.WriteLine($"Report: {path}");
                });
                return command.Parse(args[1..]).Invoke();
            }
            return new BenchCommandLine(Execute).Invoke(args);
        }
        catch (Exception error) when (error is IOException or ArgumentException or InvalidDataException or JsonException)
        {
            Console.Error.WriteLine($"Error: {error.Message}");
            return 2;
        }
    }
}
