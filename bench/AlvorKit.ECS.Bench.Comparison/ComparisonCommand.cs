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
        var provenance = ComparisonProvenance.Capture(ComparisonExperiments.All);
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
            if (args.Length > 0 && args[0] == "worker")
                return ComparisonStudy.Worker(args[1..]);

            if (args.Length > 0 && args[0] == "study")
                return ComparisonStudy.Run(args[1..]);

            if (args.Length > 0 && args[0] == "combine")
            {
                var inputs = new Argument<string[]>("results") { Arity = ArgumentArity.OneOrMore };
                var destination = new Option<string>("--output") { Required = true };
                var combine = new RootCommand("Publish one report; later inputs replace duplicate cases without pooling samples.")
                {
                    inputs, destination,
                };
                combine.SetAction(result => Console.WriteLine(ComparisonReportCollection.Write(
                    result.GetValue(inputs)!, result.GetValue(destination)!)));
                return combine.Parse(args[1..]).Invoke();
            }

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
                var baseline = new Option<FileInfo?>("--baseline") { Description = "Previous comparison JSON." };
                var command = new RootCommand("Render a saved ECS comparison without rerunning measurements.")
                {
                    input,
                    output,
                    baseline
                };
                command.SetAction(result =>
                {
                    var path = ComparisonReport.Write(result.GetValue(input)!.FullName, result.GetValue(output)?.FullName,
                        result.GetValue(baseline)?.FullName);
                    Console.WriteLine($"Report: {path}");
                });
                return command.Parse(args[1..]).Invoke();
            }
            return new BenchCommandLine(Execute).Invoke(args);
        }
        catch (Exception error) when (error is IOException or ArgumentException or InvalidDataException or JsonException
            or TimeoutException)
        {
            Console.Error.WriteLine($"Error: {error.Message}");
            return 2;
        }
    }
}
