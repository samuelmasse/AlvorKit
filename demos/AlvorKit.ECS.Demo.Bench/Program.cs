// Parse a small set of knobs so the same executable can do quick agent checks or slower local sweeps.
var options = EcsBenchOptions.Parse(args);
if (options.ShowHelp)
{
    EcsBenchOptions.PrintHelp();
    return 0;
}

// Measure each ECS path independently and keep the output plain enough to paste into reviews.
var demo = new EcsBenchDemo(options);
demo.PrintEnvironment();
var results = demo.MeasureAll();
demo.PrintResults(results);
demo.WriteJson(results);
return 0;
