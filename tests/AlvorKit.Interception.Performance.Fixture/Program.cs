using AlvorKit.Interception.Performance.Fixture;

var report = InterceptionPerformanceFixture.Run();
InterceptionPerformanceOutput.Print(report);
InterceptionPerformanceFixture.AssertAllocationInvariants(report);
