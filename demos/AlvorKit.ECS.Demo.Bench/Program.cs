return EcsBenchOptions.CreateRootCommand(options =>
{
    if (options.WorkerCase is not null)
        return new EcsArchBenchWorker(options).Run();

    if (options.Suite == EcsBenchOptions.CoreSuite && !options.List)
    {
        // Preserve the original core suite's console and JSON surface.
        var demo = new EcsBenchDemo(options);
        demo.PrintEnvironment();
        var results = demo.MeasureAll();
        demo.PrintResults(results);
        demo.WriteJson(results);
        return 0;
    }

    return new EcsBenchCoordinator(options).Run();
}).Parse(args).Invoke(new() { EnableDefaultExceptionHandler = false });
