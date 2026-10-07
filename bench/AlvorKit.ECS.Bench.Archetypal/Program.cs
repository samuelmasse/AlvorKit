if (args.Length == 2 && args[0] == "--isolated-sample")
    return ArchIsolatedMeasurement.Sample(args[1]);

if (args.Length == 2 && args[0] == "--footprint")
    return ArchFootprint.Run(args[1]);
return BenchHost.Run<ArchBenchmarks>(args);
