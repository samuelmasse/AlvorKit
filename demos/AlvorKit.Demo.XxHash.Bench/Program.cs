// Build the demo from the upstream-style sweep flags, generated xxHash backend,
// candidate native entry points, and exact input sizes this run will measure.
var demo = XxHashBenchDemo.Create(args);

// Each table is measured independently so the demo mirrors the upstream split
// between large-input bandwidth and small-input throughput or latency.
demo.PrintEnvironment();
var bandwidth = demo.MeasureLargeBandwidth();
demo.MeasureSmallFixedThroughput();
var smallVelocity = demo.MeasureSmallRandomThroughput();
demo.MeasureSmallFixedLatency();
demo.MeasureSmallRandomLatency();

// Finish with the same measured-vs-published comparison a reader can use to
// reason about P/Invoke overhead and hardware-bound hashing throughput.
demo.PrintSummary(bandwidth, smallVelocity);
return 0;
