return RangeBenchOptions.CreateRootCommand(options =>
{
    // Keep each allocator pattern separate so fragmentation, reuse, and compaction
    // costs can be compared directly from the printed table.
    var demo = new RangeBenchDemo(options);
    demo.PrintEnvironment();
    var results = demo.MeasureAll();
    demo.PrintResults(results);
    demo.WriteJson(results);
    return 0;
}).Parse(args).Invoke(new() { EnableDefaultExceptionHandler = false });
