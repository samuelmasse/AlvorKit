if (args is ["--footprint"])
{
    IndexedFootprint.Run();
    return 0;
}

return BenchHost.Run<IndexedBenchmarks>(args);
