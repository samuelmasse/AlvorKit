return EcsBenchOptions.CreateRootCommand(options =>
{
    // Measure each ECS path independently and keep the output plain enough to paste into reviews.
    var demo = new EcsBenchDemo(options);
    demo.PrintEnvironment();
    var results = demo.MeasureAll();
    demo.PrintResults(results);
    demo.WriteJson(results);
    return 0;
}).Parse(args).Invoke(new() { EnableDefaultExceptionHandler = false });
